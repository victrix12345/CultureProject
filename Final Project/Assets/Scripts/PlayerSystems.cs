using System.Collections;
using UnityEngine;
using TMPro;

public class PlayerSystems : MonoBehaviour
{
    private static WaitForSeconds 
        _waitForSeconds0_1 = new WaitForSeconds(0.1f),
        _waitForSeconds0_5 = new WaitForSeconds(0.5f), 
        _waitForSeconds1 = new WaitForSeconds(1f);
    private InputSystem_Actions inputActions;
    private CharacterController charCon;
    private const float grav = (-9.81f * 1.5f), jumpHeight = 2f;
    private float speed, recoilMultiplier = 0;
    private Vector3 playerVel;
    private Vector2 movement, look, recoil;
    private bool 
        grounded = true, 
        jumped = false, 
        sneaked = false, 
        shooting = false, 
        targetted = false, 
        reloadInput = false, 
        reloading = false;
    public GameObject cam, shootPoint;
    private int mapLayerMask, storedMag = 2, currentAmmo = 20;
    private LineRenderer lineRenderer;
    public TextMeshProUGUI ammo;
    void Awake()
    {
        UpdateAmmoUI();
        mapLayerMask = LayerMask.GetMask("Map");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputActions = new InputSystem_Actions();
        charCon = GetComponent<CharacterController>();

        inputActions.Player.Move.performed += ctx => movement = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += _ => movement = Vector2.zero;

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
    private void OnEnable() // GOOD HABIT PART 1
    {
        inputActions.Player.Enable();
    }
    private void OnDisable() // GOOD HABIT PART 2
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

        if (shooting && !targetted && currentAmmo > 0)
        {
            StartCoroutine(Shooting());
            StartCoroutine(RecoilOverTime());
        }
        if (!shooting && !targetted && reloadInput && storedMag > 0 && !reloading) StartCoroutine(Reload());
    }
    IEnumerator Shooting()
    {
        targetted = true;
        RaycastHit aimInfo = new();
        bool isHit = Physics.Raycast(cam.transform.position, cam.transform.forward, out aimInfo, 30f);
        Vector3 targetPoint = isHit ? aimInfo.point : cam.transform.position + cam.transform.forward * 30f;
        Vector3 barrelDir = (targetPoint - shootPoint.transform.position).normalized;
        float barrelDist = (shootPoint.transform.position - targetPoint).magnitude;
        bool mapHit = Physics.Raycast(shootPoint.transform.position, barrelDir, barrelDist, mapLayerMask);

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, shootPoint.transform.position);
        lineRenderer.SetPosition(1, targetPoint);
        lineRenderer.startColor = !mapHit ? Color.green : Color.red;
        lineRenderer.endColor = !mapHit ? Color.green : Color.red;

        if (isHit && !mapHit)
        {
            if (aimInfo.collider.TryGetComponent<HealthSystem>(out var targetHealth))
            {
                int damage = Random.Range(30, 40);
                targetHealth.DealDamage(damage);
                Debug.Log($"Dealt {damage} damage to {aimInfo.collider.name}");
            }
        }

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
        facing.y += look.x;
        if (facingCam.x >= 270) facingCam.x -= 360;
        facingCam.x = Mathf.Clamp(facingCam.x, -70, 70);
        facingCam.x -= look.y;
        transform.localEulerAngles = new(0, facing.y, 0);
        cam.transform.localEulerAngles = new(facingCam.x, 0, 0);
    }
    public IEnumerator Reload()
    {
        reloading = true;
        yield return _waitForSeconds0_5;
        currentAmmo = 20;
        storedMag--;
        UpdateAmmoUI();
        reloading = false;
    }
    private void UpdateAmmoUI() => ammo.text = $"{currentAmmo}/{storedMag}";
    private void RecoilOverTime()
    {
        while (shooting)
        {
            recoilMultiplier++;
        }
        while (!shooting && recoilMultiplier > 0)
        {
            recoilMultiplier--;
        }
    }
}