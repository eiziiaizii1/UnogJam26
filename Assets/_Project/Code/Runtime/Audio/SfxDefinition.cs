using Game.Core.Data;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Runtime.Audio
{
    /// <summary>
    /// Authored sound archetype (Flyweight). Pick one clip at random, with pitch variation.
    /// Implements <see cref="IValidatable"/> so the data validator can self-check it.
    /// </summary>
    [CreateAssetMenu(menuName = "IloveNature/Sfx Definition", fileName = "SfxDefinition")]
    public sealed class SfxDefinition : ScriptableObject, IValidatable
    {
        [SerializeField] private AudioClip[] _clips;
        
        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;

        [Range(0.1f, 3f)]
        [SerializeField] private float _pitchMin = 0.9f;

        [Range(0.1f, 3f)]
        [SerializeField] private float _pitchMax = 1.1f;

        [SerializeField] private AudioMixerGroup _outputGroup;

        public AudioClip[] Clips => _clips;
        public float Volume => _volume;
        public float PitchMin => _pitchMin;
        public float PitchMax => _pitchMax;
        public AudioMixerGroup OutputGroup => _outputGroup;

        private void OnValidate()
        {
            _volume = Mathf.Clamp01(_volume);
            _pitchMin = Mathf.Clamp(_pitchMin, 0.1f, 3f);
            _pitchMax = Mathf.Clamp(_pitchMax, 0.1f, 3f);
            if (_pitchMin > _pitchMax)
            {
                _pitchMax = _pitchMin;
            }
        }

        public bool Validate(out string error)
        {
            if (_clips == null || _clips.Length == 0)
            {
                error = $"{name}: Clips array cannot be empty.";
                return false;
            }

            for (int i = 0; i < _clips.Length; i++)
            {
                if (_clips[i] == null)
                {
                    error = $"{name}: Clip at index {i} is null.";
                    return false;
                }
            }

            if (_pitchMin > _pitchMax)
            {
                error = $"{name}: PitchMin ({_pitchMin}) cannot be greater than PitchMax ({_pitchMax}).";
                return false;
            }

            error = null;
            return true;
        }

        public AudioClip GetRandomClip()
        {
            if (_clips == null || _clips.Length == 0) return null;
            return _clips[Random.Range(0, _clips.Length)];
        }
    }
}
