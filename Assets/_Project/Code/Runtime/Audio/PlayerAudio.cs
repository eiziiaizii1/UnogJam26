using UnityEngine;
using Game.Runtime.Input;

namespace Game.Runtime.Audio
{
    /// <summary>
    /// Self-contained player audio listener. Subscribes to the player's InputReader Jump event
    /// and plays the jump SFX.
    /// </summary>
    public sealed class PlayerAudio : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [SerializeField] private SfxDefinition _jumpSfx;

        private void Awake()
        {
            if (_input == null)
            {
                _input = GetComponent<InputReader>();
            }
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.JumpPressed += OnJumpPressed;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.JumpPressed -= OnJumpPressed;
            }
        }

        private void OnJumpPressed()
        {
            if (SfxPlayer.Instance != null)
            {
                SfxPlayer.Instance.Play(_jumpSfx);
            }
        }
    }
}
