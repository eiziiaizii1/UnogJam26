using System;
using Game.Core.Combat;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>Damage math is pure, so it pins down cheaply (Guide §7).</summary>
    public sealed class HealthTests
    {
        [Test]
        public void StartsAtFull()
        {
            var health = new Health(5);
            Assert.AreEqual(5, health.Current);
            Assert.AreEqual(5, health.Max);
            Assert.IsTrue(health.IsAlive);
        }

        [Test]
        public void TakeDamage_Reduces()
        {
            var health = new Health(5);
            health.TakeDamage(2);
            Assert.AreEqual(3, health.Current);
            Assert.IsTrue(health.IsAlive);
        }

        [Test]
        public void TakeDamage_ClampsAtZeroAndDies()
        {
            var health = new Health(3);
            int diedCount = 0;
            health.Died += () => diedCount++;

            health.TakeDamage(10);

            Assert.AreEqual(0, health.Current);
            Assert.IsFalse(health.IsAlive);
            Assert.AreEqual(1, diedCount);
        }

        [Test]
        public void Died_FiresOnce_EvenWithExtraDamage()
        {
            var health = new Health(2);
            int diedCount = 0;
            health.Died += () => diedCount++;

            health.TakeDamage(2);
            health.TakeDamage(2);

            Assert.AreEqual(1, diedCount);
        }

        [Test]
        public void Heal_ClampsAtMax()
        {
            var health = new Health(5);
            health.TakeDamage(3);
            health.Heal(10);
            Assert.AreEqual(5, health.Current);
        }

        [Test]
        public void Heal_DoesNothingWhenDead()
        {
            var health = new Health(2);
            health.TakeDamage(2);
            health.Heal(1);
            Assert.AreEqual(0, health.Current);
            Assert.IsFalse(health.IsAlive);
        }

        [Test]
        public void NegativeDamage_Throws()
        {
            var health = new Health(3);
            Assert.Throws<ArgumentOutOfRangeException>(() => health.TakeDamage(-1));
        }
    }
}
