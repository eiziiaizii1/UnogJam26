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

        private void Awake()
        {
            _controls = new GameControls();
        }

        private void OnEnable()
        {
            _controls.Player.Enable();
            _controls.Player.Jump.performed += OnJumpPerformed;
            _controls.Player.Jump.canceled += OnJumpCanceled;
        }

        private void OnDisable()
        {
            _controls.Player.Jump.performed -= OnJumpPerformed;
            _controls.Player.Jump.canceled -= OnJumpCanceled;
            _controls.Player.Disable();
        }

        private void Update()
        {
            Move = _controls.Player.Move.ReadValue<Vector2>();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context) => JumpPressed?.Invoke();

        private void OnJumpCanceled(InputAction.CallbackContext context) => JumpReleased?.Invoke();

        private void OnDestroy()
        {
            _controls?.Dispose();
        }
    }
}
