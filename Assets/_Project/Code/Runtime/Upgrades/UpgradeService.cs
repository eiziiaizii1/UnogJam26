using Game.Runtime.Combat;
using Game.Runtime.Player;
using UnityEngine;

namespace Game.Runtime.Upgrades
{
    /// <summary>
    /// Applies an <see cref="UpgradeDefinition"/> to a player instance. Stateless by design — the
    /// run's progress lives in RunState, and because each level is a fresh scene with a fresh
    /// player, the caller re-applies every earned upgrade on each load.
    /// </summary>
    public static class UpgradeService
    {
        public static void Apply(UpgradeDefinition upgrade, GameObject player)
        {
            if (upgrade == null || player == null) return;

            if (upgrade.BonusMaxHealth > 0 && player.TryGetComponent<HealthComponent>(out var health))
            {
                health.AddMaxHealth(upgrade.BonusMaxHealth);
            }

            if (player.TryGetComponent<PlayerMotor>(out var motor))
            {
                motor.AddSpeedBonuses(upgrade.BonusWalkSpeed, upgrade.BonusJumpSpeed);
            }
        }
    }
}
