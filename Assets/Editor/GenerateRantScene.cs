using System.IO;
using RantsMailDelivery;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Gera a cena principal do protótipo a partir dos sprites incluídos.</summary>
public static class GenerateRantScene
{
    private const string ScenePath = "Assets/Scenes/RantsMailDelivery.unity";
    private const string Art = "Assets/Art/";

    [InitializeOnLoadMethod]
    private static void Bootstrap()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(ScenePath)) CreateScene();
        };
    }

    [MenuItem("Rant / Criar ou recriar protótipo 2D")]
    public static void CreateScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        Directory.CreateDirectory("Assets/Scenes");
        PrepareSprites();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = new GameObject("01 - Jogo (GDD Rant)");
        GameDirector director = root.AddComponent<GameDirector>();

        GameObject camObject = new GameObject("Main Camera");
        camObject.tag = "MainCamera";
        Camera camera = camObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.1f;
        camera.backgroundColor = new Color(.05f, .14f, .13f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camObject.AddComponent<AudioListener>();
        RunnerCamera camCtrl = camObject.AddComponent<RunnerCamera>();
        camObject.transform.position = new Vector3(7.5f, .2f, -10f);

        GameObject environment = new GameObject("02 - Floresta (referência enviada)");
        Sprite bgSprite = Load("forest_from_reference.jpg");
        if (bgSprite != null)
        {
            float tileWidth = 25.45f;
            for (int i = 0; i < 10; i++)
            {
                GameObject tile = MakeSprite("Fundo - repetição " + i, bgSprite,
                    new Vector3(-tileWidth * .5f + i * tileWidth, .10f, 8f), environment.transform, -30);
                float s = tileWidth / bgSprite.bounds.size.x;
                tile.transform.localScale = new Vector3(s, s, 1f);
            }
        }

        GameObject grounds = new GameObject("03 - Chão e plataformas");
        AddGround(grounds.transform, 0f, 66f);
        AddGround(grounds.transform, 69f, 94.5f);
        AddGround(grounds.transform, 98f, 117.5f);
        AddGround(grounds.transform, 121f, 140f);
        AddGround(grounds.transform, 143f, 159.5f);
        AddGround(grounds.transform, 164f, 188.5f);
        AddGround(grounds.transform, 193f, 215f);
        AddRaisedPlatform(grounds.transform, 15f, 19.2f, .12f);
        AddRaisedPlatform(grounds.transform, 36f, 40f, .7f);
        AddRaisedPlatform(grounds.transform, 104.5f, 109f, .12f);

        GameObject protagonist = new GameObject("04 - Rant (formiga carteiro)");
        protagonist.transform.position = new Vector3(2f, -.80f, 0f);
        protagonist.AddComponent<Rigidbody2D>();
        var col = protagonist.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(.68f, 1.18f);
        col.direction = CapsuleDirection2D.Vertical;
        var player = protagonist.AddComponent<RunnerPlayer>();
        var ant = MakeSprite("Sprite - Rant", Load("Rant.png"), Vector3.zero, protagonist.transform, 10);
        ant.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
        player.bodySprite = ant.GetComponent<SpriteRenderer>();
        var cap = MakeSprite("Chapéu cosmético", Load("CapOverlay.png"), Vector3.zero, protagonist.transform, 11);
        cap.transform.localScale = ant.transform.localScale;
        player.capSprite = cap.GetComponent<SpriteRenderer>();
        var leaf = MakeSprite("Folha para planar - Parte 2 e 3", Load("Leaf.png"), new Vector3(0f, .78f, 0f), protagonist.transform, 9);
        leaf.transform.localScale = new Vector3(1.04f, .70f, 1f);
        player.leafSprite = leaf.GetComponent<SpriteRenderer>();
        player.leafSprite.enabled = false;
        camCtrl.target = protagonist.transform;
        director.player = player;
        director.cameraController = camCtrl;

        GameObject collectibles = new GameObject("05 - Açúcar branco e azul");
        MakeSugarRun(collectibles.transform, 0, new float[] { 5, 8, 13, 16, 21, 24, 29, 33, 39, 42, 47, 51, 55, 60 });
        MakeSugarRun(collectibles.transform, 1, new float[] { 73, 77, 81, 85, 89, 93, 100, 103, 107, 112, 115, 124, 129, 133, 136 });
        MakeSugarRun(collectibles.transform, 2, new float[] { 146, 150, 154, 158, 166, 168, 173, 178, 183, 185, 195, 199, 202, 205, 210 });
        AddPickup(collectibles.transform, 40f, .97f, true);
        AddPickup(collectibles.transform, 126f, .82f, true);
        AddPickup(collectibles.transform, 200f, 1.2f, true);

        GameObject hazards = new GameObject("06 - Perigos segundo o GDD");
        AddHazard(hazards.transform, "Formiga trabalhadora", "WorkerAnt.png", 11f, -.9f, .82f, .78f);
        AddHazard(hazards.transform, "Cristal espinhoso", "Wall.png", 26.5f, -.82f, .86f, 1.2f);
        AddHazard(hazards.transform, "Formiga trabalhadora", "WorkerAnt.png", 35f, -.9f, .82f, .78f);
        AddHazard(hazards.transform, "Arbusto quebrável", "Bush.png", 45f, -.95f, 1.22f, .92f, true);
        AddHazard(hazards.transform, "Parede", "Wall.png", 57f, -.73f, .95f, 1.44f, false, true);
        AddHazard(hazards.transform, "Língua de Tera", "Tongue.png", 85f, -.6f, .67f, 1.5f, false, false, true);
        AddHazard(hazards.transform, "Arbusto quebrável", "Bush.png", 105f, -.88f, 1.1f, .96f, true);
        AddHazard(hazards.transform, "Língua de Tera", "Tongue.png", 111f, -.6f, .67f, 1.5f, false, false, true);
        AddHazard(hazards.transform, "Tronco", "Log.png", 128f, -.87f, 1.08f, .85f, true);
        AddHazard(hazards.transform, "Arbusto quebrável", "Bush.png", 153f, -.85f, 1.1f, .93f, true);
        AddHazard(hazards.transform, "Tronco", "Log.png", 171f, -.83f, 1.18f, 1f, true);
        AddHazard(hazards.transform, "Pedra", "Rock.png", 183f, -.82f, .96f, 1.14f);
        AddHazard(hazards.transform, "Tronco final", "Log.png", 207f, -.79f, 1.2f, 1f, true);

        GameObject decoration = new GameObject("07 - Placas e cenário");
        foreach (var x in new[] { 23f, 90f, 169f })
        {
            GameObject checkpoint = MakeSprite("Marco de checkpoint", Load("Letter.png"), new Vector3(x, -1.04f, 0f), decoration.transform, 3);
            checkpoint.transform.localScale = new Vector3(.47f, .47f, 1f);
        }
        GameObject tera = MakeSprite("Tera - aparição Parte 3", Load("Tera.png"), new Vector3(197f, -.37f, 1f), decoration.transform, 5);
        tera.transform.localScale = new Vector3(2.35f, 2.35f, 1f);
        var chaser = tera.AddComponent<TeraChaser>();
        chaser.target = protagonist.transform;
        foreach (float x in new[] { 8, 32, 75, 99, 147, 174, 200 })
        {
            GameObject letter = MakeSprite("Carta na paisagem", Load("Letter.png"), new Vector3(x, 1.2f, 1f), decoration.transform, -1);
            letter.transform.localScale = new Vector3(.24f, .24f, 1f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        Debug.Log("Rant's Mail Delivery: Cena pronta! Abra Assets/Scenes/RantsMailDelivery e clique em Play.");
    }

    private static void PrepareSprites()
    {
        foreach (string path in Directory.GetFiles(Art))
        {
            if (!path.EndsWith(".png") && !path.EndsWith(".jpg")) continue;
            var importer = AssetImporter.GetAtPath(path.Replace('\\', '/')) as TextureImporter;
            if (importer == null) { AssetDatabase.ImportAsset(path); importer = AssetImporter.GetAtPath(path) as TextureImporter; }
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.mipmapEnabled = false;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }
    }

    private static Sprite Load(string file)
    {
        string path = Art + file;
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) Debug.LogError("Sprite não encontrado: " + path);
        return sprite;
    }

    private static GameObject MakeSprite(string name, Sprite sprite, Vector3 position, Transform parent, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = position;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    private static void AddGround(Transform parent, float from, float to)
    {
        GameObject ground = new GameObject("Chão sólido " + from + " a " + to);
        ground.transform.SetParent(parent);
        ground.transform.position = new Vector3((from + to) * .5f, -2.3f, 0f);
        ground.AddComponent<GroundSegment>();
        BoxCollider2D collider = ground.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(to - from, 1.6f);
        PaintGroundTiles(ground.transform, from, to, -2.25f);
    }

    private static void AddRaisedPlatform(Transform parent, float from, float to, float top)
    {
        GameObject platform = new GameObject("Plataforma " + from + " a " + to);
        platform.transform.SetParent(parent);
        platform.transform.position = new Vector3((from + to) * .5f, top - .2f, 0f);
        platform.AddComponent<GroundSegment>();
        BoxCollider2D collider = platform.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(to - from, .45f);
        Sprite groundSprite = Load("Ground.png");
        var sprite = MakeSprite("Visual", groundSprite, Vector3.zero, platform.transform, 1);
        sprite.transform.localScale = new Vector3((to - from) / 1.6f, .5f / .7f, 1f);
    }

    private static void PaintGroundTiles(Transform ground, float from, float to, float y)
    {
        Sprite sprite = Load("Ground.png");
        for (float p = from; p < to - .02f; p += 2.0f)
        {
            float length = Mathf.Min(2f, to - p);
            // Position is local to the ground collider centered at -2.3.
            var tile = MakeSprite("Grama", sprite, new Vector3(p + length * .5f - (from + to) * .5f, y + 2.3f, 0f), ground, 2);
            tile.transform.localScale = new Vector3(length / 1.6f, 2.1f / .7f, 1f);
        }
    }

    private static void MakeSugarRun(Transform parent, int part, float[] xPositions)
    {
        for (int i = 0; i < xPositions.Length; i++)
        {
            // A maioria está no chão, algumas exigem saltar.
            float y = i % 6 == 4 ? .53f : -.77f;
            AddPickup(parent, xPositions[i], y, false);
        }
    }

    private static void AddPickup(Transform parent, float x, float y, bool blue)
    {
        GameObject item = MakeSprite(blue ? "Açúcar AZUL (cosmético)" : "Açúcar branco (progresso)",
            Load(blue ? "SugarBlue.png" : "SugarWhite.png"), new Vector3(x, y, 0f), parent, 6);
        item.transform.localScale = Vector3.one * (blue ? .44f : .34f);
        var collider = item.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1f;
        var pickup = item.AddComponent<PickupItem>();
        pickup.isBlue = blue;
    }

    private static void AddHazard(Transform parent, string name, string sprite, float x, float y, float width, float height,
        bool breakable = false, bool fatal = false, bool tongue = false)
    {
        GameObject go = MakeSprite(name, Load(sprite), new Vector3(x, y, 0f), parent, 7);
        var sp = go.GetComponent<SpriteRenderer>().sprite;
        go.transform.localScale = new Vector3(width / sp.bounds.size.x, height / sp.bounds.size.y, 1f);
        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(sp.bounds.size.x * .83f, sp.bounds.size.y * .83f);
        collider.isTrigger = true;
        HazardObject hazard = go.AddComponent<HazardObject>();
        hazard.breakable = breakable;
        hazard.instantlyFatal = fatal;
        hazard.risingTongue = tongue;
    }
}
