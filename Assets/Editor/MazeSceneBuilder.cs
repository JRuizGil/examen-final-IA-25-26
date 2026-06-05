#if UNITY_EDITOR
// Herramienta de editor: construye la escena MazeLab completa con un clic.
// Menu: MazeLab > Build Scene
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Collections.Generic;

public static class MazeSceneBuilder
{
    [MenuItem("MazeLab/Build Scene")]
    public static void BuildScene()
    {
        // Limpia la escena (excepto camara y luz si existen)
        var existing = GameObject.Find("Maze");
        if (existing) Object.DestroyImmediate(existing);
        foreach (var name in new[]{"Player","Enemy","PatrolPoints","Traps","Goal"})
        {
            var go = GameObject.Find(name);
            if (go) Object.DestroyImmediate(go);
        }

        // ── Luz ──────────────────────────────────────────────────────────────
        EnsureLight();

        // ── LABERINTO ────────────────────────────────────────────────────────
        var maze = new GameObject("Maze");

        // NavMeshSurface — necesario con AI Navigation 2.x
        var surface = maze.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.useGeometry    = NavMeshCollectGeometry.PhysicsColliders;

        // Suelo
        var floor = CreateCube("Floor", maze.transform,
            new Vector3(0, -0.05f, 0), new Vector3(20, 0.1f, 20));
        GameObjectUtility.SetStaticEditorFlags(floor, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);

        // Paredes exteriores
        Wall(maze, "Wall_N",  new Vector3( 0,   2,  10), new Vector3(20, 4, 0.4f));
        Wall(maze, "Wall_S",  new Vector3( 0,   2, -10), new Vector3(20, 4, 0.4f));
        Wall(maze, "Wall_E",  new Vector3( 10,  2,   0), new Vector3(0.4f, 4, 20));
        Wall(maze, "Wall_W",  new Vector3(-10,  2,   0), new Vector3(0.4f, 4, 20));

        // Paredes interiores — forman un laberinto simple con pasillos
        Wall(maze, "Inner_1", new Vector3(-5,  2,  5),  new Vector3(10, 4, 0.4f));
        Wall(maze, "Inner_2", new Vector3( 3,  2,  5),  new Vector3( 0.4f, 4, 5));  // abertura W
        Wall(maze, "Inner_3", new Vector3( 0,  2,  0),  new Vector3( 6, 4, 0.4f));
        Wall(maze, "Inner_4", new Vector3(-3,  2,  0),  new Vector3( 0.4f, 4, 5));
        Wall(maze, "Inner_5", new Vector3( 5,  2, -3),  new Vector3( 0.4f, 4, 8));
        Wall(maze, "Inner_6", new Vector3(-5,  2, -5),  new Vector3(10, 4, 0.4f));
        Wall(maze, "Inner_7", new Vector3( 0,  2, -5),  new Vector3( 0.4f, 4, 5));
        Wall(maze, "Inner_8", new Vector3(-5,  2,  0),  new Vector3( 4, 4, 0.4f));

        // ── GOAL ─────────────────────────────────────────────────────────────
        var goalGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goalGO.name = "Goal";
        goalGO.transform.position = new Vector3(8, 0.5f, 8);
        goalGO.transform.localScale = Vector3.one * 0.8f;
        var goalCol = goalGO.GetComponent<SphereCollider>();
        goalCol.isTrigger = true;
        SetColor(goalGO, new Color(1f, 0.8f, 0f));   // dorado
        goalGO.AddComponent<GoalTrigger>();

        // ── PLAYER ───────────────────────────────────────────────────────────
        var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag  = "Player";
        player.transform.position = new Vector3(-8, 1, -8);
        Object.DestroyImmediate(player.GetComponent<Rigidbody>());

        var cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.4f;
        SetColor(player, new Color(0.2f, 0.5f, 1f));

        // Movimiento
        player.AddComponent<PlayerMovement>();
        player.AddComponent<PlayerHealth>();

        // CameraRig
        var cameraRig = new GameObject("CameraRig");
        cameraRig.transform.SetParent(player.transform);
        cameraRig.transform.localPosition = new Vector3(0, 1.6f, 0);

        // Camara principal
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            mainCam = camGO.AddComponent<Camera>();
            camGO.AddComponent<AudioListener>();
        }
        mainCam.transform.SetParent(cameraRig.transform);
        mainCam.transform.localPosition = Vector3.zero;
        mainCam.transform.localRotation = Quaternion.identity;
        mainCam.gameObject.AddComponent<MouseLook>();

        // Asigna la referencia de camara en PlayerMovement
        var pm = player.GetComponent<PlayerMovement>();
        pm.cameraTransform = mainCam.transform;


        // ── TRAMPAS ──────────────────────────────────────────────────────────
        var trapsRoot = new GameObject("Traps");
        CreateTrap(trapsRoot.transform, "Trap_1", new Vector3(-2, 0.05f,  3));
        CreateTrap(trapsRoot.transform, "Trap_2", new Vector3( 6, 0.05f, -2));
        CreateTrap(trapsRoot.transform, "Trap_3", new Vector3(-7, 0.05f,  7));

        // ── PATROL POINTS ────────────────────────────────────────────────────
        var patrolRoot = new GameObject("PatrolPoints");
        var wpPositions = new Vector3[]
        {
            new Vector3(-7,  0.5f,  7),
            new Vector3( 7,  0.5f,  7),
            new Vector3( 7,  0.5f, -7),
            new Vector3(-7,  0.5f, -7),
            new Vector3( 0,  0.5f,  0),
        };
        var waypoints = new List<Transform>();
        for (int i = 0; i < wpPositions.Length; i++)
        {
            var wp = new GameObject($"WP_{i}");
            wp.transform.SetParent(patrolRoot.transform);
            wp.transform.position = wpPositions[i];
            waypoints.Add(wp.transform);
        }

        // ── ENEMIGO ──────────────────────────────────────────────────────────
        var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        enemy.transform.position = new Vector3(0, 1, 0);
        Object.DestroyImmediate(enemy.GetComponent<Rigidbody>());
        SetColor(enemy, new Color(0.7f, 0.1f, 0.1f));

        var agent = enemy.AddComponent<NavMeshAgent>();
        agent.speed = 3.5f;
        agent.stoppingDistance = 0.5f;
        agent.radius = 0.4f;

        var bt = enemy.AddComponent<EnemyBT>();
        bt.patrolPoints    = waypoints.ToArray();
        bt.detectionRadius = 7f;

        // ── Guardar escena ───────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[MazeLab] Escena construida. Ejecuta MazeLab > Bake NavMesh y guarda la escena.");
        EditorUtility.DisplayDialog("MazeLab",
            "Escena construida correctamente.\n\n" +
            "Pasos siguientes:\n" +
            "1. MazeLab > Bake NavMesh  (o selecciona Maze > NavMeshSurface > Bake)\n" +
            "2. File > Save (Ctrl+S)\n\n" +
            "Verificacion: el Enemy NO se mueve y las Traps NO cambian de color. Es lo que tendrás que implementar.",
            "OK");
    }

    [MenuItem("MazeLab/Bake NavMesh")]
    public static void BakeNavMesh()
    {
        var surface = Object.FindAnyObjectByType<NavMeshSurface>();
        if (surface == null)
        {
            EditorUtility.DisplayDialog("MazeLab", "No se encontro ningun NavMeshSurface en la escena.\nEjecuta primero MazeLab > Build Scene.", "OK");
            return;
        }
        surface.BuildNavMesh();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[MazeLab] NavMesh horneado correctamente.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void Wall(GameObject parent, string name, Vector3 pos, Vector3 scale)
    {
        var w = CreateCube(name, parent.transform, pos, scale);
        GameObjectUtility.SetStaticEditorFlags(w, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
        SetColor(w, new Color(0.55f, 0.55f, 0.55f));
    }

    static GameObject CreateCube(string name, Transform parent, Vector3 pos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position   = pos;
        go.transform.localScale = scale;
        return go;
    }

    static void CreateTrap(Transform parent, string name, Vector3 pos)
    {
        var trap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trap.name = name;
        trap.transform.SetParent(parent);
        trap.transform.position   = pos;
        trap.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);

        var col = trap.GetComponent<BoxCollider>();
        col.isTrigger = true;

        var fsm = trap.AddComponent<TrapFSM>();
        fsm.trapRenderer = trap.GetComponent<Renderer>();

        SetColor(trap, Color.green);
    }

    static void SetColor(GameObject go, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (!r) return;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        mat.color = color;
        r.sharedMaterial = mat;
    }

    static void EnsureLight()
    {
        if (Object.FindAnyObjectByType<Light>() != null) return;
        var lightGO = new GameObject("Directional Light");
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
        var l = lightGO.AddComponent<Light>();
        l.type      = LightType.Directional;
        l.intensity = 1f;
        l.shadows   = LightShadows.Soft;
    }
}
#endif