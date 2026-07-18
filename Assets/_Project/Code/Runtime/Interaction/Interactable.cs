using System.Collections.Generic;
using UnityEngine;

namespace Game.Runtime.Interaction
{
    /// <summary>
    /// Something the player can walk up to and press Interact on — a carcass, a sign, a terminal.
    /// Owns its prompt marker and its lines; the actual "who is nearest, who gets to open" decision
    /// belongs to <see cref="PlayerInteractor"/> so two interactables can never both open at once.
    /// <para>
    /// Registers itself in a static list rather than relying on trigger colliders: proximity is a
    /// query the player makes once a frame, and this keeps carcasses free of physics setup.
    /// </para>
    /// </summary>
    public sealed class Interactable : MonoBehaviour
    {
        [Header("Content")]
        [Tooltip("Shown one line per Interact press. The bubble closes after the last one.")]
        [TextArea(2, 4)]
        [SerializeField] private string[] _lines = { "..." };

        [Header("References")]
        [Tooltip("The '!' marker. Shown only while this is the player's nearest target.")]
        [SerializeField] private GameObject _prompt;
        [SerializeField] private DialogueBubble _bubble;

        [Header("Tuning")]
        [Tooltip("How close the player must be, in world units, measured from this object's origin.")]
        [SerializeField] private float _range = 2.5f;

        private static readonly List<Interactable> Registry = new();

        /// <summary>All enabled interactables, for the player to search. Never null.</summary>
        public static IReadOnlyList<Interactable> All => Registry;

        public float Range => _range;

        /// <summary>True while this interactable's bubble is showing.</summary>
        public bool IsOpen => _bubble != null && _bubble.IsOpen;

        private void OnEnable()
        {
            Registry.Add(this);
            if (_prompt != null) _prompt.SetActive(false);
        }

        private void OnDisable()
        {
            Registry.Remove(this);
            // Leaving a prompt lit on a disabled object is a classic loose end after a scene swap.
            if (_prompt != null) _prompt.SetActive(false);
        }

        /// <summary>Squared distance to a point — squared so the caller can compare without a sqrt.</summary>
        public float SqrDistanceTo(Vector2 point) => ((Vector2)transform.position - point).sqrMagnitude;

        public bool IsInRange(Vector2 point) => SqrDistanceTo(point) <= _range * _range;

        public void ShowPrompt(bool show)
        {
            if (_prompt != null && _prompt.activeSelf != show) _prompt.SetActive(show);
        }

        /// <summary>Opens the bubble. Returns false if there is nothing to say.</summary>
        public bool Open()
        {
            if (_bubble == null || _lines == null || _lines.Length == 0) return false;
            ShowPrompt(false);
            _bubble.Show(_lines);
            return true;
        }

        /// <summary>Finishes the current line, or moves to the next one. Returns false once closed.</summary>
        public bool Advance() => _bubble != null && _bubble.Advance();

        public void ForceClose()
        {
            if (_bubble != null) _bubble.Close();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, _range);
        }
    }
}
