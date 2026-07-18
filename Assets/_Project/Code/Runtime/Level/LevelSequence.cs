using System;
using Game.Core.Data;
using Game.Runtime.Upgrades;
using UnityEngine;

namespace Game.Runtime.Level
{
    /// <summary>
    /// The authored order of levels for a run, and the upgrade granted on entering each one
    /// (Guide §5.5 — content is data). Designers edit this asset instead of anyone editing code.
    /// </summary>
    [CreateAssetMenu(menuName = "IloveNature/Level Sequence", fileName = "LevelSequence")]
    public sealed class LevelSequence : ScriptableObject, IValidatable
    {
        [Serializable]
        public sealed class LevelEntry
        {
            [Tooltip("Scene name — must also be listed in File ▸ Build Settings.")]
            public string SceneName;

            [Tooltip("Upgrade auto-granted when ENTERING this level. Leave empty on the first level.")]
            public UpgradeDefinition UpgradeOnEnter;
        }

        [SerializeField] private LevelEntry[] _levels;

        public int Count => _levels?.Length ?? 0;

        /// <summary>True when <paramref name="index"/> addresses a real level.</summary>
        public bool IsValidIndex(int index) => index >= 0 && index < Count;

        /// <summary>True when this is the final level — completing it ends the run.</summary>
        public bool IsLastLevel(int index) => index >= Count - 1;

        public string GetSceneName(int index) => IsValidIndex(index) ? _levels[index].SceneName : null;

        /// <summary>The upgrade granted on entering this level (null for the first level).</summary>
        public UpgradeDefinition GetUpgradeOnEnter(int index) => IsValidIndex(index) ? _levels[index].UpgradeOnEnter : null;

        public bool Validate(out string error)
        {
            if (Count == 0)
            {
                error = $"{name}: no levels listed.";
                return false;
            }

            for (int i = 0; i < _levels.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(_levels[i].SceneName))
                {
                    error = $"{name}: entry {i} has no scene name.";
                    return false;
                }
            }

            error = null;
            return true;
        }
    }
}
