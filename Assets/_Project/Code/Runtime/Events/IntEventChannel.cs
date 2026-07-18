using System;
using Game.Core.Events;
using UnityEngine;

namespace Game.Runtime.Events
{
    /// <summary>
    /// ScriptableObject event channel (Guide §11.4): a shared asset that raisers and listeners
    /// reference in the inspector, decoupling them with no singleton and no direct references.
    /// Wraps the pure Core <see cref="EventChannel{T}"/>. Listeners must unsubscribe in OnDisable.
    /// </summary>
    [CreateAssetMenu(menuName = "IloveNature/Int Event Channel", fileName = "IntEventChannel")]
    public sealed class IntEventChannel : ScriptableObject
    {
        private readonly EventChannel<int> _channel = new();

        public void Subscribe(Action<int> listener) => _channel.Subscribe(listener);

        public void Unsubscribe(Action<int> listener) => _channel.Unsubscribe(listener);

        public void Raise(int value) => _channel.Raise(value);
    }
}
