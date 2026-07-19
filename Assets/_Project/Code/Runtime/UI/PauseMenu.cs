using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Runtime.Level;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Controller for the in-game Pause Menu.
    /// Intercepts Escape key presses, pauses game logic, and handles options, VSync, target frame rate,
    /// returning to the main menu (resets run), and exiting.
    /// </summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _pausePanel;

        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _exitButton;

        [Header("Settings Controls")]
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Toggle _vSyncToggle;
        [SerializeField] private TMP_Dropdown _frameLimitDropdown;

        private const string VolumePrefKey = "Volume";
        private const string VSyncPrefKey = "VSync";
        private const string FrameLimitPrefKey = "FrameLimit";
        private bool _isPaused;

        private void Start()
        {
            // Reset state
            _isPaused = false;
            if (_pausePanel != null) _pausePanel.SetActive(false);

            // Wire buttons
            if (_resumeButton != null) _resumeButton.onClick.AddListener(Resume);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            if (_exitButton != null) _exitButton.onClick.AddListener(ExitGame);

            // Load and apply volume
            float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
            AudioListener.volume = savedVolume;
            if (_volumeSlider != null)
            {
                _volumeSlider.minValue = 0f;
                _volumeSlider.maxValue = 1f;
                _volumeSlider.value = savedVolume;
                _volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }

            // Load and apply VSync
            int savedVSync = PlayerPrefs.GetInt(VSyncPrefKey, 1);
            QualitySettings.vSyncCount = savedVSync;
            if (_vSyncToggle != null)
            {
                _vSyncToggle.isOn = savedVSync > 0;
                _vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            }

            // Load and apply Frame Limit
            int savedLimit = PlayerPrefs.GetInt(FrameLimitPrefKey, -1);
            Application.targetFrameRate = savedLimit;
            if (_frameLimitDropdown != null)
            {
                _frameLimitDropdown.value = GetDropdownIndexFromFrameLimit(savedLimit);
                _frameLimitDropdown.onValueChanged.AddListener(OnFrameLimitChanged);
            }
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.Escape))
            {
                if (_isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }

        public void Pause()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            if (_pausePanel != null) _pausePanel.SetActive(true);
        }

        public void Resume()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            if (_pausePanel != null) _pausePanel.SetActive(false);
        }

        private void ReturnToMainMenu()
        {
            // Reset TimeScale before leaving scene
            Time.timeScale = 1f;

            // Reset current run controller state to avoid carrying upgrades/progress over
            if (RunController.Instance != null)
            {
                Destroy(RunController.Instance.gameObject);
            }

            // Transition
            if (ScreenTransition.Instance != null)
            {
                ScreenTransition.Instance.TransitionToScene("mainmenu");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("mainmenu");
            }
        }

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;
            PlayerPrefs.SetFloat(VolumePrefKey, value);
            PlayerPrefs.Save();
        }

        private void OnVSyncChanged(bool isOn)
        {
            int value = isOn ? 1 : 0;
            QualitySettings.vSyncCount = value;
            PlayerPrefs.SetInt(VSyncPrefKey, value);
            PlayerPrefs.Save();
        }

        private void OnFrameLimitChanged(int index)
        {
            int limit = GetFrameLimitFromDropdownIndex(index);
            Application.targetFrameRate = limit;
            PlayerPrefs.SetInt(FrameLimitPrefKey, limit);
            PlayerPrefs.Save();
        }

        private int GetFrameLimitFromDropdownIndex(int index)
        {
            return index switch
            {
                0 => 30,
                1 => 60,
                2 => 120,
                3 => 144,
                _ => -1 // Index 4 / default = Unlimited
            };
        }

        private int GetDropdownIndexFromFrameLimit(int limit)
        {
            return limit switch
            {
                30 => 0,
                60 => 1,
                120 => 2,
                144 => 3,
                _ => 4 // Unlimited
            };
        }

        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (_resumeButton != null) _resumeButton.onClick.RemoveListener(Resume);
            if (_mainMenuButton != null) _mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            if (_exitButton != null) _exitButton.onClick.RemoveListener(ExitGame);
            if (_volumeSlider != null) _volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            if (_vSyncToggle != null) _vSyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
            if (_frameLimitDropdown != null) _frameLimitDropdown.onValueChanged.RemoveListener(OnFrameLimitChanged);
        }
    }
}
