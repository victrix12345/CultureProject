using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerSystems : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    private CharacterController charCon;
    private float grav = -9.81f * 2, jumpHeight = 5f, speed = 7f;
    private Vector3 playerVel;
    private Vector2 movement, look;
    private bool grounded, jumped, sneaked;
    public GameObject cam;
    void Awake()
    {
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
    }
    private void OnEnable() // GOOD HABIT PART 1
    {
        inputActions.Player.Move.Enable();
        inputActions.Player.Jump.Enable();
        inputActions.Player.Sprint.Enable();
        inputActions.Player.Look.Enable();
    }
    private void OnDisable() // GOOD HABIT PART 2
    {
        inputActions.Player.Move.Disable();
        inputActions.Player.Jump.Disable();
        inputActions.Player.Sprint.Disable();
        inputActions.Player.Look.Disable();
    }
    public IEnumerator Jump()
    {
        playerVel.y = Mathf.Sqrt(jumpHeight * -2 * (-9.81f * 3f));
        yield return new WaitForSeconds(1f);
    }
    private void FixedUpdate()
    {
        grounded = charCon.isGrounded;

        Vector3 facing = transform.localEulerAngles;
        Vector3 facingCam = cam.transform.localEulerAngles;
        facing.y += look.x;
        if (facingCam.x >= 270) facingCam.x -= 360;
        facingCam.x = Mathf.Clamp(facingCam.x, -70, 70);
        facingCam.x -= look.y;
        transform.localEulerAngles = new(0, facing.y, 0);
        cam.transform.localEulerAngles = new(facingCam.x, 0, 0);

        Vector3 newMove = new(movement.x, 0, movement.y);
        newMove = Vector3.ClampMagnitude(newMove, 1);

        if (jumped && grounded) StartCoroutine(Jump());

        if (grounded && !jumped && playerVel.y < -2) playerVel.y = -2; // stabilising movement on slopes

        else grav = -9.81f * 3; // default gravity
        playerVel.y += grav * Time.deltaTime; // applying gravity

        if (sneaked) speed /= 2;

        Vector3 endMove = newMove * speed + Vector3.up * playerVel.y;
        charCon.Move(endMove * Time.deltaTime); // apply all necessary forces calculated
    }
}