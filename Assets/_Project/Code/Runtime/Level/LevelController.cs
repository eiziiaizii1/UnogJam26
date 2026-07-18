using System.Collections;
using Game.Runtime.Combat;
using Game.Runtime.Events;
using Game.Runtime.Player;
using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// Per-level lifecycle. Handles the in-level feedback (freeze/tint on completion, respawn on
    /// death) and <b>announces</b> both facts on event channels so the persistent
    /// <see cref="RunController"/> can sequence the run (Guide §5.4 — announce facts, don't issue orders).
    /// <para>
    /// If no <see cref="RunController"/> is present (e.g. you press Play on a single level while
    /// authoring), it falls back to the M1 behaviour: respawn locally instead of reloading.
    /// </para>
    /// </summary>
    public sealed class LevelController : MonoBehaviour
    {
        [SerializeField] private GameObject _player;
        [SerializeField] private LevelExit _exit;
        [Tooltip("Player control behaviours frozen when the level is completed (input, motor, shooter).")]
        [SerializeField] private Behaviour[] _playerControl;
        [SerializeField] private float _respawnDelaySeconds = 1.25f;
        [SerializeField] private Color _completeTint = new(0.40f, 0.90f, 0.50f);

        [Header("Run channels (leave empty to stay level-local)")]
        [SerializeField] private VoidEventChannel _levelCompleted;
        [SerializeField] private VoidEventChannel _playerDiedChannel;

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
            Debug.Log("[Level] COMPLETE — reached the exit!");

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

            // Announce it — the RunController advances to the next level (or ends the run).
            if (_levelCompleted == null)
            {
                Debug.LogWarning($"[{nameof(LevelController)}] 'Level Completed' channel is NOT assigned — " +
                                 "the run cannot advance. Assign it in the Inspector to chain levels.", this);
                return;
            }

            if (RunController.Instance == null)
            {
                Debug.LogWarning($"[{nameof(LevelController)}] No {nameof(RunController)} in the scene — " +
                                 "nothing is listening, so the level cannot advance.", this);
            }

            _levelCompleted.Raise();
        }

        private void OnPlayerDied()
        {
            if (_completed) return;

            // With a run in progress, the RunController reloads the level. Standalone, respawn here.
            if (RunController.Instance != null && _playerDiedChannel != null)
            {
                _playerDiedChannel.Raise();
                return;
            }

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
