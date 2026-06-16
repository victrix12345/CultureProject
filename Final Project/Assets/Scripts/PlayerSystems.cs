using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerSystems : MonoBehaviour
{
    private Animator animController;
    private static WaitForSeconds 
        _waitForSeconds0_1 = new WaitForSeconds(0.1f),
        _waitForSeconds2 = new WaitForSeconds(2f), 
        _waitForSeconds1 = new WaitForSeconds(1f);
    private InputSystem_Actions inputActions;
    private CharacterController charCon;
    private const float
        grav = (-9.81f * 1.5f),
        jumpHeight = 2f,
        recoilRecoveryRate = 8f,
        patternResetDelay = 0.4f,
        baseRecoilPitch = 0.005f,
        pitchEscelation = 0.005f,
        maxPitch = 0.02f,
        yawSpread = 0.03f,
        yawEscalation = 0.01f;
    private float
        mouseSens = 0.5f,
        speed,
        yawDirection = 0f,
        timeSinceShot = 0f;
    private Vector3 playerVel;
    private Vector2
        movement,
        look,
        recoilOffset = Vector2.zero;
    private bool 
        grounded = true, 
        jumped = false, 
        sneaked = false, 
        shooting = false, 
        targetted = false, 
        reloadInput = false, 
        reloading = false;
    public GameObject cam, shootPoint;
    private int 
        mapLayerMask,
        storedMag = 10, 
        currentAmmo = 20,
        shotIndex = 0;
    private LineRenderer lineRenderer;
    public TextMeshProUGUI ammo;
    void Awake()
    {
        UpdateAmmoUI();
        mapLayerMask = LayerMask.GetMask("Map");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        animController = GetComponent<Animator>();

        inputActions = new InputSystem_Actions();
        charCon = GetComponent<CharacterController>();

        inputActions.Player.Move.performed += ctx =>
        {
            movement = ctx.ReadValue<Vector2>();
            animController.SetBool("isWalking", true);
        };
        inputActions.Player.Move.canceled += _ =>
        {
            movement = Vector2.zero;
            animController.SetBool("isWalking", false);
        };
        inputActions.Player.Jump.performed += _ => jumped = true;
        inputActions.Player.Jump.canceled += _ => jumped = false;

        inputActions.Player.Sprint.performed += _ => sneaked = true;
        inputActions.Player.Sprint.canceled += _ => sneaked = false;

        inputActions.Player.Look.performed += ctx => look = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += _ => look = Vector2.zero;

        inputActions.Player.Attack.performed += _ => shooting = true;
        inputActions.Player.Attack.canceled += _ => shooting = false;

        inputActions.Player.Interact.performed += _ => reloadInput = true;
        inputActions.Player.Interact.canceled += _ => reloadInput = false;

        GameObject lineObj = new GameObject("RayVisualizer");
        lineObj.transform.SetParent(transform);
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
    }
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    public IEnumerator Jump()
    {
        playerVel.y = Mathf.Sqrt(jumpHeight * -2 * grav);
        yield return _waitForSeconds1;
    }
    private void FixedUpdate()
    {
        grounded = charCon.isGrounded;

        Vector3 newMove = movement.x * transform.right + movement.y * transform.forward;
        newMove = Vector3.ClampMagnitude(newMove, 1);

        if (jumped && grounded) StartCoroutine(Jump());

        if (grounded && !jumped && playerVel.y < -2) playerVel.y = -2;

        playerVel.y += grav * Time.deltaTime;

        speed = sneaked ? 3.5f : 7;

        Vector3 endMove = (newMove * speed) + (Vector3.up * playerVel.y);
        charCon.Move(endMove * Time.deltaTime);
    }
    private void Update()
    {
        CameraCalc();

        if (shooting && !targetted && currentAmmo > 0) StartCoroutine(Shooting()); 
        if (!shooting && !targetted && reloadInput && storedMag > 0 && !reloading) StartCoroutine(Reload());

        HandleRecoilRecovery();
    }
    private void ApplyRecoil()
    {
        float pitch = Mathf.Min(baseRecoilPitch + (pitchEscelation * shotIndex), maxPitch);

        pitch += Random.Range(0.08f, 0.12f);

        float maxYaw = yawSpread + (yawEscalation * shotIndex);

        yawDirection = Mathf.Clamp(yawDirection + Random.Range(-0.15f, 0.15f), -0.3f, 0.3f);
        float yaw = yawDirection * Random.Range(0.1f, maxYaw);

        recoilOffset += new Vector2(pitch, yaw);
        shotIndex++;
        timeSinceShot = 0f;
    }
    private void HandleRecoilRecovery()
    {
        if (!shooting)
        {
            timeSinceShot += Time.deltaTime;
            if (timeSinceShot > patternResetDelay)
            {
                shotIndex = 0;
                yawDirection = 0f;
            }
        }
        recoilOffset = Vector2.Lerp(recoilOffset, Vector2.zero, recoilRecoveryRate * Time.deltaTime);
    }
    IEnumerator Shooting()
    {
        targetted = true;
        animController.SetTrigger("TriggerShoot");
        RaycastHit aimInfo = new();
        bool isHit = Physics.Raycast(cam.transform.position, cam.transform.forward, out aimInfo, 30f);
        Vector3 targetPoint = isHit ? aimInfo.point : cam.transform.position + cam.transform.forward * 30f;
        Vector3 barrelDir = (targetPoint - shootPoint.transform.position).normalized;
        float barrelDist = (shootPoint.transform.position - targetPoint).magnitude;
        bool mapHit = Physics.Raycast(shootPoint.transform.position, barrelDir, barrelDist, mapLayerMask);

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, shootPoint.transform.position);
        lineRenderer.SetPosition(1, targetPoint);
        lineRenderer.startColor = !mapHit && isHit ? Color.green : Color.red;
        lineRenderer.endColor = !mapHit && isHit ? Color.green : Color.red;

        if (isHit && !mapHit)
        {
            if (aimInfo.collider.TryGetComponent<HealthSystem>(out var targetHealth))
            {
                int damage = Random.Range(30, 40);
                targetHealth.DealDamage(damage);
                Debug.Log($"Dealt {damage} damage to {aimInfo.collider.name}");
            }
        }
        ApplyRecoil();
        currentAmmo--;
        UpdateAmmoUI();
        yield return _waitForSeconds0_1;
        lineRenderer.enabled = false;
        targetted = false;
    }
    private void OnDestroy()
    {
        inputActions?.Dispose();
    }
    private void CameraCalc()
    { 
        Vector3 facing = gameObject.transform.localEulerAngles;
        Vector3 facingCam = cam.transform.localEulerAngles;
        look *= mouseSens;
        facing.y += look.x + recoilOffset.y;
        if (facingCam.x >= 270) facingCam.x -= 360;
        facingCam.x = Mathf.Clamp(facingCam.x - look.y - recoilOffset.x, -70f, 70f);
        facingCam.x -= look.y;
        transform.localEulerAngles = new(0, facing.y, 0);
        cam.transform.localEulerAngles = new(facingCam.x, 0, 0);
    }
    public IEnumerator Reload()
    {
        reloading = true;
        yield return _waitForSeconds2;
        currentAmmo = 20;
        storedMag--;
        UpdateAmmoUI();
        reloading = false;
    }
    private void UpdateAmmoUI() => ammo.text = $"{currentAmmo}/{storedMag}";
}