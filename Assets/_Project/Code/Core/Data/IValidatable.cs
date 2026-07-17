namespace Game.Core.Data
{
    /// <summary>
    /// Authored data (ScriptableObjects) that can self-check at import and boot (Guide §3.6).
    /// A broken config should scream at startup with a named culprit, not null-ref mid-playtest.
    /// </summary>
    public interface IValidatable
    {
        /// <returns>
        /// True when the asset is valid. Otherwise false, and <paramref name="error"/>
        /// describes the first problem found.
        /// </returns>
        bool Validate(out string error);
    }
}
