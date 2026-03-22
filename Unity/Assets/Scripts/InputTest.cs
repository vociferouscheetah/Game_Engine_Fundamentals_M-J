using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class InputTest : MonoBehaviour
{
    private CustomInputActions input;
    private bool attackHeld;

    private void Awake() => input = new CustomInputActions();

    private void OnEnable()
    {
        input.Player.Enable();

        input.Player.Attack.performed += AttackPressed;
        input.Player.Attack.started += _ => attackHeld = true;
        input.Player.Attack.canceled += AttackReleased;
    }

    private void OnDisable()
    {
        input.Player.Attack.performed -= AttackPressed;
        input.Player.Attack.canceled -= AttackReleased;
        input.Player.Disable();
    }

    private void Update()
    {
        if (attackHeld) Debug.Log("ATTACK HELD");

        Vector2 move = input.Player.Move.ReadValue<Vector2>();
        if (move != Vector2.zero) Debug.Log($"MOVE {move}");
    }

    private void AttackPressed(InputAction.CallbackContext _) => Debug.Log("ATTACK PRESSED");

    private void AttackReleased(InputAction.CallbackContext _)
    {
        attackHeld = false;
        Debug.Log("ATTACK RELEASED");
    }
}
