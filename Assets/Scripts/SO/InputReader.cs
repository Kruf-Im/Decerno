using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Objects/InputReader")]
public class InputReader : ScriptableObject, Controls.IPlayerActions, Controls.IUIActions
{
    // Events for player actions
    public UnityEvent<Vector2> MoveEvent { get; } = new UnityEvent<Vector2>();
    public UnityEvent AttackEvent { get; } = new UnityEvent();
    public UnityEvent<bool> InteractEvent { get; } = new UnityEvent<bool>();
    public UnityEvent<bool> JumpEvent { get; } = new UnityEvent<bool>();
    public UnityEvent PreviousEvent { get; } = new UnityEvent();
    public UnityEvent NextEvent { get; } = new UnityEvent();
    public UnityEvent<bool> SprintEvent { get; } = new UnityEvent<bool>();
    public UnityEvent PauseEvent { get; } = new UnityEvent();
    public UnityEvent InventoryEvent { get; } = new UnityEvent();

    // Event for UI actions
    public UnityEvent<Vector2> NavigateEvent { get; } = new UnityEvent<Vector2>();
    public UnityEvent SubmitEvent { get; } = new UnityEvent();
    public UnityEvent CancelEvent { get; } = new UnityEvent();
    public UnityEvent<Vector2> PointEvent { get; } = new UnityEvent<Vector2>();
    public UnityEvent ClickEvent { get; } = new UnityEvent();
    public UnityEvent RightClickEvent { get; } = new UnityEvent();
    public UnityEvent MiddleClickEvent { get; } = new UnityEvent();
    public UnityEvent<Vector2> ScrollWheelEvent { get; } = new UnityEvent<Vector2>();

    public Vector2 MouseDelta { get; private set; }
    public Vector2 MoveInput { get; private set; }
    private Controls _controls;
    private void OnEnable()
    {
        if (_controls == null)
        {
            _controls = new Controls();
            _controls.Player.SetCallbacks(this);
            _controls.UI.SetCallbacks(this);
        }
        EnablePlayerInput();
    }
    private void OnDisable()
    {
        DisableInput();
    }

    public void EnablePlayerInput()
    {
        _controls.Disable();
        _controls.Player.Enable();
    }
    public void EnableUIInput()
    {
        _controls.Disable();
        _controls.UI.Enable();
    }
    public void DisableInput()
    {
        _controls.Disable();
    }

    // IPlayerActions interface methods
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent.Invoke(context.ReadValue<Vector2>());
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        MouseDelta = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        AttackEvent.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            InteractEvent.Invoke(true);
        }
        else if (context.canceled)
        {
            InteractEvent.Invoke(false);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpEvent.Invoke(true);
        }
        else if (context.canceled)
        {
            JumpEvent.Invoke(false);
        }
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
        PreviousEvent.Invoke();
    }

    public void OnNext(InputAction.CallbackContext context)
    {
        NextEvent.Invoke();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SprintEvent.Invoke(true);
        }
        else if (context.canceled)
        {
            SprintEvent.Invoke(false);
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        PauseEvent.Invoke();
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        InventoryEvent.Invoke();
    }

    // IUIActions interface methods
    public void OnNavigate(InputAction.CallbackContext context)
    {
        NavigateEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        SubmitEvent.Invoke();
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        CancelEvent.Invoke();
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
        PointEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        ClickEvent.Invoke();
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
        RightClickEvent.Invoke();
    }

    public void OnMiddleClick(InputAction.CallbackContext context)
    {
        MiddleClickEvent.Invoke();
    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {
        ScrollWheelEvent.Invoke(context.ReadValue<Vector2>());
    }
}
