using System.Collections;
using Game.Runtime.Combat;
using Game.Runtime.Player;
using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// Minimal level lifecycle for the M1 slice: respawn the player on death (its promised "real
    /// restart") and declare the level complete when the <see cref="LevelExit"/> is reached.
    /// Full app-level sequencing (upgrade screen, next level, ending) is M2's GameFlow/LevelLoader.
    /// </summary>
    public sealed class LevelController : MonoBehaviour
    {
        [SerializeField] private GameObject _player;
        [SerializeField] private LevelExit _exit;
        [Tooltip("Player control behaviours frozen when the level is completed (input, motor, shooter).")]
        [SerializeField] private Behaviour[] _playerControl;
        [SerializeField] private float _respawnDelaySeconds = 1.25f;
        [SerializeField] private Color _completeTint = new(0.40f, 0.90f, 0.50f);

        private HealthComponent _health;
        private PlayerDeath _death;
        private Rigidbody2D _body;
        private SpriteRenderer _renderer;
        private Vector3 _spawnPosition;
        private bool _completed;

        private void Awake()
        {
            _spawnPosition = _player.transform.position;
            _health = _player.GetComponent<HealthComponent>();
            _death = _player.GetComponent<PlayerDeath>();
            _body = _player.GetComponent<Rigidbody2D>();
            _renderer = _player.GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (_exit != null) _exit.Reached += OnExitReached;
            if (_death != null) _death.Died += OnPlayerDied;
        }

        private void OnDisable()
        {
            if (_exit != null) _exit.Reached -= OnExitReached;
            if (_death != null) _death.Died -= OnPlayerDied;
        }

        private void OnExitReached()
        {
            if (_completed) return;
            _completed = true;
            Debug.Log("[Level] COMPLETE — reached the exit! (upgrade + next-level flow arrives in M2)");

            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            if (_playerControl != null)
            {
                foreach (var behaviour in _playerControl)
                {
                    if (behaviour != null) behaviour.enabled = false;
                }
            }

            if (_renderer != null) _renderer.color = _completeTint;
        }

        private void OnPlayerDied()
        {
            if (_completed) return;
            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(_respawnDelaySeconds);

            _player.transform.position = _spawnPosition;
            if (_body != null) _body.linearVelocity = Vector2.zero;
            _health?.ResetToFull();
            _death?.Revive();
        }
    }
}
