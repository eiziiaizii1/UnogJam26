using UnityEngine;
using UnityEngine.Pool;

namespace Game.Runtime.Audio
{
    /// <summary>
    /// A pooled AudioSource wrapper. Automatically returns itself to the pool when playback ends,
    /// avoiding allocation and runtime overhead.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class PooledAudioSource : MonoBehaviour
    {
        private AudioSource _source;
        private IObjectPool<PooledAudioSource> _pool;
        private float _releaseTime;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
        }

        public void Play(AudioClip clip, float volume, float pitch, UnityEngine.Audio.AudioMixerGroup group, IObjectPool<PooledAudioSource> pool)
        {
            _pool = pool;
            _source.clip = clip;
            _source.volume = volume;
            _source.pitch = pitch;
            _source.outputAudioMixerGroup = group;
            _source.Play();
            
            _releaseTime = Time.time + (clip.length / Mathf.Max(0.001f, pitch));
        }

        private void Update()
        {
            if (Time.time >= _releaseTime)
            {
                _pool.Release(this);
            }
        }
    }
}
