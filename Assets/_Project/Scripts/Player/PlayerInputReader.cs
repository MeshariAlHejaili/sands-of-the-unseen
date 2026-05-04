using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputReader : MonoBehaviour
{
    private const string MoveActionPath = "Player/Move";
    private const string AttackActionPath = "Player/Attack";
    private const string DashActionPath = "Player/Dash";
    private const string SprintActionPath = "Player/Sprint";
    private const string PointActionPath = "UI/Point";

    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction dashAction;
    private InputAction sprintAction;
    private InputAction pointAction;

    public Vector2 MoveInput => moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
    public bool IsAttackHeld => attackAction != null && attackAction.IsPressed();
    public bool WasDashPressedThisFrame => dashAction != null && dashAction.WasPressedThisFrame();
    public bool IsSprintHeld => sprintAction != null && sprintAction.IsPressed();
    public Vector2 PointerPosition => pointAction != null ? pointAction.ReadValue<Vector2>() : Vector2.zero;

    public static PlayerInputReader GetOrAdd(GameObject owner)
    {
        if (owner.TryGetComponent(out PlayerInputReader reader))
        {
            return reader;
        }

        return owner.AddComponent<PlayerInputReader>();
    }

    private void Awake()
    {
        InputActionAsset actions = InputSystem.actions;

        moveAction = actions.FindAction(MoveActionPath, true);
        attackAction = actions.FindAction(AttackActionPath, true);
        dashAction = actions.FindAction(DashActionPath, true);
        sprintAction = actions.FindAction(SprintActionPath, true);
        pointAction = actions.FindAction(PointActionPath, true);
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        attackAction?.Enable();
        dashAction?.Enable();
        sprintAction?.Enable();
        pointAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        attackAction?.Disable();
        dashAction?.Disable();
        sprintAction?.Disable();
        pointAction?.Disable();
    }
}
