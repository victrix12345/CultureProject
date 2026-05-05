using System.Collections;
using UnityEngine;

public class PlayerSystems : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_2 = new WaitForSeconds(0.2f);
    private InputSystem_Actions inputActions;
    private CharacterController charCon;
    private float grav = -9.81f / 2, jumpHeight = 5f, speed = 7f, rayDistance, rayDrawTime;
    private Vector3 playerVel, rayStart, rayDirection;
    private Vector2 movement, look;
    private bool grounded = true, jumped = false, sneaked = false, shooting = false, targetted = false, rayHit = false;
    public GameObject cam;
    private int entityLayerMask;
    void Awake()
    {
        entityLayerMask = LayerMask.GetMask("Entity");
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

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
        yield return new WaitForSeconds(1f);
    }
    private void FixedUpdate()
    {
        grounded = charCon.isGrounded;

        Vector3 newMove = movement.x * transform.right + movement.y * transform.forward;
        newMove = Vector3.ClampMagnitude(newMove, 1);

        if (jumped && grounded) StartCoroutine(Jump());

        if (grounded && !jumped && playerVel.y < -2) playerVel.y = -2; // stabilising movement on slopes

        else grav = -9.81f * 3; // default gravity
        playerVel.y += grav * Time.deltaTime; // applying gravity

        if (sneaked) speed = 3.5f;

        Vector3 endMove = (newMove * speed) + (Vector3.up * playerVel.y);
        charCon.Move(endMove * Time.deltaTime); // apply all necessary forces calculated
    }
    private void Update()
    {
        Vector3 facing = gameObject.transform.localEulerAngles;
        Vector3 facingCam = cam.transform.localEulerAngles;
        facing.y += look.x;
        if (facingCam.x >= 270) facingCam.x -= 360;
        facingCam.x = Mathf.Clamp(facingCam.x, -70, 70);
        facingCam.x -= look.y;
        transform.localEulerAngles = new(0, facing.y, 0);
        cam.transform.localEulerAngles = new(facingCam.x, 0, 0);

        if (shooting && !targetted) StartCoroutine(Shooting());
    }
    IEnumerator Shooting()
    {
        targetted = true;
        RaycastHit aimInfo = new();
        bool isHit = Physics.Raycast(cam.transform.position, cam.transform.forward, out aimInfo, 100f, entityLayerMask);

        rayStart = cam.transform.position;
        rayDirection = cam.transform.forward;
        rayDistance = isHit ? aimInfo.distance : 100f;
        rayHit = isHit;
        rayDrawTime = 0.2f;

        if (isHit)
        {
            HealthSystem targetHealth = aimInfo.collider.gameObject.GetComponent<HealthSystem>();
            if (targetHealth != null)
            {
                int damage = Random.Range(20, 40);
                targetHealth.DealDamage(damage);
                Debug.Log("Dealt " + damage + " damage to " + aimInfo.collider.name);
            }
        }
        yield return _waitForSeconds0_2;
        targetted = false;
    }
    private void OnDrawGizmos()
    {
        if (rayDrawTime > 0)
        {
            Gizmos.color = rayHit ? Color.green : Color.red;
            Gizmos.DrawRay(rayStart, rayDirection * rayDistance);
            rayDrawTime -= Time.deltaTime;
        }
    }
}