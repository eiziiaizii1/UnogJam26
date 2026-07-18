using Game.Core.Run;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>Run progression is pure, so it pins down cheaply (Guide §7).</summary>
    public sealed class RunStateTests
    {
        [Test]
        public void StartsAtFirstLevelWithNothingCollected()
        {
            var run = new RunState();
            Assert.AreEqual(0, run.LevelIndex);
            Assert.AreEqual(0, run.Collectibles);
            Assert.IsEmpty(run.AppliedUpgrades);
        }

        [Test]
        public void AdvanceLevel_IncrementsAndNotifies()
        {
            var run = new RunState();
            int notified = -1;
            run.LevelIndexChanged += index => notified = index;

            run.AdvanceLevel();

            Assert.AreEqual(1, run.LevelIndex);
            Assert.AreEqual(1, notified);
        }

        [Test]
        public void AddCollectibles_Accumulates_AndIgnoresNonPositive()
        {
            var run = new RunState();
            run.AddCollectibles(3);
            run.AddCollectibles(2);
            run.AddCollectibles(0);
            run.AddCollectibles(-5);

            Assert.AreEqual(5, run.Collectibles);
        }

        [Test]
        public void RecordUpgrade_KeepsOrder_AndIgnoresEmpty()
        {
            var run = new RunState();
            run.RecordUpgrade("rapid_fire");
            run.RecordUpgrade("");
            run.RecordUpgrade("armour");

            Assert.AreEqual(2, run.AppliedUpgrades.Count);
            Assert.AreEqual("rapid_fire", run.AppliedUpgrades[0]);
            Assert.AreEqual("armour", run.AppliedUpgrades[1]);
        }

        [Test]
        public void Reset_ClearsEverything()
        {
            var run = new RunState();
            run.AdvanceLevel();
            run.AddCollectibles(4);
            run.RecordUpgrade("armour");

            run.Reset();

            Assert.AreEqual(0, run.LevelIndex);
            Assert.AreEqual(0, run.Collectibles);
            Assert.IsEmpty(run.AppliedUpgrades);
        }
    }
}
