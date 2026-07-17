using System;
using System.Collections.Generic;

namespace Game.Core.Events
{
    /// <summary>
    /// Minimal typed observer channel (Guide §4.2). Announces past-tense facts to
    /// listeners that do not know each other; payloads should be small immutable data.
    /// Main-thread gameplay use only (not thread-safe).
    /// </summary>
    public sealed class EventChannel<T>
    {
        private readonly List<Action<T>> _listeners = new();

        /// <summary>Adds a listener. Idempotent: subscribing the same delegate twice is a no-op.</summary>
        public void Subscribe(Action<T> listener)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));
            if (!_listeners.Contains(listener)) _listeners.Add(listener);
        }

        /// <summary>Removes a listener if present.</summary>
        public void Unsubscribe(Action<T> listener)
        {
            _listeners.Remove(listener);
        }

        /// <summary>Delivers <paramref name="payload"/> to every listener. Safe for a listener to unsubscribe during dispatch.</summary>
        public void Raise(T payload)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                _listeners[i].Invoke(payload);
            }
        }
    }
}
