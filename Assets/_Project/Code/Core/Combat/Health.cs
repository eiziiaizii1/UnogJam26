using System;

namespace Game.Core.Combat
{
    /// <summary>
    /// Pure, engine-free hit-point model (Guide §3.4, §7 — the damage math lives here so it
    /// unit-tests trivially). MonoBehaviour adapters own an instance and expose it to the engine.
    /// </summary>
    public sealed class Health
    {
        /// <summary>Maximum hit points, fixed at construction.</summary>
        public int Max { get; }

        /// <summary>Current hit points, in [0, Max].</summary>
        public int Current { get; private set; }

        /// <summary>True while <see cref="Current"/> is above zero.</summary>
        public bool IsAlive => Current > 0;

        /// <summary>Raised after any change as (current, max).</summary>
        public event Action<int, int> Changed;

        /// <summary>Raised exactly once, the moment hit points reach zero.</summary>
        public event Action Died;

        public Health(int max)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max), "Max health must be positive.");
            Max = max;
            Current = max;
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Damage must be non-negative.");
            if (!IsAlive || amount == 0) return;

            Current = Math.Max(0, Current - amount);
            Changed?.Invoke(Current, Max);
            if (Current == 0) Died?.Invoke();
        }

        public void Heal(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Heal must be non-negative.");
            if (!IsAlive || amount == 0) return;

            Current = Math.Min(Max, Current + amount);
            Changed?.Invoke(Current, Max);
        }
    }
}
