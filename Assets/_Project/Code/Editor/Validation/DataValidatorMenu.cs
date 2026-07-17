using System.Text;
using Game.Core.Data;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Validation
{
    /// <summary>
    /// Boot-time / on-demand authored-data validator (Guide §3.6, §11.4). Scans every
    /// ScriptableObject implementing <see cref="IValidatable"/> and names the culprit on
    /// failure. Concrete validatable definitions arrive with content in M1+; until then
    /// this reports a clean project.
    /// </summary>
    public static class DataValidatorMenu
    {
        [MenuItem("Tools/IloveNature/Validate Data")]
        public static void ValidateAll()
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableObject");
            var report = new StringBuilder();
            int total = 0;
            int failed = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset is not IValidatable validatable) continue;

                total++;
                if (!validatable.Validate(out var error))
                {
                    failed++;
                    report.AppendLine($" - {path}: {error}");
                    Debug.LogError($"[DataValidator] {path}: {error}", asset);
                }
            }

            if (failed == 0)
            {
                Debug.Log($"[DataValidator] OK — {total} validatable asset(s) passed.");
            }
            else
            {
                Debug.LogError($"[DataValidator] {failed}/{total} asset(s) failed:\n{report}");
            }
        }
    }
}
