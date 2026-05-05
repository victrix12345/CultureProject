using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerSystems : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    private CharacterController charCon;
    private float grav = -9.81f / 2, jumpHeight = 5f, speed = 7f;
    private Vector3 playerVel;
    private Vector2 movement, look;
    private bool grounded, jumped, sneaked, shooting;
    public GameObject cam;
    void Awake()
    {
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

        if (shooting) StartCoroutine(Shooting());
    }
    IEnumerator Shooting()
    {
        RaycastHit aimInfo = new();
        bool isHit = Physics.Raycast(cam.transform.position, cam.transform.forward, out aimInfo, 100f, 3);
        if (isHit)
        {
            HealthSystem targetHealth = aimInfo.collider.gameObject.GetComponent<HealthSystem>();
            targetHealth.DealDamage(34);
        }
        yield return new WaitForSeconds(0.1f);
    }
}