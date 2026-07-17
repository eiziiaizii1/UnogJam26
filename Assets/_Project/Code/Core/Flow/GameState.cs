namespace Game.Core.Flow
{
    /// <summary>
    /// Application-level flow states (Guide §5.6). Boot wires services, Menu is the
    /// front end, Gameplay runs a level, Ending plays the world-collapse finale.
    /// </summary>
    public enum GameState
    {
        Boot,
        Menu,
        Gameplay,
        Ending,
    }
}
