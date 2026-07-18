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
        [SerializeField] private float _fireIntervalSeconds = 0.35f; // Silahın ağırlığını hissettirmek için varsayılan atış hızı yavaşlatıldı (0.18f -> 0.35f)
        [SerializeField] private float _bulletSpeedUnitsPerSecond = 18f;
        [Tooltip("Muzzle offset from the player origin; X is mirrored by facing direction.")]
        [SerializeField] private Vector2 _muzzleOffset = new(0.6f, 0f);

        [Header("Gamefeel & Recoil")]
        [SerializeField] private float _recoilForce = 4.5f; // Geri tepme kuvveti
        [SerializeField] private float _screenShakeIntensity = 0.18f; // Ekran sarsıntı şiddeti
        [SerializeField] private float _screenShakeDecay = 6f; // Ekran sarsıntı sönümlenme hızı

        [Header("Muzzle Flash Light")]
        [Tooltip("Silah ucunda belirecek olan Işık nesnesi (GameObject olarak açılıp kapanır)")]
        [SerializeField] private GameObject _muzzleFlashLight;
        [SerializeField] private float _flashDuration = 0.05f; // Işığın aktif kalma süresi

        [Header("Audio")]
        [SerializeField] private Game.Runtime.Audio.SfxDefinition _shootSfx;

        private Rigidbody2D _rigidbody;
        private float _nextFireTime;
        private float _facing = 1f;
        private Coroutine _flashCoroutine;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            
            // Başlangıçta namlu ışığını kapat
            if (_muzzleFlashLight != null)
            {
                _muzzleFlashLight.SetActive(false);
            }
        }

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

            // Geri tepme (Recoil) fiziksel itme
            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = new Vector2(_rigidbody.linearVelocity.x - _facing * _recoilForce, _rigidbody.linearVelocity.y);
            }

            // Ekran sarsıntısı (Screen Shake)
            var mainCamera = Camera.main;
            if (mainCamera != null && mainCamera.TryGetComponent<Game.Runtime.Presentation.CameraFollow>(out var follow))
            {
                follow.Shake(_screenShakeIntensity, _screenShakeDecay);
            }

            // Namlu ateşi ışığı (Muzzle Flash Light)
            if (_muzzleFlashLight != null)
            {
                if (_flashCoroutine != null)
                {
                    StopCoroutine(_flashCoroutine);
                }
                _flashCoroutine = StartCoroutine(FlashLightRoutine());
            }

            // Atış ses efekti (Shoot SFX)
            if (_shootSfx != null && Game.Runtime.Audio.SfxPlayer.Instance != null)
            {
                Game.Runtime.Audio.SfxPlayer.Instance.Play(_shootSfx);
            }
        }

        private System.Collections.IEnumerator FlashLightRoutine()
        {
            _muzzleFlashLight.SetActive(true);
            yield return new WaitForSeconds(_flashDuration);
            _muzzleFlashLight.SetActive(false);
            _flashCoroutine = null;
        }
    }
}
