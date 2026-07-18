using System;
using UnityEngine;

namespace Game.Runtime.Events
{
    /// <summary>
    /// A payload-free ScriptableObject event channel (Guide §11.4) — the companion to
    /// <see cref="IntEventChannel"/>. Used for run-level facts like "level completed" or
    /// "player died", letting a per-scene object announce to a persistent listener without
    /// either holding a reference to the other. Listeners must unsubscribe in OnDisable.
    /// </summary>
    [CreateAssetMenu(menuName = "IloveNature/Void Event Channel", fileName = "VoidEventChannel")]
    public sealed class VoidEventChannel : ScriptableObject
    {
        private event Action Raised;

        public void Subscribe(Action listener) => Raised += listener;

        public void Unsubscribe(Action listener) => Raised -= listener;

        public void Raise() => Raised?.Invoke();
    }
}
