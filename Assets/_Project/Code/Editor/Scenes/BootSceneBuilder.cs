using System.Collections.Generic;
using System.IO;
using Game.Runtime.Boot;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.Editor.Scenes
{
    /// <summary>
    /// Generates the persistent Boot scene programmatically (Guide §5.2, §11.5) so it is
    /// never hand-authored. Creates an empty scene holding a single <see cref="GameBootstrap"/>
    /// and registers it as build scene 0. Run once via the menu.
    /// </summary>
    public static class BootSceneBuilder
    {
        private const string SceneDirectory = "Assets/_Project/Scenes";
        private const string ScenePath = SceneDirectory + "/Boot.unity";

        [MenuItem("Tools/IloveNature/Create Boot Scene")]
        public static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrap = new GameObject(nameof(GameBootstrap), typeof(GameBootstrap));
            Undo.RegisterCreatedObjectUndo(bootstrap, "Create GameBootstrap");

            if (!Directory.Exists(SceneDirectory))
            {
                Directory.CreateDirectory(SceneDirectory);
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            RegisterAsFirstBuildScene(ScenePath);

            Debug.Log($"[BootSceneBuilder] Created {ScenePath} and set it as build scene 0.");
        }

        private static void RegisterAsFirstBuildScene(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
