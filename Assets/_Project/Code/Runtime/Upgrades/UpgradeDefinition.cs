using Game.Core.Data;
using UnityEngine;

namespace Game.Runtime.Upgrades
{
    /// <summary>
    /// An authored upgrade the robot gains between levels (Guide §5.5 — content is data).
    /// Upgrades are predetermined per level and auto-applied; there is no player choice, so this
    /// asset is the whole feature. Bonuses are additive on top of the player's authored base stats.
    /// </summary>
    [CreateAssetMenu(menuName = "IloveNature/Upgrade Definition", fileName = "Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject, IValidatable
    {
        [Tooltip("Stable id recorded in the run (also shown in logs).")]
        [SerializeField] private string _id = "upgrade";
        [Tooltip("Shown by the level-up indicator.")]
        [SerializeField] private string _displayName = "Upgrade";

        [Header("Bonuses (added to the player's base stats)")]
        [SerializeField] private int _bonusMaxHealth;
        [SerializeField] private float _bonusWalkSpeed;
        [SerializeField] private float _bonusJumpSpeed;

        public string Id => _id;
        public string DisplayName => _displayName;
        public int BonusMaxHealth => _bonusMaxHealth;
        public float BonusWalkSpeed => _bonusWalkSpeed;
        public float BonusJumpSpeed => _bonusJumpSpeed;

        private void OnValidate()
        {
            _bonusMaxHealth = Mathf.Max(0, _bonusMaxHealth);
            _bonusWalkSpeed = Mathf.Max(0f, _bonusWalkSpeed);
            _bonusJumpSpeed = Mathf.Max(0f, _bonusJumpSpeed);
        }

        public bool Validate(out string error)
        {
            if (string.IsNullOrWhiteSpace(_id))
            {
                error = $"{name}: Id must not be empty.";
                return false;
            }

            if (_bonusMaxHealth == 0 && Mathf.Approximately(_bonusWalkSpeed, 0f) && Mathf.Approximately(_bonusJumpSpeed, 0f))
            {
                error = $"{name}: grants no bonus at all.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
