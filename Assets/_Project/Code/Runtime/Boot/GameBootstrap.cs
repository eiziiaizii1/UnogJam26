using Game.Core.Flow;
using UnityEngine;

namespace Game.Runtime.Boot
{
    /// <summary>
    /// The single composition root (Guide §5.2). Constructs services, wires the game-flow
    /// state machine in a documented order, then drives the opening transitions. It
    /// auto-installs before the first scene loads so the game boots from ANY scene during
    /// the jam — pressing Play in a sandbox scene brings the whole flow up correctly.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private GameFlow _flow;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureExists()
        {
            if (FindAnyObjectByType<GameBootstrap>() != null) return;

            var host = new GameObject(nameof(GameBootstrap));
            host.AddComponent<GameBootstrap>();
        }

        private void Awake()
        {
            // Guard against a second instance surviving an additive load or a manual placement.
            if (FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Compose();
        }

        private void Compose()
        {
            _flow = new GameFlow();
            _flow.StateChanged += OnStateChanged;

            // Services are constructed and brought up here in a documented order (Guide §5.3)
            // as later milestones add them. M0 has no gameplay services yet.

            float volume = PlayerPrefs.GetFloat("Volume", 1f);
            AudioListener.volume = volume;

            int vsync = PlayerPrefs.GetInt("VSync", 1);
            QualitySettings.vSyncCount = vsync;

            int limit = PlayerPrefs.GetInt("FrameLimit", -1);
            Application.targetFrameRate = limit;

            _flow.Enter(GameState.Menu);

            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLower();
            if (activeScene != "mainmenu" && activeScene != "boot" && activeScene != "boot 1")
            {
                _flow.Enter(GameState.Gameplay);
            }
        }

        private void OnStateChanged(GameState from, GameState to)
        {
            Debug.Log($"[GameBootstrap] Flow {from} -> {to}");
        }

        private void OnDestroy()
        {
            if (_flow != null)
            {
                _flow.StateChanged -= OnStateChanged;
            }
        }
    }
}
