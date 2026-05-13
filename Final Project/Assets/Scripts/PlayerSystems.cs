using System.Collections;
using TMPro.SpriteAssetUtilities;
using UnityEngine;

public class PlayerSystems : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
    private InputSystem_Actions inputActions;
    private CharacterController charCon;
    private float grav = -9.81f / 2, jumpHeight = 2f, speed = 7f, rayDistance, rayFireTime;
    private Vector3 playerVel, rayStart, rayDirection;
    private Vector2 movement, look;
    private bool grounded = true, jumped = false, sneaked = false, shooting = false, targetted = false, rayHit = false;
    public GameObject cam, shootPoint;
    private int mapLayerMask;
    private LineRenderer lineRenderer;
    void Awake()
    {
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

        GameObject lineObj = new GameObject("RayVisualizer");
        lineObj.transform.SetParent(transform);
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
    }
    private void OnEnable() // GOOD HABIT PART 1
    {
        inputActions.Player.Move.Enable();
        inputActions.Player.Jump.Enable();
        inputActions.Player.Sprint.Enable();
        inputActions.Player.Look.Enable();
        inputActions.Player.Attack.Enable();
    }
    private void OnDisable() // GOOD HABIT PART 2
    {
        inputActions.Player.Move.Disable();
        inputActions.Player.Jump.Disable();
        inputActions.Player.Sprint.Disable();
        inputActions.Player.Look.Disable();
        inputActions.Player.Attack.Disable();
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

        else grav = -9.81f * 3;
        playerVel.y += grav * Time.deltaTime;

        if (sneaked) speed = 3.5f;
        else speed = 7f;

        Vector3 endMove = (newMove * speed) + (Vector3.up * playerVel.y);
        charCon.Move(endMove * Time.deltaTime);
    }
    private void Update()
    {
        CameraCalc();

        if (shooting && !targetted) StartCoroutine(Shooting());
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

        rayStart = shootPoint.transform.position;
        rayDirection = barrelDir;
        rayDistance = barrelDist;
        rayHit = mapHit;
        rayFireTime = Time.time;

        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, rayStart);
        lineRenderer.SetPosition(1, targetPoint);
        lineRenderer.startColor = rayHit ? Color.green : Color.red;
        lineRenderer.endColor = rayHit ? Color.green : Color.red;

        if (isHit && !mapHit)
        {
            HealthSystem targetHealth = aimInfo.collider.gameObject.GetComponent<HealthSystem>();
            if (targetHealth != null)
            {
                int damage = Random.Range(30, 40);
                targetHealth.DealDamage(damage);
                Debug.Log("Dealt " + damage + " damage to " + aimInfo.collider.name);
            }
        }
        yield return _waitForSeconds0_1;
        lineRenderer.enabled = false;
        yield return _waitForSeconds0_1;
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
}