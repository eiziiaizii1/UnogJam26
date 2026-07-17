using Game.Runtime.Input;
using UnityEngine;

namespace Game.Runtime.Combat
{
    /// <summary>
    /// Player shooting (Guide §3.1 single responsibility): its own component next to
    /// <see cref="PlayerMotor"/>, both fed by the same <see cref="InputReader"/>. Fires pooled
    /// bullets at a fixed rate while held, in the last-faced horizontal direction. Knows nothing
    /// about pooling — it just asks <see cref="BulletPool"/> to spawn.
    /// </summary>
    public sealed class Shooter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InputReader _input;
        [SerializeField] private BulletPool _bulletPool;

        [Header("Tuning")]
        [SerializeField] private float _fireIntervalSeconds = 0.18f;
        [SerializeField] private float _bulletSpeedUnitsPerSecond = 18f;
        [Tooltip("Muzzle offset from the player origin; X is mirrored by facing direction.")]
        [SerializeField] private Vector2 _muzzleOffset = new(0.6f, 0f);

        private float _nextFireTime;
        private float _facing = 1f;

        private void Update()
        {
            if (_input == null || _bulletPool == null) return;

            UpdateFacing();

            if (_input.FireHeld && Time.time >= _nextFireTime)
            {
                Fire();
            }
        }

        private void UpdateFacing()
        {
            float moveX = _input.Move.x;
            if (Mathf.Abs(moveX) > 0.01f)
            {
                _facing = Mathf.Sign(moveX);
            }
        }

        private void Fire()
        {
            _nextFireTime = Time.time + _fireIntervalSeconds;

            var direction = new Vector2(_facing, 0f);
            var muzzle = (Vector2)transform.position + new Vector2(_muzzleOffset.x * _facing, _muzzleOffset.y);
            _bulletPool.Spawn(muzzle, direction, _bulletSpeedUnitsPerSecond, gameObject);
        }
    }
}
