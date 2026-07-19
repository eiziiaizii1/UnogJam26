using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Controller for the Main Menu screen.
    /// Handles playing the game, settings panel toggles, volume settings, VSync, target frame rate, and game exit.
    /// </summary>
    public sealed class MainMenu : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private Button _settingsBackButton;

        [Header("Settings Controls")]
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Toggle _vSyncToggle;
        [SerializeField] private TMP_Dropdown _frameLimitDropdown;

        [Header("Level Options")]
        [SerializeField] private string _firstLevelSceneName = "Level_01";

        private const string VolumePrefKey = "Volume";
        private const string VSyncPrefKey = "VSync";
        private const string FrameLimitPrefKey = "FrameLimit";

        private void Start()
        {
            // Reset timescale in case we returned here while paused
            Time.timeScale = 1f;

            // Wire buttons
            if (_playButton != null) _playButton.onClick.AddListener(OnPlayPressed);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsPressed);
            if (_exitButton != null) _exitButton.onClick.AddListener(OnExitPressed);
            if (_settingsBackButton != null) _settingsBackButton.onClick.AddListener(OnSettingsBackPressed);

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

            // Initial UI state
            ShowMainMenu();
        }

        private void OnPlayPressed()
        {
            if (ScreenTransition.Instance != null)
            {
                ScreenTransition.Instance.TransitionToScene(_firstLevelSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(_firstLevelSceneName);
            }
        }

        private void OnSettingsPressed()
        {
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(true);
        }

        private void OnSettingsBackPressed()
        {
            ShowMainMenu();
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

        private void OnExitPressed()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowMainMenu()
        {
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(true);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_playButton != null) _playButton.onClick.RemoveListener(OnPlayPressed);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsPressed);
            if (_exitButton != null) _exitButton.onClick.RemoveListener(OnExitPressed);
            if (_settingsBackButton != null) _settingsBackButton.onClick.RemoveListener(OnSettingsBackPressed);
            if (_volumeSlider != null) _volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            if (_vSyncToggle != null) _vSyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
            if (_frameLimitDropdown != null) _frameLimitDropdown.onValueChanged.RemoveListener(OnFrameLimitChanged);
        }
    }
}
