using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSystems : MonoBehaviour
{
    private InputSystem_Actions inputActions;
    private CharacterController charCon;
    private float grav = -9.81f * 2, jumpHeight = 5f, speed = 7f;
    private Vector3 playerVel;
    private Vector2 movement;
    private bool grounded, jumped, sneaked;
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

    }
    private void OnEnable() // GOOD HABIT PART 1
    {
        inputActions.Player.Move.Enable();
        inputActions.Player.Jump.Enable();
    }
    private void OnDisable() // GOOD HABIT PART 2
    {
        inputActions.Player.Move.Disable();
        inputActions.Player.Jump.Disable();
    }
    public IEnumerator Jump()
    {
        playerVel.y = Mathf.Sqrt(jumpHeight * -2 * (-9.81f * 3f));
        yield return new WaitForSeconds(1f);
    }
    private void FixedUpdate()
    {
        grounded = charCon.isGrounded;

        Vector3 newMove = new(movement.x, 0, movement.y);
        newMove = Vector3.ClampMagnitude(newMove, 1);

        if (jumped) StartCoroutine(Jump());

        if (grounded && !jumped && playerVel.y < -2) playerVel.y = -2; // stabilising movement on slopes

        else grav = -9.81f * 3; // default gravity
        playerVel.y += grav * Time.deltaTime; // applying gravity

        Vector3 endMove = newMove * speed + Vector3.up * playerVel.y;
        charCon.Move(endMove * Time.deltaTime); // apply all necessary forces calculated
    }
}