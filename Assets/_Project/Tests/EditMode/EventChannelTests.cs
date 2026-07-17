using Game.Core.Events;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    /// <summary>Verifies the observer contract the whole game will rely on (Guide §4.2).</summary>
    public sealed class EventChannelTests
    {
        [Test]
        public void Raise_InvokesSubscriber()
        {
            var channel = new EventChannel<int>();
            int received = 0;
            channel.Subscribe(v => received = v);

            channel.Raise(42);

            Assert.AreEqual(42, received);
        }

        [Test]
        public void Unsubscribe_StopsDelivery()
        {
            var channel = new EventChannel<int>();
            int count = 0;
            void Handler(int _) => count++;
            channel.Subscribe(Handler);

            channel.Raise(1);
            channel.Unsubscribe(Handler);
            channel.Raise(1);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void Subscribe_IsIdempotent()
        {
            var channel = new EventChannel<int>();
            int count = 0;
            void Handler(int _) => count++;
            channel.Subscribe(Handler);
            channel.Subscribe(Handler);

            channel.Raise(1);

            Assert.AreEqual(1, count);
        }
    }
}
