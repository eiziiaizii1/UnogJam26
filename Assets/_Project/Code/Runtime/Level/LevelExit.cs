using System;
using Game.Runtime.Player;
using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// The goal zone. When the player reaches it, announces <see cref="Reached"/> once. Kept a
    /// dumb notifier (§3.4) — what "reaching the exit" *means* (complete, load next, upgrade) is
    /// the <see cref="LevelController"/>'s call now and GameFlow's in M2.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class LevelExit : MonoBehaviour
    {
        public event Action Reached;

        private bool _triggered;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (other.GetComponentInParent<PlayerMotor>() == null) return;

            _triggered = true;
            Reached?.Invoke();
        }
    }
}
