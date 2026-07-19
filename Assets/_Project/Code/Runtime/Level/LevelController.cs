using System.Collections;
using Game.Runtime.Combat;
using Game.Runtime.Events;
using Game.Runtime.Player;
using UnityEngine;
using PrimeTween;

namespace Game.Runtime.Level
{
    /// <summary>
    /// Per-level lifecycle. Handles the in-level feedback (freeze/tint on completion, respawn on
    /// death) and <b>announces</b> both facts on event channels so the persistent
    /// <see cref="RunController"/> can sequence the run (Guide §5.4 — announce facts, don't issue orders).
    /// </summary>
    public sealed class LevelController : MonoBehaviour
    {
        [SerializeField] private GameObject _player;
        [SerializeField] private LevelExit _exit;
        [Tooltip("Player control behaviours frozen when the level is completed or during entrance (input, motor, shooter).")]
        [SerializeField] private Behaviour[] _playerControl;
        [SerializeField] private float _respawnDelaySeconds = 1.25f;
        [SerializeField] private Color _completeTint = new(0.40f, 0.90f, 0.50f);

        [Header("Run channels (leave empty to stay level-local)")]
        [SerializeField] private VoidEventChannel _levelCompleted;
        [SerializeField] private VoidEventChannel _playerDiedChannel;

        private HealthComponent _health;
        private PlayerDeath _death;
        private Rigidbody2D _body;
        private SpriteRenderer _renderer;
        private Vector3 _spawnPosition;
        private Vector3 _originalPlayerScale;
        private bool _completed;

        private void Awake()
        {
            _spawnPosition = _player.transform.position;
            _health = _player.GetComponent<HealthComponent>();
            _death = _player.GetComponent<PlayerDeath>();
            _body = _player.GetComponent<Rigidbody2D>();
            _renderer = _player.GetComponent<SpriteRenderer>();
            _originalPlayerScale = _player.transform.localScale;
        }

        private void Start()
        {
            PlayEntranceAnimation();

            // Shift trigger offset 1.2 units to the left on Level_04 so player explodes before reaching the portal
            bool isFinalLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_04";
            if (isFinalLevel && _exit != null)
            {
                var boxCol = _exit.GetComponent<BoxCollider2D>();
                if (boxCol != null)
                {
                    boxCol.offset = new Vector2(-1.2f, 0f);
                }
            }
        }

        private void OnEnable()
        {
            if (_exit != null) _exit.Reached += OnExitReached;
            if (_death != null) _death.Died += OnPlayerDied;
        }

        private void OnDisable()
        {
            if (_exit != null) _exit.Reached -= OnExitReached;
            if (_death != null) _death.Died -= OnPlayerDied;
        }

        private void PlayEntranceAnimation()
        {
            // Reset rotation
            _player.transform.localRotation = Quaternion.identity;

            // 1. Disable player controls during entrance animation
            if (_playerControl != null)
            {
                foreach (var behaviour in _playerControl)
                {
                    if (behaviour != null) behaviour.enabled = false;
                }
            }

            // 2. Set scale to zero
            _player.transform.localScale = Vector3.zero;

            // 3. Scale up to original scale with a nice spring bounce
            Tween.Scale(_player.transform, _originalPlayerScale, 0.5f, Ease.OutBack)
                .OnComplete(() =>
                {
                    // Re-enable controls
                    if (_playerControl != null)
                    {
                        foreach (var behaviour in _playerControl)
                        {
                            if (behaviour != null) behaviour.enabled = true;
                        }
                    }
                });
        }

        private void OnExitReached()
        {
            if (_completed) return;
            _completed = true;
            Debug.Log("[Level] COMPLETE — reached the exit!");

            // Check if it's the final level (Level_04)
            bool isFinalLevel = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Level_04";

            if (isFinalLevel)
            {
                StartCoroutine(FinalExplosionRoutine());
                return;
            }

            // 1. Freeze Rigidbody
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            // 2. Disable controls
            if (_playerControl != null)
            {
                foreach (var behaviour in _playerControl)
                {
                    if (behaviour != null) behaviour.enabled = false;
                }
            }

            if (_renderer != null) _renderer.color = _completeTint;

            // 3. Play exit animation: Move to portal center, spin, shrink to zero
            Vector3 portalPos = _exit != null ? _exit.transform.position : _player.transform.position;
            float duration = 0.55f;

            // Align Z position to player Z to avoid depth sorting issues during tween
            portalPos.z = _player.transform.position.z;

            Tween.Position(_player.transform, portalPos, duration, Ease.InOutQuad);
            Tween.Rotation(_player.transform, new Vector3(0f, 0f, 720f), duration, Ease.InQuad);
            Tween.Scale(_player.transform, Vector3.zero, duration, Ease.InBack)
                .OnComplete(() =>
                {
                    // 4. Raise completed event to trigger transition to next scene
                    if (_levelCompleted != null)
                    {
                        _levelCompleted.Raise();
                    }
                    else
                    {
                        Debug.LogWarning($"[{nameof(LevelController)}] 'Level Completed' channel is NOT assigned — " +
                                         "the run cannot advance. Assign it in the Inspector to chain levels.", this);
                    }

                    if (RunController.Instance == null)
                    {
                        Debug.LogWarning($"[{nameof(LevelController)}] No {nameof(RunController)} in the scene — " +
                                         "nothing is listening, so the level cannot advance.", this);
                    }
                });
        }

        private IEnumerator FinalExplosionRoutine()
        {
            // 1. Freeze player controls and physics
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.constraints = RigidbodyConstraints2D.FreezeAll;
            }
            if (_playerControl != null)
            {
                foreach (var behaviour in _playerControl)
                {
                    if (behaviour != null) behaviour.enabled = false;
                }
            }

            // 2. Spawn charging particles from left and right
            var chargePsLeft = CreateChargeParticles(-0.4f);
            var chargePsRight = CreateChargeParticles(0.4f);

            // Camera shakes with increasing intensity during charge up (1.5s)
            float elapsed = 0f;
            float duration = 1.5f;
            var cameraFollow = Camera.main != null ? Camera.main.GetComponent<Game.Runtime.Presentation.CameraFollow>() : null;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (cameraFollow != null)
                {
                    cameraFollow.Shake(0.12f + (elapsed / duration) * 0.35f, 5f);
                }
                yield return null;
            }

            // Destroy charging particles
            if (chargePsLeft != null) Destroy(chargePsLeft.gameObject);
            if (chargePsRight != null) Destroy(chargePsRight.gameObject);

            // 3. EXPLODE!
            var explosionPs = CreateExplosionParticles();

            // Hide Player Visuals / Sprite Renderer
            var visuals = _player.transform.Find("Visuals");
            if (visuals != null) visuals.gameObject.SetActive(false);
            else if (_renderer != null) _renderer.enabled = false;

            // Heavy screen shake on explosion
            if (cameraFollow != null)
            {
                cameraFollow.Shake(1.3f, 2f);
            }

            // Wait for explosion to finish
            yield return new WaitForSeconds(1.2f);

            if (explosionPs != null) Destroy(explosionPs.gameObject);

            // 4. Load Credits scene
            if (_levelCompleted != null)
            {
                _levelCompleted.Raise();
            }
        }

        private ParticleSystem CreateChargeParticles(float xOffset)
        {
            GameObject go = new GameObject("ChargeParticles");
            go.transform.SetParent(_player.transform);
            go.transform.localPosition = new Vector3(xOffset, -0.2f, 0f);

            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 2f;
            main.loop = true;
            main.startLifetime = 0.45f;
            main.startSpeed = 2.5f;
            main.startSize = 0.15f;
            main.gravityModifier = -0.25f; // Rise up
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 35f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.radius = 0.08f;
            shape.angle = 12f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(Color.red, 0f), 
                    new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f), 
                    new GradientColorKey(Color.yellow, 1f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0f), 
                    new GradientAlphaKey(0.85f, 0.8f), 
                    new GradientAlphaKey(0f, 1f) 
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sortingOrder = 15;
            var sr = _player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) r.sharedMaterial = sr.sharedMaterial;

            ps.Play();
            return ps;
        }

        private ParticleSystem CreateExplosionParticles()
        {
            GameObject go = new GameObject("ExplosionParticles");
            go.transform.position = _player.transform.position;

            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 1.2f;
            main.loop = false;
            main.startLifetime = 0.9f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 7.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.32f);
            main.gravityModifier = 0.08f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.burstCount = 1;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 70) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.yellow, 0.25f), 
                    new GradientColorKey(new Color(1f, 0.35f, 0f), 0.65f), 
                    new GradientColorKey(Color.red, 1f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1.0f, 0f), 
                    new GradientAlphaKey(0.9f, 0.6f), 
                    new GradientAlphaKey(0f, 1f) 
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 1f);
            curve.AddKey(1f, 0.15f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

            var r = go.GetComponent<ParticleSystemRenderer>();
            r.sortingOrder = 20;
            var sr = _player.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) r.sharedMaterial = sr.sharedMaterial;

            ps.Play();
            return ps;
        }

        private void OnPlayerDied()
        {
            if (_completed) return;

            // With a run in progress, the RunController reloads the level. Standalone, respawn here.
            if (RunController.Instance != null && _playerDiedChannel != null)
            {
                _playerDiedChannel.Raise();
                return;
            }

            StartCoroutine(Respawn());
        }

        private IEnumerator Respawn()
        {
            yield return new WaitForSeconds(_respawnDelaySeconds);

            _player.transform.position = _spawnPosition;
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
                _body.constraints = RigidbodyConstraints2D.None;
                _body.freezeRotation = true;
            }
            _health?.ResetToFull();
            _death?.Revive();

            // Re-trigger entrance animation
            PlayEntranceAnimation();
        }
    }
}
