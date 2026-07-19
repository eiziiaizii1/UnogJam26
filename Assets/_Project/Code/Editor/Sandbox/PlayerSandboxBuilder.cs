using System.IO;
using Game.Runtime.Collectibles;
using Game.Runtime.Combat;
using Game.Runtime.Enemies;
using Game.Runtime.Events;
using Game.Runtime.Input;
using Game.Runtime.Level;
using Game.Runtime.Player;
using Game.Runtime.Presentation;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Sandbox
{
    /// <summary>
    /// DEPRECATED — M1 greybox scaffolding, kept only to generate a throwaway test scene.
    /// <para>
    /// Levels, prefabs and component wiring are now authored <b>by hand in the editor</b> (Guide §9
    /// promotion rule, §11.5). This tool is deliberately non-destructive: it never deletes scene
    /// objects and never overwrites an existing prefab, so it cannot clobber authored work.
    /// Delete this file once the prefabs have been harvested.
    /// </para>
    /// </summary>
    public static class PlayerSandboxBuilder
    {
        private const string SquareSpritePath = "Assets/_Project/Content/Shared/Square.png";
        private const string BulletPrefabPath = "Assets/_Project/Content/Combat/Bullet.prefab";
        private const string EnemyDefinitionPath = "Assets/_Project/Data/Enemies/Critter.asset";
        private const string EnemyPrefabPath = "Assets/_Project/Content/Enemies/Enemy.prefab";
        private const string CollectibleChannelPath = "Assets/_Project/Data/Events/CollectiblePicked.asset";
        private static readonly Color RobotColor = new(0.25f, 0.28f, 0.32f);
        private static readonly Color NatureColor = new(0.36f, 0.62f, 0.30f);
        private static readonly Color LaserColor = new(1f, 0.85f, 0.30f);

        [MenuItem("Tools/IloveNature/[Deprecated] Generate Greybox Sandbox")]
        public static void BuildPlayerSandbox()
        {
            if (!EditorUtility.DisplayDialog(
                    "Generate Greybox Sandbox (deprecated)",
                    "This was M1 scaffolding. Levels and prefabs are now authored by hand in the editor.\n\n" +
                    "It only ADDS greybox objects to the CURRENT scene. It will not delete scene objects " +
                    "and will not overwrite existing prefabs.\n\n" +
                    "Run it only in a scratch scene.",
                    "Generate", "Cancel"))
            {
                return;
            }

            var sprite = GetOrCreateSquareSprite();
            var cameraGo = EnsureCamera();
            var sandboxRoot = CreateScenery(sprite);

            var collectibleChannel = GetOrCreateCollectibleChannel();
            SpawnCollectibles(sandboxRoot, sprite, collectibleChannel);

            var enemyDefinition = GetOrCreateEnemyDefinition();
            var enemyPrefab = BuildEnemyPrefab(sprite, enemyDefinition);
            SpawnEnemy(enemyPrefab, sandboxRoot);

            var bulletPrefab = BuildBulletPrefab(sprite);
            var pool = CreateBulletPool(bulletPrefab);
            var player = CreatePlayer(sprite, pool, collectibleChannel);
            WireCameraFollow(cameraGo, player);

            var levelExit = SpawnLevelExit(sandboxRoot, sprite);
            CreateLevelController(sandboxRoot, player, levelExit);

            Selection.activeGameObject = player;
            Debug.Log("[PlayerSandbox] Sandbox built. A/D walk, Space jump, J/mouse shoot. Left = enemy, right = crates → reach the blue exit at the far right. Die = respawn after a moment.");
        }

        private static GameObject CreatePlayer(Sprite sprite, BulletPool pool, IntEventChannel collectibleChannel)
        {
            var go = new GameObject("Player");
            Undo.RegisterCreatedObjectUndo(go, "Create Player");
            go.transform.position = Vector3.zero;
            go.transform.localScale = new Vector3(1f, 1.5f, 1f);

            var visualsGo = new GameObject("Visuals");
            visualsGo.transform.SetParent(go.transform);
            visualsGo.transform.localPosition = Vector3.zero;
            visualsGo.transform.localRotation = Quaternion.identity;
            visualsGo.transform.localScale = new Vector3(3f, 3f, 3f);
            Undo.RegisterCreatedObjectUndo(visualsGo, "Create Player Visuals");

            var renderer = Undo.AddComponent<SpriteRenderer>(visualsGo);
            renderer.sprite = sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 10;

            var body = Undo.AddComponent<Rigidbody2D>(go);
            body.gravityScale = 3f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            Undo.AddComponent<BoxCollider2D>(go); // auto-sizes to the sprite bounds

            var input = Undo.AddComponent<InputReader>(go);
            var motor = Undo.AddComponent<PlayerMotor>(go);
            var shooter = Undo.AddComponent<Shooter>(go);

            var motorSerialized = new SerializedObject(motor);
            motorSerialized.FindProperty("_input").objectReferenceValue = input;
            motorSerialized.ApplyModifiedProperties();

            var shooterSerialized = new SerializedObject(shooter);
            shooterSerialized.FindProperty("_input").objectReferenceValue = input;
            shooterSerialized.FindProperty("_bulletPool").objectReferenceValue = pool;
            shooterSerialized.ApplyModifiedProperties();

            var health = Undo.AddComponent<HealthComponent>(go);
            var healthSerialized = new SerializedObject(health);
            healthSerialized.FindProperty("_maxHealth").intValue = 5;
            healthSerialized.ApplyModifiedProperties();

            Undo.AddComponent<DamageFlash>(go);

            var death = Undo.AddComponent<PlayerDeath>(go);
            var deathSerialized = new SerializedObject(death);
            var disableList = deathSerialized.FindProperty("_disableOnDeath");
            disableList.arraySize = 3;
            disableList.GetArrayElementAtIndex(0).objectReferenceValue = motor;
            disableList.GetArrayElementAtIndex(1).objectReferenceValue = shooter;
            disableList.GetArrayElementAtIndex(2).objectReferenceValue = input;
            deathSerialized.FindProperty("_renderer").objectReferenceValue = renderer;
            deathSerialized.ApplyModifiedProperties();

            var collector = Undo.AddComponent<PlayerCollector>(go);
            var collectorSerialized = new SerializedObject(collector);
            collectorSerialized.FindProperty("_pickedChannel").objectReferenceValue = collectibleChannel;
            collectorSerialized.ApplyModifiedProperties();

            // Wire player sprite animator
            var animator = Undo.AddComponent<PlayerSpriteAnimator>(go);
            var animatorSerialized = new SerializedObject(animator);
            animatorSerialized.FindProperty("_input").objectReferenceValue = input;
            animatorSerialized.FindProperty("_renderer").objectReferenceValue = renderer;
            
            // Load sprites
            var idleRight = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sağ.png");
            var idleLeft = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sol.png");
            animatorSerialized.FindProperty("_idleRight").objectReferenceValue = idleRight;
            animatorSerialized.FindProperty("_idleLeft").objectReferenceValue = idleLeft;

            // Walk right
            var walkRightProp = animatorSerialized.FindProperty("_walkRight");
            walkRightProp.arraySize = 2;
            walkRightProp.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot yürüme sağ1.png");
            walkRightProp.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot yürüme sağ2.png");

            // Walk left
            var walkLeftProp = animatorSerialized.FindProperty("_walkLeft");
            walkLeftProp.arraySize = 2;
            walkLeftProp.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot yürüme sol1.png");
            walkLeftProp.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot yürüme sol2.png");

            // Jump right
            var jumpRightProp = animatorSerialized.FindProperty("_jumpRight");
            jumpRightProp.arraySize = 3;
            jumpRightProp.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sağ zıplama1.png");
            jumpRightProp.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sağ zıplama2.png");
            jumpRightProp.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sağ zıplama3.png");

            // Jump left
            var jumpLeftProp = animatorSerialized.FindProperty("_jumpLeft");
            jumpLeftProp.arraySize = 3;
            jumpLeftProp.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sol zıplama1.png");
            jumpLeftProp.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sol zıplama2.png");
            jumpLeftProp.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sol zıplama3.png");

            animatorSerialized.ApplyModifiedProperties();

            return go;
        }

        private static void SpawnCollectibles(GameObject sandboxRoot, Sprite sprite, IntEventChannel channel)
        {
            var color = new Color(0.40f, 0.90f, 0.60f);
            Vector3[] positions =
            {
                new(1.5f, -1.5f, 0f),
                new(-3f, -1.5f, 0f),
                new(-6f, -1.5f, 0f),
                new(-2f, 0.3f, 0f), // above the ground — needs a jump (kept clear of the player spawn at x=0)
            };

            foreach (var position in positions)
            {
                var go = new GameObject("Collectible");
                go.transform.SetParent(sandboxRoot.transform);
                go.transform.position = position;
                go.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.color = color;
                renderer.sortingOrder = 6;

                var collider = go.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
                collider.radius = 0.6f;

                var collectible = go.AddComponent<Collectible>();
                var serialized = new SerializedObject(collectible);
                serialized.FindProperty("_value").intValue = 1;
                serialized.FindProperty("_pickedChannel").objectReferenceValue = channel;
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static IntEventChannel GetOrCreateCollectibleChannel()
        {
            var existing = AssetDatabase.LoadAssetAtPath<IntEventChannel>(CollectibleChannelPath);
            if (existing != null) return existing;

            Directory.CreateDirectory(Path.GetDirectoryName(CollectibleChannelPath));

            var channel = ScriptableObject.CreateInstance<IntEventChannel>();
            AssetDatabase.CreateAsset(channel, CollectibleChannelPath);
            AssetDatabase.SaveAssets();
            return channel;
        }

        private static BulletPool CreateBulletPool(GameObject bulletPrefab)
        {
            var go = new GameObject("BulletPool");
            Undo.RegisterCreatedObjectUndo(go, "Create BulletPool");

            var pool = Undo.AddComponent<BulletPool>(go);
            var serialized = new SerializedObject(pool);
            serialized.FindProperty("_prefab").objectReferenceValue = bulletPrefab.GetComponent<Bullet>();
            serialized.ApplyModifiedProperties();

            return pool;
        }

        private static GameObject BuildBulletPrefab(Sprite sprite)
        {
            // NEVER overwrite: prefabs are hand-authored now, so an existing one always wins.
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(BulletPrefabPath);
            if (existing != null) return existing;

            Directory.CreateDirectory(Path.GetDirectoryName(BulletPrefabPath));

            var temp = new GameObject("Bullet");
            temp.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

            var renderer = temp.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = LaserColor;
            renderer.sortingOrder = 5;

            var body = temp.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = temp.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f; // local; the 0.3 scale gives ~0.15 world units

            temp.AddComponent<Bullet>();

            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, BulletPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        private static GameObject CreateScenery(Sprite sprite)
        {
            var root = new GameObject("Sandbox");
            Undo.RegisterCreatedObjectUndo(root, "Create Sandbox");

            var ground = new GameObject("Ground");
            ground.transform.SetParent(root.transform);
            ground.transform.position = new Vector3(0f, -3f, 0f);
            ground.transform.localScale = new Vector3(30f, 1f, 1f);
            var groundRenderer = ground.AddComponent<SpriteRenderer>();
            groundRenderer.sprite = sprite;
            groundRenderer.color = NatureColor;
            groundRenderer.sortingOrder = 0;
            ground.AddComponent<BoxCollider2D>();

            // Background posts so camera-follow motion is visible against the flat ground.
            for (int i = -3; i <= 3; i++)
            {
                if (i == 0) continue;

                var post = new GameObject($"Marker {i}");
                post.transform.SetParent(root.transform);
                post.transform.position = new Vector3(i * 4f, -1.5f, 1f);
                post.transform.localScale = new Vector3(0.4f, 3f, 1f);
                var postRenderer = post.AddComponent<SpriteRenderer>();
                postRenderer.sprite = sprite;
                postRenderer.color = new Color(0.30f, 0.45f, 0.28f);
                postRenderer.sortingOrder = -1;
            }

            // Destructible crates: solid obstacles at the player's shoot height. 3 hits to clear.
            var crateColor = new Color(0.55f, 0.40f, 0.25f);
            for (int i = 1; i <= 3; i++)
            {
                var crate = new GameObject($"Crate {i}");
                crate.transform.SetParent(root.transform);
                crate.transform.position = new Vector3(i * 3f, -1.75f, 0f);
                crate.transform.localScale = new Vector3(1f, 1.5f, 1f);

                var crateRenderer = crate.AddComponent<SpriteRenderer>();
                crateRenderer.sprite = sprite;
                crateRenderer.color = crateColor;
                crateRenderer.sortingOrder = 1;

                crate.AddComponent<BoxCollider2D>();
                crate.AddComponent<HealthComponent>(); // default 3 HP
                crate.AddComponent<DestroyOnDeath>();
            }

            return root;
        }

        private static void SpawnEnemy(GameObject enemyPrefab, GameObject sandboxRoot)
        {
            var enemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, sandboxRoot.transform);
            enemy.transform.position = new Vector3(-8f, -1.75f, 0f);
        }

        private static LevelExit SpawnLevelExit(GameObject sandboxRoot, Sprite sprite)
        {
            var go = new GameObject("LevelExit");
            go.transform.SetParent(sandboxRoot.transform);
            go.transform.position = new Vector3(12f, -1f, 0f); // far right, past the crates
            go.transform.localScale = new Vector3(1f, 3f, 1f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.40f, 0.80f, 1f);
            renderer.sortingOrder = 2;

            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            return go.AddComponent<LevelExit>();
        }

        private static void CreateLevelController(GameObject sandboxRoot, GameObject player, LevelExit exit)
        {
            var go = new GameObject("LevelController");
            go.transform.SetParent(sandboxRoot.transform);

            var controller = go.AddComponent<LevelController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_player").objectReferenceValue = player;
            serialized.FindProperty("_exit").objectReferenceValue = exit;

            var control = serialized.FindProperty("_playerControl");
            control.arraySize = 3;
            control.GetArrayElementAtIndex(0).objectReferenceValue = player.GetComponent<InputReader>();
            control.GetArrayElementAtIndex(1).objectReferenceValue = player.GetComponent<PlayerMotor>();
            control.GetArrayElementAtIndex(2).objectReferenceValue = player.GetComponent<Shooter>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static EnemyDefinition GetOrCreateEnemyDefinition()
        {
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(EnemyDefinitionPath);
            if (existing != null) return existing;

            Directory.CreateDirectory(Path.GetDirectoryName(EnemyDefinitionPath));

            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("_displayName").stringValue = "Critter";
            serialized.FindProperty("_maxHealth").intValue = 3;
            serialized.FindProperty("_moveSpeed").floatValue = 2f;
            serialized.FindProperty("_contactDamage").intValue = 1;
            serialized.FindProperty("_patrolHalfWidth").floatValue = 3f;
            serialized.FindProperty("_tintColor").colorValue = new Color(0.80f, 0.30f, 0.25f);
            serialized.FindProperty("_size").vector2Value = Vector2.one;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(definition, EnemyDefinitionPath);
            AssetDatabase.SaveAssets();
            return definition;
        }

        private static GameObject BuildEnemyPrefab(Sprite sprite, EnemyDefinition definition)
        {
            // NEVER overwrite: prefabs are hand-authored now, so an existing one always wins.
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            if (existing != null) return existing;

            Directory.CreateDirectory(Path.GetDirectoryName(EnemyPrefabPath));

            var temp = new GameObject("Enemy");
            temp.transform.localScale = new Vector3(definition.Size.x, definition.Size.y, 1f);

            var renderer = temp.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = definition.TintColor;
            renderer.sortingOrder = 8;

            var body = temp.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.freezeRotation = true;

            temp.AddComponent<BoxCollider2D>();
            temp.AddComponent<HealthComponent>();
            temp.AddComponent<DestroyOnDeath>();
            temp.AddComponent<DamageFlash>();
            temp.AddComponent<ContactDamage>();

            var controller = temp.AddComponent<EnemyController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_definition").objectReferenceValue = definition;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(temp, EnemyPrefabPath);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        private static GameObject EnsureCamera()
        {
            if (Camera.main != null) return Camera.main.gameObject;

            var any = Object.FindFirstObjectByType<Camera>();
            if (any != null) return any.gameObject;

            var go = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(go, "Create Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);

            var cam = Undo.AddComponent<Camera>(go);
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.15f, 0.18f, 0.22f);
            return go;
        }

        private static void WireCameraFollow(GameObject cameraGo, GameObject player)
        {
            var follow = cameraGo.GetComponent<CameraFollow>();
            if (follow == null)
            {
                follow = Undo.AddComponent<CameraFollow>(cameraGo);
            }

            var serialized = new SerializedObject(follow);
            serialized.FindProperty("_target").objectReferenceValue = player.transform;
            serialized.ApplyModifiedProperties();
        }

        private static Sprite GetOrCreateSquareSprite()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
            if (existing != null) return existing;

            Directory.CreateDirectory(Path.GetDirectoryName(SquareSpritePath));

            var texture = new Texture2D(32, 32);
            var pixels = new Color32[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 255, 255, 255);
            }
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(SquareSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(SquareSpritePath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SquareSpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(SquareSpritePath);
        }

        [MenuItem("Tools/IloveNature/Setup Player Animator on Prefab")]
        public static void SetupPlayerAnimatorOnPrefab()
        {
            var prefabPath = "Assets/_Project/Content/prefabs/Player.prefab";
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError($"[PlayerAnimatorSetup] Player prefab not found at {prefabPath}");
                return;
            }

            var rootPath = PrefabUtility.LoadPrefabContents(prefabPath);

            // 1. Setup Visuals Child Object
            var visualsTransform = rootPath.transform.Find("Visuals");
            GameObject visualsGo;
            SpriteRenderer renderer;
            if (visualsTransform == null)
            {
                visualsGo = new GameObject("Visuals");
                visualsGo.transform.SetParent(rootPath.transform);
                visualsGo.transform.localPosition = Vector3.zero;
                visualsGo.transform.localRotation = Quaternion.identity;
                visualsGo.transform.localScale = new Vector3(3f, 3f, 3f);
                
                var oldRenderer = rootPath.GetComponent<SpriteRenderer>();
                renderer = visualsGo.AddComponent<SpriteRenderer>();
                if (oldRenderer != null)
                {
                    renderer.sprite = oldRenderer.sprite;
                    renderer.color = Color.white;
                    renderer.sortingOrder = oldRenderer.sortingOrder;
                    Object.DestroyImmediate(oldRenderer, true);
                }
                else
                {
                    renderer.color = Color.white;
                }
            }
            else
            {
                visualsGo = visualsTransform.gameObject;
                renderer = visualsGo.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = visualsGo.AddComponent<SpriteRenderer>();
                }
            }

            // 2. Setup Components & Wire references
            var animator = rootPath.GetComponent<PlayerSpriteAnimator>();
            if (animator == null)
            {
                animator = rootPath.AddComponent<PlayerSpriteAnimator>();
            }

            var input = rootPath.GetComponent<InputReader>();
            var motor = rootPath.GetComponent<PlayerMotor>();
            var death = rootPath.GetComponent<PlayerDeath>();

            // motor doesn't have _spriteParent on this branch

            if (death != null)
            {
                var deathSerialized = new SerializedObject(death);
                deathSerialized.FindProperty("_renderer").objectReferenceValue = renderer;
                deathSerialized.ApplyModifiedProperties();
            }

            var animatorSerialized = new SerializedObject(animator);
            animatorSerialized.FindProperty("_input").objectReferenceValue = input;
            animatorSerialized.FindProperty("_renderer").objectReferenceValue = renderer;
            
            // Load sprites
            animatorSerialized.FindProperty("_idleRight").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sağ.png");
            animatorSerialized.FindProperty("_idleLeft").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sol.png");

            var walkRightProp = animatorSerialized.FindProperty("_walkRight");
            walkRightProp.arraySize = 2;
            walkRightProp.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot yürüme sağ1.png");
            walkRightProp.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot yürüme sağ2.png");

            var walkLeftProp = animatorSerialized.FindProperty("_walkLeft");
            walkLeftProp.arraySize = 2;
            walkLeftProp.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot yürüme sol1.png");
            walkLeftProp.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot yürüme sol2.png");

            var jumpRightProp = animatorSerialized.FindProperty("_jumpRight");
            jumpRightProp.arraySize = 3;
            jumpRightProp.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sağ zıplama1.png");
            jumpRightProp.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sağ zıplama2.png");
            jumpRightProp.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sağ zıplama3.png");

            var jumpLeftProp = animatorSerialized.FindProperty("_jumpLeft");
            jumpLeftProp.arraySize = 3;
            jumpLeftProp.GetArrayElementAtIndex(0).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sol zıplama1.png");
            jumpLeftProp.GetArrayElementAtIndex(1).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sol zıplama2.png");
            jumpLeftProp.GetArrayElementAtIndex(2).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Images/robot 1/robot sol zıplama3.png");

            animatorSerialized.ApplyModifiedProperties();
            
            PrefabUtility.SaveAsPrefabAsset(rootPath, prefabPath);
            PrefabUtility.UnloadPrefabContents(rootPath);
            
            Debug.Log($"[PlayerAnimatorSetup] Successfully migrated SpriteRenderer to 'Visuals' child and wired PlayerSpriteAnimator on prefab {prefabPath}");
        }

        [MenuItem("Tools/IloveNature/Setup Player Footstep Dust")]
        public static void SetupPlayerFootstepDust()
        {
            var prefabPath = "Assets/_Project/Content/prefabs/Player.prefab";
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (playerPrefab == null)
            {
                Debug.LogError($"[PlayerDustSetup] Player prefab not found at {prefabPath}");
                return;
            }

            var rootGo = PrefabUtility.LoadPrefabContents(prefabPath);

            // 1. Locate or create FootstepDust child
            var dustTransform = rootGo.transform.Find("FootstepDust");
            GameObject dustGo;
            if (dustTransform == null)
            {
                dustGo = new GameObject("FootstepDust");
                dustGo.transform.SetParent(rootGo.transform, false);
                dustGo.transform.localPosition = new Vector3(0f, -0.6f, 0f); // Offset to player's feet
                dustGo.transform.localRotation = Quaternion.identity;
                dustGo.transform.localScale = Vector3.one;
            }
            else
            {
                dustGo = dustTransform.gameObject;
            }

            // 2. Setup ParticleSystem
            var ps = dustGo.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = dustGo.AddComponent<ParticleSystem>();
            }

            // Configure modules
            var main = ps.main;
            main.duration = 1f;
            main.loop = true;
            main.playOnAwake = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.22f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f);
            main.gravityModifier = -0.05f; // floats up very slightly
            main.simulationSpace = ParticleSystemSimulationSpace.World; // stays on ground
            main.maxParticles = 50;

            var emission = ps.emission;
            emission.enabled = false; // PlayerMotor controls active emission
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 5f; // automatically spawns when moving!

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.15f;
            shape.rotation = new Vector3(90f, 0f, 0f); // align circle parallel to ground

            // Size over lifetime
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 0.2f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Color over lifetime (fade out)
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(new Color(0.85f, 0.82f, 0.76f), 0f), new GradientColorKey(new Color(0.85f, 0.82f, 0.76f), 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0.75f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            // Particle Renderer Settings (style as soft dots)
            var psr = dustGo.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sortingOrder = 9; // just behind player sprite
                // Use default Sprites material
                var spritesDefault = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
                if (spritesDefault != null)
                {
                    psr.sharedMaterial = spritesDefault;
                }
            }

            // 3. Link to PlayerMotor component
            var motor = rootGo.GetComponent<PlayerMotor>();
            if (motor != null)
            {
                var motorSerialized = new SerializedObject(motor);
                motorSerialized.FindProperty("_footstepDust").objectReferenceValue = ps;
                motorSerialized.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(rootGo, prefabPath);
            PrefabUtility.UnloadPrefabContents(rootGo);

            Debug.Log($"[PlayerDustSetup] Successfully created and configured footstep dust ParticleSystem on player prefab at {prefabPath}");
        }
    }
}
