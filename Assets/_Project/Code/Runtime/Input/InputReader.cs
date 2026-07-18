using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Runtime.Input
{
    /// <summary>
    /// The single edge where raw Input System bindings become game actions (Guide §5.8, §11.9).
    /// Gameplay reads intents from here — <see cref="Move"/>, <see cref="JumpPressed"/>,
    /// <see cref="JumpReleased"/> — and never touches Keyboard/Gamepad APIs directly, which is
    /// what keeps rebinding and AI-driven players cheap later. Wraps the generated
    /// <see cref="GameControls"/> asset (C# class generation is enabled on the .inputactions).
    /// </summary>
    public sealed class InputReader : MonoBehaviour
    {
        private GameControls _controls;

        /// <summary>Current movement axis. For this side-scroller only <c>.x</c> is used (walk).</summary>
        public Vector2 Move { get; private set; }

        /// <summary>Raised the frame the jump button goes down.</summary>
        public event Action JumpPressed;

        /// <summary>Raised when the jump button is released (drives variable jump height).</summary>
        public event Action JumpReleased;

        /// <summary>True while the fire button is held (the shooter auto-fires at its own rate).</summary>
        public bool FireHeld { get; private set; }

        /// <summary>Raised the frame the interact button goes down (talk to a carcass, read a sign).</summary>
        public event Action InteractPressed;

        private void Awake()
        {
            _controls = new GameControls();
        }

        private void OnEnable()
        {
            _controls.Player.Enable();
            _controls.Player.Jump.performed += OnJumpPerformed;
            _controls.Player.Jump.canceled += OnJumpCanceled;
            _controls.Player.Interact.performed += OnInteractPerformed;
        }

        private void OnDisable()
        {
            _controls.Player.Jump.performed -= OnJumpPerformed;
            _controls.Player.Jump.canceled -= OnJumpCanceled;
            _controls.Player.Interact.performed -= OnInteractPerformed;
            _controls.Player.Disable();

            // Stale intents would otherwise survive the disable and fire on re-enable
            // (dialogue toggles this component, so it happens constantly).
            Move = Vector2.zero;
            FireHeld = false;
        }

        private void Update()
        {
            Move = _controls.Player.Move.ReadValue<Vector2>();
            FireHeld = _controls.Player.Fire.IsPressed();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context) => JumpPressed?.Invoke();

        private void OnJumpCanceled(InputAction.CallbackContext context) => JumpReleased?.Invoke();

        private void OnInteractPerformed(InputAction.CallbackContext context) => InteractPressed?.Invoke();

        private void OnDestroy()
        {
            _controls?.Dispose();
        }
    }
}
