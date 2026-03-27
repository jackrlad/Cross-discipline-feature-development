using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(PlayerInput))]
public class InputReader : MonoBehaviour
{
    public Vector2 Move { get; private set;}
    public bool Attack {get; private set;}
    public bool Swap {get; private set;}
    public bool Jump {get; private set;}
    public bool cRight {get; private set;}
    public bool cLeft {get; private set;}
    public bool Pause {get; private set;}

    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction swapAction;
    private InputAction jumpAction;
    private InputAction cRightAction;
    private InputAction cLeftAction;
    private InputAction pauseAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        var gameplay = playerInput.actions.FindActionMap("Gameplay", true);
        moveAction = gameplay.FindAction("Movement", true);
        attackAction = gameplay.FindAction("Attack", true);
        swapAction = gameplay.FindAction("Swap", true);
        jumpAction = gameplay.FindAction("Jump", true);
        cRightAction = gameplay.FindAction("Camera Right", true);
        cLeftAction = gameplay.FindAction("Camera Left", true);
        pauseAction = gameplay.FindAction("Pause", true);
    }

    private void OnEnable()
    {
        playerInput.actions.FindActionMap("Gameplay", true).Enable();

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;

        attackAction.performed += OnAttack;
        attackAction.canceled += OnAttack;

        swapAction.performed += OnSwap;
        swapAction.canceled += OnSwap;

        jumpAction.performed += OnJump;
        jumpAction.canceled += OnJump;

        cRightAction.performed += OnCRight;
        cRightAction.canceled +=  OnCRight;

        cLeftAction.performed += OnCLeft;
        cLeftAction.canceled +=  OnCLeft;

        pauseAction.performed += OnPause;
        pauseAction.canceled +=  OnPause;
    }

    private void OnDisable()
    {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;

        attackAction.performed -= OnAttack;
        attackAction.canceled -= OnAttack;

        swapAction.performed -= OnSwap;
        swapAction.canceled -= OnSwap;

        jumpAction.performed -= OnJump;
        jumpAction.canceled -= OnJump;

        cRightAction.performed -= OnCRight;
        cRightAction.canceled -=  OnCRight;

        cLeftAction.performed -= OnCLeft;
        cLeftAction.canceled -=  OnCLeft;

        pauseAction.performed -= OnPause;
        pauseAction.canceled -=  OnPause;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Move = ctx.ReadValue<Vector2>();
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        Attack = ctx.ReadValueAsButton();
    }

    private void OnSwap(InputAction.CallbackContext ctx)
    {
        Swap = ctx.ReadValueAsButton();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        Jump = ctx.ReadValueAsButton();
    }

    private void OnCRight(InputAction.CallbackContext ctx)
    {
        cRight = ctx.ReadValueAsButton();
    }

    private void OnCLeft(InputAction.CallbackContext ctx)
    {
        cLeft = ctx.ReadValueAsButton();
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        Pause = ctx.ReadValueAsButton();
    }

    public void Clear()
    {
        Move = Vector2.zero;
        Attack = false;
        Swap = false;
        Jump = false;
        cRight = false;
        cLeft = false;
        Pause = false;
    }
}