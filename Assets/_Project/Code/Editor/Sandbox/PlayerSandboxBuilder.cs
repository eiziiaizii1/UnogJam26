using System.IO;
using Game.Runtime.Combat;
using Game.Runtime.Input;
using Game.Runtime.Player;
using Game.Runtime.Presentation;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Sandbox
{
    /// <summary>
    /// One-click greybox sandbox for feel testing (Guide §13.4). Generates a placeholder
    /// square sprite, a ground platform, a bullet prefab + pool, and a fully-wired player, so
    /// walk + jump + shoot can be exercised immediately. Everything is throwaway M1 scaffolding.
    /// </summary>
    public static class PlayerSandboxBuilder
    {
        private const string SquareSpritePath = "Assets/_Project/Content/Shared/Square.png";
        private const string BulletPrefabPath = "Assets/_Project/Content/Combat/Bullet.prefab";
        private static readonly Color RobotColor = new(0.25f, 0.28f, 0.32f);
        private static readonly Color NatureColor = new(0.36f, 0.62f, 0.30f);
        private static readonly Color LaserColor = new(1f, 0.85f, 0.30f);

        [MenuItem("Tools/IloveNature/Build Player Sandbox")]
        public static void BuildPlayerSandbox()
        {
            ClearPreviousSandbox();

            var sprite = GetOrCreateSquareSprite();
            var cameraGo = EnsureCamera();
            CreateScenery(sprite);

            var bulletPrefab = GetOrCreateBulletPrefab(sprite);
            var pool = CreateBulletPool(bulletPrefab);
            var player = CreatePlayer(sprite, pool);
            WireCameraFollow(cameraGo, player);

            Selection.activeGameObject = player;
            Debug.Log("[PlayerSandbox] Built scenery + player + bullet pool + camera follow. Play — A/D walk, Space jump, J / left-mouse shoot.");
        }

        private static void ClearPreviousSandbox()
        {
            foreach (var motor in Object.FindObjectsByType<PlayerMotor>(FindObjectsSortMode.None))
            {
                Undo.DestroyObjectImmediate(motor.gameObject);
            }
            foreach (var pool in Object.FindObjectsByType<BulletPool>(FindObjectsSortMode.None))
            {
                Undo.DestroyObjectImmediate(pool.gameObject);
            }
            var scenery = GameObject.Find("Sandbox");
            if (scenery != null)
            {
                Undo.DestroyObjectImmediate(scenery);
            }
        }

        private static GameObject CreatePlayer(Sprite sprite, BulletPool pool)
        {
            var go = new GameObject("Player");
            Undo.RegisterCreatedObjectUndo(go, "Create Player");
            go.transform.position = Vector3.zero;
            go.transform.localScale = new Vector3(1f, 1.5f, 1f);

            var renderer = Undo.AddComponent<SpriteRenderer>(go);
            renderer.sprite = sprite;
            renderer.color = RobotColor;
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

            return go;
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

        private static GameObject GetOrCreateBulletPrefab(Sprite sprite)
        {
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

        private static void CreateScenery(Sprite sprite)
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
    }
}
