using System;
using System.Collections.Generic;

namespace Game.Core.Flow
{
    /// <summary>
    /// Application-level flow state machine (Guide §5.6). Pure and engine-free so
    /// transitions are unit-testable; all side effects (scene loads, UI) live in
    /// subscribers to <see cref="StateChanged"/>, never inside this type.
    /// </summary>
    public sealed class GameFlow
    {
        private static readonly Dictionary<GameState, GameState[]> Transitions = new()
        {
            { GameState.Boot, new[] { GameState.Menu } },
            { GameState.Menu, new[] { GameState.Gameplay } },
            { GameState.Gameplay, new[] { GameState.Ending, GameState.Menu } },
            { GameState.Ending, new[] { GameState.Menu } },
        };

        /// <summary>The state the flow is currently in. Starts at <see cref="GameState.Boot"/>.</summary>
        public GameState Current { get; private set; } = GameState.Boot;

        /// <summary>Raised after a successful transition as (previous, next).</summary>
        public event Action<GameState, GameState> StateChanged;

        /// <summary>True if <paramref name="next"/> is a legal transition from the current state.</summary>
        public bool CanEnter(GameState next)
        {
            return Transitions.TryGetValue(Current, out var allowed)
                   && Array.IndexOf(allowed, next) >= 0;
        }

        /// <summary>Transitions to <paramref name="next"/>, or throws on an illegal transition (fail fast, Guide §3.6).</summary>
        public void Enter(GameState next)
        {
            if (!CanEnter(next))
            {
                throw new InvalidOperationException($"Illegal game-flow transition {Current} -> {next}.");
            }

            var previous = Current;
            Current = next;
            StateChanged?.Invoke(previous, next);
        }
    }
}
