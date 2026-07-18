using System.Collections;
using Game.Core.Run;
using Game.Runtime.Events;
using Game.Runtime.Upgrades;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Runtime.Level
{
    /// <summary>
    /// Drives the run across scene loads (Guide §5.6): owns the <see cref="RunState"/>, listens for
    /// "level completed" / "player died", and loads the next level or reloads the current one.
    /// <para>
    /// Persistent: place this prefab in the FIRST level only — it survives loads and destroys any
    /// duplicate that arrives with a later scene. Per-scene objects talk to it through event
    /// channels, so nothing needs a direct reference in either direction.
    /// </para>
    /// </summary>
    public sealed class RunController : MonoBehaviour
    {
        public static RunController Instance { get; private set; }

        [Header("Content")]
        [SerializeField] private LevelSequence _levels;

        [Header("Channels (raised by the level)")]
        [SerializeField] private VoidEventChannel _levelCompleted;
        [SerializeField] private VoidEventChannel _playerDied;
        [Tooltip("Optional: accumulates collectibles across the whole run.")]
        [SerializeField] private IntEventChannel _collectiblePicked;

        [Header("Timing")]
        [SerializeField] private float _levelCompleteDelaySeconds = 1.2f;
        [SerializeField] private float _respawnDelaySeconds = 1.2f;

        /// <summary>Progress for the current run. Read-only to everyone but this controller.</summary>
        public RunState State { get; } = new();

        private bool _transitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            WarnAboutMissingWiring();
        }

        /// <summary>Fail loudly at boot rather than silently doing nothing at the exit (Guide §3.6).</summary>
        private void WarnAboutMissingWiring()
        {
            if (_levels == null)
            {
                Debug.LogWarning($"[{nameof(RunController)}] No LevelSequence assigned — levels cannot advance.", this);
            }
            else if (_levels.Count < 2)
            {
                Debug.LogWarning($"[{nameof(RunController)}] LevelSequence has {_levels.Count} entry — " +
                                 "completing it counts as finishing the run, so nothing will load. Add a second level.", this);
            }

            if (_levelCompleted == null)
            {
                Debug.LogWarning($"[{nameof(RunController)}] 'Level Completed' channel not assigned — will never advance.", this);
            }

            if (_playerDied == null)
            {
                Debug.LogWarning($"[{nameof(RunController)}] 'Player Died' channel not assigned — deaths will not reload.", this);
            }
        }

        private void OnEnable()
        {
            if (_levelCompleted != null) _levelCompleted.Subscribe(OnLevelCompleted);
            if (_playerDied != null) _playerDied.Subscribe(OnPlayerDied);
            if (_collectiblePicked != null) _collectiblePicked.Subscribe(OnCollectiblePicked);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            if (_levelCompleted != null) _levelCompleted.Unsubscribe(OnLevelCompleted);
            if (_playerDied != null) _playerDied.Unsubscribe(OnPlayerDied);
            if (_collectiblePicked != null) _collectiblePicked.Unsubscribe(OnCollectiblePicked);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyEarnedUpgrades();

        /// <summary>
        /// Re-applies every upgrade earned so far to the freshly-loaded level's player. Each level
        /// is a new scene with a new player instance, so this must be cumulative — otherwise the
        /// robot would reset to base stats at the start of every level.
        /// </summary>
        private void ApplyEarnedUpgrades()
        {
            if (_levels == null || State.LevelIndex <= 0) return;

            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            for (int i = 1; i <= State.LevelIndex; i++)
            {
                var upgrade = _levels.GetUpgradeOnEnter(i);
                if (upgrade != null) UpgradeService.Apply(upgrade, player);
            }
        }

        private void OnCollectiblePicked(int amount) => State.AddCollectibles(amount);

        private void OnLevelCompleted()
        {
            if (_transitioning) return;

            if (_levels == null)
            {
                Debug.LogWarning($"[{nameof(RunController)}] No LevelSequence assigned — cannot advance.", this);
                return;
            }

            if (_levels.IsLastLevel(State.LevelIndex))
            {
                Debug.Log($"[Run] Final level complete — collectibles: {State.Collectibles}. (Ending sequence lands in M4.)");
                return;
            }

            StartCoroutine(AdvanceToNextLevel());
        }

        private void OnPlayerDied()
        {
            if (_transitioning) return;
            StartCoroutine(ReloadCurrentLevel());
        }

        private IEnumerator AdvanceToNextLevel()
        {
            _transitioning = true;
            yield return new WaitForSeconds(_levelCompleteDelaySeconds);

            State.AdvanceLevel();

            // Record once per advance (not per reload) so the run's upgrade list stays accurate.
            var earned = _levels.GetUpgradeOnEnter(State.LevelIndex);
            if (earned != null)
            {
                State.RecordUpgrade(earned.Id);
                Debug.Log($"[Run] Upgrade earned: {earned.DisplayName}");
            }

            string scene = _levels.GetSceneName(State.LevelIndex);
            Debug.Log($"[Run] Level {State.LevelIndex} → loading '{scene}'.");

            _transitioning = false;
            SceneManager.LoadScene(scene);
        }

        private IEnumerator ReloadCurrentLevel()
        {
            _transitioning = true;
            yield return new WaitForSeconds(_respawnDelaySeconds);

            _transitioning = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
