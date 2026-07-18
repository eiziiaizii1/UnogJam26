using Game.Runtime.Events;
using UnityEngine;

namespace Game.Runtime.Collectibles
{
    /// <summary>
    /// Marks the player as able to collect pickups (the typed alternative to a string tag, §10.5)
    /// and tallies collectibles by listening to the shared channel — decoupled from whichever
    /// <see cref="Collectible"/> raised it. The HUD will read <see cref="Total"/> in M3.
    /// </summary>
    public sealed class PlayerCollector : MonoBehaviour
    {
        [SerializeField] private IntEventChannel _pickedChannel;

        public int Total { get; private set; }

        private void OnEnable()
        {
            if (_pickedChannel != null) _pickedChannel.Subscribe(OnCollected);
        }

        private void OnDisable()
        {
            if (_pickedChannel != null) _pickedChannel.Unsubscribe(OnCollected);
        }

        private void OnCollected(int value)
        {
            Total += value;
            Debug.Log($"[Collector] +{value} → total {Total}");
        }
    }
}
