using System;
using System.Collections.Generic;

namespace Game.Core.Run
{
    /// <summary>
    /// The player's progress through one run (Guide §2.6 runtime state). Pure and engine-free so
    /// it unit-tests trivially, and owned by the persistent run controller so it survives scene
    /// loads. Nothing else may write it.
    /// </summary>
    public sealed class RunState
    {
        private readonly List<string> _appliedUpgrades = new();

        /// <summary>Zero-based index of the level currently being played.</summary>
        public int LevelIndex { get; private set; }

        /// <summary>Nature collectibles gathered across the whole run.</summary>
        public int Collectibles { get; private set; }

        /// <summary>Ids of upgrades granted so far, in order.</summary>
        public IReadOnlyList<string> AppliedUpgrades => _appliedUpgrades;

        /// <summary>Raised after <see cref="AdvanceLevel"/> with the new index.</summary>
        public event Action<int> LevelIndexChanged;

        public void AdvanceLevel()
        {
            LevelIndex++;
            LevelIndexChanged?.Invoke(LevelIndex);
        }

        public void AddCollectibles(int amount)
        {
            if (amount <= 0) return;
            Collectibles += amount;
        }

        public void RecordUpgrade(string upgradeId)
        {
            if (string.IsNullOrWhiteSpace(upgradeId)) return;
            _appliedUpgrades.Add(upgradeId);
        }

        /// <summary>Clears everything for a fresh run (menu → new game).</summary>
        public void Reset()
        {
            LevelIndex = 0;
            Collectibles = 0;
            _appliedUpgrades.Clear();
        }
    }
}
