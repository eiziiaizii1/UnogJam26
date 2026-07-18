using Game.Runtime.Events;
using UnityEngine;

namespace Game.Runtime.Collectibles
{
    /// <summary>
    /// A nature pickup. When the player's <see cref="PlayerCollector"/> overlaps it, it announces
    /// its value on the shared channel and despawns — it never touches the score directly (§5.4),
    /// so HUD, audio, and achievements can all listen to the same fact independently.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class Collectible : MonoBehaviour
    {
        [SerializeField] private int _value = 1;
        [SerializeField] private IntEventChannel _pickedChannel;

        [Header("Bob")]
        [SerializeField] private float _bobAmplitude = 0.15f;
        [SerializeField] private float _bobFrequency = 2f;

        private Vector3 _basePosition;
        private bool _collected;

        private void Start()
        {
            _basePosition = transform.position;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * _bobFrequency) * _bobAmplitude;
            transform.position = _basePosition + new Vector3(0f, offset, 0f);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (other.GetComponentInParent<PlayerCollector>() == null) return;

            _collected = true;
            if (_pickedChannel != null) _pickedChannel.Raise(_value);
            Destroy(gameObject);
        }
    }
}
