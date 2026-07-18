using Game.Runtime.Combat;
using Game.Runtime.Input;
using Game.Runtime.Player;
using UnityEngine;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// The player's side of interaction: finds the nearest <see cref="Interactable"/> in range,
    /// lights its prompt, and routes Interact presses to it. Centralising the choice here is what
    /// stops every interactable in the scene from answering the same key press.
    /// <para>
    /// While a bubble is open it disables the input *consumers* (<see cref="PlayerMotor"/>,
    /// <see cref="Shooter"/>) rather than the <see cref="InputReader"/> itself — disabling the
    /// reader would also kill the Interact action needed to advance the dialogue.
    /// </para>
    /// </summary>
    [RequireComponent(typeof(InputReader))]
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private InputReader _input;
        [Tooltip("Disabled while a dialogue is open so the player can't walk or shoot mid-sentence.")]
        [SerializeField] private PlayerMotor _motor;
        [SerializeField] private Shooter _shooter;

        private Interactable _nearest;
        private Interactable _active;

        private void Awake()
        {
            if (_input == null) _input = GetComponent<InputReader>();
            if (_motor == null) _motor = GetComponent<PlayerMotor>();
            if (_shooter == null) _shooter = GetComponent<Shooter>();
        }

        private void OnEnable() => _input.InteractPressed += OnInteractPressed;

        private void OnDisable()
        {
            _input.InteractPressed -= OnInteractPressed;
            // Never strand the player frozen if this gets disabled mid-conversation.
            if (_active != null) EndDialogue();
        }

        private void Update()
        {
            // The prompt is meaningless while talking, and the target must not drift mid-sentence.
            if (_active != null) return;

            var found = FindNearest();
            if (found == _nearest) return;

            if (_nearest != null) _nearest.ShowPrompt(false);
            _nearest = found;
            if (_nearest != null) _nearest.ShowPrompt(true);
        }

        private Interactable FindNearest()
        {
            Vector2 position = transform.position;
            Interactable best = null;
            float bestSqr = float.MaxValue;

            var all = Interactable.All;
            for (int i = 0; i < all.Count; i++)
            {
                var candidate = all[i];
                if (candidate == null || !candidate.IsInRange(position)) continue;

                float sqr = candidate.SqrDistanceTo(position);
                if (sqr >= bestSqr) continue;

                bestSqr = sqr;
                best = candidate;
            }

            return best;
        }

        private void OnInteractPressed()
        {
            if (_active != null)
            {
                // Advance returns false once the last line is dismissed.
                if (!_active.Advance()) EndDialogue();
                return;
            }

            if (_nearest == null || !_nearest.Open()) return;

            _active = _nearest;
            SetPlayerBusy(true);
        }

        private void EndDialogue()
        {
            if (_active != null)
            {
                _active.ForceClose();
                _active = null;
            }

            SetPlayerBusy(false);
            _nearest = null; // Re-evaluated next Update, which re-lights the prompt if still in range.
        }

        private void SetPlayerBusy(bool busy)
        {
            if (_motor != null) _motor.enabled = !busy;
            if (_shooter != null) _shooter.enabled = !busy;
        }
    }
}
