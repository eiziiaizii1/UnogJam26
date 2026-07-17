using Game.Core.Data;
using UnityEngine;

namespace Game.Runtime.Enemies
{
    /// <summary>
    /// Authored enemy archetype (Guide §4.1 Flyweight, §11.4). Adding enemy #2, #3, #4 is a new
    /// asset, not a new class (Pillar 4). Implements <see cref="IValidatable"/> so the boot/menu
    /// data validator names a broken archetype instead of null-reffing mid-playtest (§3.6).
    /// </summary>
    [CreateAssetMenu(menuName = "IloveNature/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject, IValidatable
    {
        [SerializeField] private string _displayName = "Enemy";
        [SerializeField] private int _maxHealth = 3;
        [SerializeField] private float _moveSpeed = 2f;
        [SerializeField] private int _contactDamage = 1;
        [Tooltip("Half the width of the back-and-forth patrol, in world units, centred on spawn.")]
        [SerializeField] private float _patrolHalfWidth = 3f;
        [SerializeField] private Color _tintColor = Color.white;
        [SerializeField] private Vector2 _size = Vector2.one;

        public string DisplayName => _displayName;
        public int MaxHealth => _maxHealth;
        public float MoveSpeed => _moveSpeed;
        public int ContactDamage => _contactDamage;
        public float PatrolHalfWidth => _patrolHalfWidth;
        public Color TintColor => _tintColor;
        public Vector2 Size => _size;

        private void OnValidate()
        {
            _maxHealth = Mathf.Max(1, _maxHealth);
            _moveSpeed = Mathf.Max(0f, _moveSpeed);
            _contactDamage = Mathf.Max(0, _contactDamage);
            _patrolHalfWidth = Mathf.Max(0f, _patrolHalfWidth);
        }

        public bool Validate(out string error)
        {
            if (_maxHealth <= 0) { error = $"{name}: MaxHealth must be positive."; return false; }
            if (_moveSpeed < 0f) { error = $"{name}: MoveSpeed must be non-negative."; return false; }
            if (_contactDamage < 0) { error = $"{name}: ContactDamage must be non-negative."; return false; }
            if (_patrolHalfWidth < 0f) { error = $"{name}: PatrolHalfWidth must be non-negative."; return false; }
            error = null;
            return true;
        }
    }
}
