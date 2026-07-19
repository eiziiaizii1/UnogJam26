using UnityEngine;
using UnityEngine.Pool;
using Game.Runtime.Events;

namespace Game.Runtime.Audio
{
    /// <summary>
    /// Central scene service that manages a pre-warmed pool of <see cref="PooledAudioSource"/>s.
    /// Observers can play SFX cues. Automatically handles central event hooks like Pickups.
    /// </summary>
    public sealed class SfxPlayer : MonoBehaviour
    {
        public static SfxPlayer Instance { get; private set; }

        [Header("Pool Settings")]
        [SerializeField] private int _prewarmCount = 12;
        [SerializeField] private int _maxSize = 48;

        [Header("Pickup Event Hook")]
        [SerializeField] private IntEventChannel _collectiblePickedChannel;
        [SerializeField] private SfxDefinition _pickupSfx;

        private IObjectPool<PooledAudioSource> _pool;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _pool = new ObjectPool<PooledAudioSource>(
                createFunc: CreateAudioSourceInstance,
                actionOnGet: s => s.gameObject.SetActive(true),
                actionOnRelease: s => s.gameObject.SetActive(false),
                actionOnDestroy: s => Destroy(s.gameObject),
                collectionCheck: false,
                defaultCapacity: _prewarmCount,
                maxSize: _maxSize
            );

            Prewarm();
        }

        private void OnEnable()
        {
            if (_collectiblePickedChannel != null)
            {
                _collectiblePickedChannel.Subscribe(OnCollectiblePicked);
            }
        }

        private void OnDisable()
        {
            if (_collectiblePickedChannel != null)
            {
                _collectiblePickedChannel.Unsubscribe(OnCollectiblePicked);
            }
        }

        private void Prewarm()
        {
            var warmed = new PooledAudioSource[_prewarmCount];
            for (int i = 0; i < _prewarmCount; i++) warmed[i] = _pool.Get();
            for (int i = 0; i < _prewarmCount; i++) _pool.Release(warmed[i]);
        }

        private PooledAudioSource CreateAudioSourceInstance()
        {
            var go = new GameObject("PooledAudioSource");
            go.transform.SetParent(transform);
            
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // Force 2D sound for jam UI / gameplay feedback

            var pooled = go.AddComponent<PooledAudioSource>();
            return pooled;
        }

        /// <summary>
        /// Plays a sound cue from the pre-warmed AudioSource pool.
        /// </summary>
        public void Play(SfxDefinition cue)
        {
            if (cue == null) return;

            var clip = cue.GetRandomClip();
            if (clip == null) return;

            float pitch = Random.Range(cue.PitchMin, cue.PitchMax);
            
            var source = _pool.Get();
            source.Play(clip, cue.Volume, pitch, cue.OutputGroup, _pool);
        }

        private void OnCollectiblePicked(int amount)
        {
            Play(_pickupSfx);
        }

        /// <summary>
        /// Plays a cue on a throwaway <see cref="DontDestroyOnLoad"/> source so it survives a scene
        /// load. The pooled sources above live and die with their scene, which truncates any cue
        /// that outlasts a transition — the final explosion rings for 5s but the outro loads after
        /// 1.8s. Use sparingly; this is for one-shot cinematic beats, not gameplay SFX.
        /// </summary>
        public static void PlayDetached(SfxDefinition cue)
        {
            if (cue == null) return;

            var clip = cue.GetRandomClip();
            if (clip == null) return;

            var host = new GameObject("DetachedSfx_" + clip.name);
            DontDestroyOnLoad(host);

            var source = host.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = cue.Volume;
            source.pitch = Random.Range(cue.PitchMin, cue.PitchMax);
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = cue.OutputGroup;
            source.Play();

            // Pitch stretches playback time, so scale the lifetime by it or a slowed cue gets cut.
            Destroy(host, clip.length / Mathf.Max(0.01f, source.pitch) + 0.25f);
        }
    }
}
