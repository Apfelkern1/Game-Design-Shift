using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShiftPrototype.Editor
{
    public static class ShiftSceneBuilder
    {
        const string Root = "Assets/SHIFT/Generated";
        static Sprite square;
        static PhysicsMaterial2D frictionless;
        static readonly Color Common = new Color(0.48f, 0.53f, 0.62f);
        static readonly Color Factory = new Color(0.32f, 0.68f, 0.94f);
        static readonly Color Ruins = new Color(0.35f, 0.78f, 0.45f);
        static ShiftWorld world;

        [MenuItem("Tools/SHIFT/Testszene erstellen")]
        public static void Create()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(Root);
            PrepareAssets();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            world = new GameObject("SHIFT Game").AddComponent<ShiftWorld>();
            world.factory = new GameObject("Zeit 1 - Fabrik (blau)");
            world.ruins = new GameObject("Zeit 2 - Ruine (gruen)");
            Transform common = new GameObject("Beide Zeiten (grau)").transform;
            world.roomSpawns = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                world.roomSpawns[i] = Point("Start Raum " + (i + 1), i * 40 + 1.5f, 1f, common);
                Block("Linke Begrenzung", i * 40 - 1, 3, 1, 9, Common, common);
            }

            // Room 1: one jump and two barriers
            Block("Boden 1", 12, -0.5f, 26, 1, Common, common);
            Block("Sprungblock", 6, 0.65f, 1.6f, 1.3f, Common, common);
            Block("Fabriktor", 11, 4, 1, 8, Factory, world.factory.transform);
            Block("Rankenwand", 17, 4, 1, 8, Ruins, world.ruins.transform);
            Zone("Ausgang 1", 23, 1.2f, 1, 2.4f, ShiftZone.ZoneType.Exit, 0, common);

            // Room 2: small gap and barrier
            Block("Startboden 2", 43.5f, -0.5f, 9, 1, Common, common); // endet bei 48
            Block("Zielboden 2", 59, -0.5f, 16, 1, Common, common); // beginnt bei 51
            Zone("Abgrund 2", 49.5f, -2.2f, 3, 0.7f, ShiftZone.ZoneType.Hazard, 1, common);
            var cp2 = Zone("Checkpoint 2", 56, 0.75f, 0.7f, 1.5f, ShiftZone.ZoneType.Checkpoint, 1, common);
            cp2.respawnPoint = Point("Respawn 2", 55, 1, common);
            Block("Maschinenwand", 59, 4, 1, 8, Factory, world.factory.transform);
            Zone("Ausgang 2", 65, 1.2f, 1, 2.4f, ShiftZone.ZoneType.Exit, 1, common);

            // Raum 3: two jumps
            Block("Startboden 3", 84.5f, -0.5f, 11, 1, Common, common);
            Block("Zwischenstufe", 89, 0.8f, 2, 1.6f, Ruins, world.ruins.transform);
            Block("Hoher Absatz", 94, 1.6f, 8, 3.2f, Common, common);
            var cp3 = Zone("Checkpoint 3", 87, 0.75f, 0.7f, 1.5f, ShiftZone.ZoneType.Checkpoint, 2, common);
            cp3.respawnPoint = Point("Respawn 3", 86, 1, common);
            Block("Letztes Fabriktor", 95, 6.2f, 0.8f, 6, Factory, world.factory.transform);
            Block("Landeboden 3", 102, -0.5f, 8, 1, Common, common);
            Zone("Ausgang 3", 104, 1.2f, 1, 2.4f, ShiftZone.ZoneType.Exit, 2, common);

            GameObject playerObject = Visual("Player", 1.5f, 1, 0.7f, 1.1f, new Color(1f, 0.93f, 0.75f), null);
            playerObject.GetComponent<SpriteRenderer>().sortingOrder = 10;
            playerObject.AddComponent<BoxCollider2D>().sharedMaterial = frictionless;
            playerObject.AddComponent<Rigidbody2D>();
            world.player = playerObject.AddComponent<ShiftPlayer>();
            world.player.world = world;
            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.orthographicSize = 6.8f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = new Vector3(3.5f, 3.2f, -10);
            camera.gameObject.AddComponent<AudioListener>();
            world.cameraRig = camera.gameObject.AddComponent<ShiftCamera>();
            world.cameraRig.target = playerObject.transform;
            world.cameraRig.SetEra(false);
            world.gameObject.AddComponent<ShiftHUD>().world = world;
            world.ruins.SetActive(false);
            string path = AssetDatabase.GenerateUniqueAssetPath(Root + "/SHIFT_Testszene.unity");
            EditorSceneManager.SaveScene(scene, path);
            Selection.activeGameObject = playerObject;
            Debug.Log("SHIFT: Testszene erstellt. Play druecken. Szene: " + path);
        }

        static void PrepareAssets()
        {
            string imagePath = Root + "/Square.png";
            if (!File.Exists(imagePath))
            {
                var texture = new Texture2D(16, 16);
                var pixels = new Color[256];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
                texture.SetPixels(pixels); texture.Apply();
                File.WriteAllBytes(imagePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }
            AssetDatabase.ImportAsset(imagePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(imagePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
            square = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
            string materialPath = Root + "/NoFriction.physicsMaterial2D";
            frictionless = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
            if (frictionless == null)
            {
                frictionless = new PhysicsMaterial2D("NoFriction") { friction = 0, bounciness = 0 };
                AssetDatabase.CreateAsset(frictionless, materialPath);
            }
            AssetDatabase.SaveAssets();
        }
        static Transform Point(string name, float x, float y, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(x, y, 0);
            return go.transform;
        }
        static GameObject Visual(string name, float x, float y, float w, float h, Color color, Transform parent)
        {
            GameObject go = Point(name, x, y, parent).gameObject;
            go.transform.localScale = new Vector3(w, h, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            // Unlit
            renderer.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
            return go;
        }
        static void Block(string name, float x, float y, float w, float h, Color color, Transform parent)
        {
            Visual(name, x, y, w, h, color, parent).AddComponent<BoxCollider2D>().sharedMaterial = frictionless;
        }
        static ShiftZone Zone(string name, float x, float y, float w, float h, ShiftZone.ZoneType type, int room, Transform parent)
        {
            Color color = type == ShiftZone.ZoneType.Hazard ? new Color(0.95f, 0.25f, 0.35f) :
                type == ShiftZone.ZoneType.Checkpoint ? new Color(1f, 0.8f, 0.25f) : new Color(0.7f, 0.95f, 1f);
            GameObject go = Visual(name, x, y, w, h, color, parent);
            go.GetComponent<SpriteRenderer>().sortingOrder = 1;
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            var zone = go.AddComponent<ShiftZone>();
            zone.type = type; zone.world = world; zone.room = room;
            return zone;
        }
    }
}
