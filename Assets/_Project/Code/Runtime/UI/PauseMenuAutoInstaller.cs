using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Runtime.UI
{
    /// <summary>
    /// Automatic scene loader helper that spawns the persistent ScreenTransition 
    /// and the level-specific PauseMenu prefabs from Resources on scene load.
    /// This eliminates the need for manual scene-by-scene canvas placement.
    /// </summary>
    public static class PauseMenuAutoInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnSceneLoaded()
        {
            string sceneName = SceneManager.GetActiveScene().name.ToLower();

            // 1. Skip installer on boots and menus
            if (sceneName == "boot" || sceneName == "boot 1" || sceneName == "mainmenu" || sceneName == "samplescene")
            {
                return;
            }

            // 2. Ensure ScreenTransition is present
            if (ScreenTransition.Instance == null)
            {
                var transitionPrefab = Resources.Load<GameObject>("ScreenTransition");
                if (transitionPrefab != null)
                {
                    Object.Instantiate(transitionPrefab);
                }
                else
                {
                    Debug.LogWarning("[PauseMenuAutoInstaller] ScreenTransition prefab not found in Resources folder.");
                }
            }

            // 3. Ensure PauseMenu is present
            if (Object.FindFirstObjectByType<PauseMenu>() == null)
            {
                var pausePrefab = Resources.Load<GameObject>("PauseMenu");
                if (pausePrefab != null)
                {
                    Object.Instantiate(pausePrefab);
                }
                else
                {
                    Debug.LogWarning("[PauseMenuAutoInstaller] PauseMenu prefab not found in Resources folder.");
                }
            }
        }
    }
}
