using System;
using Game.Core.Flow;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>Boot smoke test: the flow state machine reaches gameplay legally (Guide §7).</summary>
    public sealed class GameFlowTests
    {
        [Test]
        public void StartsInBoot()
        {
            var flow = new GameFlow();
            Assert.AreEqual(GameState.Boot, flow.Current);
        }

        [Test]
        public void BootReachesGameplayThroughMenu()
        {
            var flow = new GameFlow();

            flow.Enter(GameState.Menu);
            flow.Enter(GameState.Gameplay);

            Assert.AreEqual(GameState.Gameplay, flow.Current);
        }

        [Test]
        public void StateChanged_FiresWithFromAndTo()
        {
            var flow = new GameFlow();
            GameState? from = null;
            GameState? to = null;
            flow.StateChanged += (f, t) => { from = f; to = t; };

            flow.Enter(GameState.Menu);

            Assert.AreEqual(GameState.Boot, from);
            Assert.AreEqual(GameState.Menu, to);
        }

        [Test]
        public void IllegalTransition_Throws()
        {
            var flow = new GameFlow();

            Assert.Throws<InvalidOperationException>(() => flow.Enter(GameState.Ending));
        }
    }
}
