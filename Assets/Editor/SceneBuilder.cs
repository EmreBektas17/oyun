using UnityEngine;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using PuzzleGame.Core;
using PuzzleGame.Interaction;
using PuzzleGame.Transition;
using PuzzleGame.Puzzles;
using PuzzleGame.UI;
using PuzzleGame.Audio;

/// <summary>
/// Editor tool that builds the complete puzzle game scene hierarchy
/// with placeholder sprites, components, and wired references.
/// Run via menu: PuzzleGame > Build Complete Scene
/// </summary>
public class SceneBuilder : Editor
{
    private static string SpritePath = "Assets/Art/Placeholders";
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    [MenuItem("PuzzleGame/Build Complete Scene")]
    static void BuildScene()
    {
        if (!EditorUtility.DisplayDialog("Build Puzzle Scene",
            "This will create the complete puzzle game scene.\nExisting scene objects (except Camera/Light) will remain.\n\nContinue?",
            "Build", "Cancel"))
            return;

        spriteCache.Clear();
        EnsureDirectories();

        // 1. Setup camera
        var cam = SetupCamera();

        // 2. Create managers
        var managers = CreateManagers(cam);

        // 3. Create room
        var roomRoot = CreateRoom(managers);

        // 4. Create zoom views
        var zoomViews = CreateZoomViews(managers);

        // 5. Create UI
        CreateUI(managers);

        // 6. Create EventSystem
        CreateEventSystem();

        // 7. Wire TransitionManager references
        WireTransitionManager(managers, roomRoot, zoomViews, cam);

        // 8. Create ScriptableObject puzzle data assets
        CreatePuzzleDataAssets();

        // 9. Wire puzzle data to puzzle scripts
        WirePuzzleData(zoomViews, roomRoot);

        EditorUtility.DisplayDialog("Scene Built",
            "Complete scene hierarchy created!\n\n" +
            "• Save the scene (Ctrl+S)\n" +
            "• Set Interactable layer mask on InteractionRaycaster\n" +
            "• Add cursor textures to CursorManager\n" +
            "• Add audio clips to AudioManager",
            "OK");
    }

    // ============================================================
    // CAMERA
    // ============================================================

    static Camera SetupCamera()
    {
        var camObj = GameObject.Find("Main Camera");
        if (camObj == null)
        {
            camObj = new GameObject("Main Camera");
            camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
        }

        var cam = camObj.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5.4f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.backgroundColor = new Color(0.1f, 0.12f, 0.18f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        // Add raycaster
        var raycaster = camObj.GetComponent<InteractionRaycaster>();
        if (raycaster == null)
            raycaster = camObj.AddComponent<InteractionRaycaster>();

        // Set layer mask to Everything
        var so = new SerializedObject(raycaster);
        so.FindProperty("interactableLayer").intValue = ~0;
        so.ApplyModifiedProperties();

        return cam;
    }

    // ============================================================
    // MANAGERS
    // ============================================================

    static Dictionary<string, MonoBehaviour> CreateManagers(Camera cam)
    {
        var dict = new Dictionary<string, MonoBehaviour>();

        var managersRoot = new GameObject("--- MANAGERS ---");

        // GameManager
        var gmObj = new GameObject("GameManager");
        gmObj.transform.SetParent(managersRoot.transform);
        dict["GameManager"] = gmObj.AddComponent<GameManager>();

        // TransitionManager
        var tmObj = new GameObject("TransitionManager");
        tmObj.transform.SetParent(managersRoot.transform);
        dict["TransitionManager"] = tmObj.AddComponent<TransitionManager>();

        // CursorManager
        var cmObj = new GameObject("CursorManager");
        cmObj.transform.SetParent(managersRoot.transform);
        dict["CursorManager"] = cmObj.AddComponent<CursorManager>();

        // UIManager
        var umObj = new GameObject("UIManager");
        umObj.transform.SetParent(managersRoot.transform);
        dict["UIManager"] = umObj.AddComponent<UIManager>();

        // AudioManager
        var amObj = new GameObject("AudioManager");
        amObj.transform.SetParent(managersRoot.transform);
        var am = amObj.AddComponent<AudioManager>();
        var sfxSrc = amObj.AddComponent<AudioSource>();
        var ambSrc = amObj.AddComponent<AudioSource>();
        var amSo = new SerializedObject(am);
        amSo.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
        amSo.FindProperty("ambientSource").objectReferenceValue = ambSrc;
        amSo.ApplyModifiedProperties();
        dict["AudioManager"] = am;

        return dict;
    }

    // ============================================================
    // ROOM
    // ============================================================

    static GameObject CreateRoom(Dictionary<string, MonoBehaviour> managers)
    {
        var roomRoot = new GameObject("RoomRoot");

        // Background
        CreateSprite("Background", roomRoot.transform, Vector3.zero, 19.2f, 10.8f,
            new Color(0.17f, 0.23f, 0.29f), new Color(0.12f, 0.16f, 0.22f), -10, 192, 108);

        // Vault Door (center)
        var door = CreateInteractableSprite("VaultDoor", roomRoot.transform,
            new Vector3(0f, -0.5f, 0f), 3f, 4.5f,
            new Color(0.42f, 0.48f, 0.55f), new Color(0.3f, 0.35f, 0.4f),
            0, "vault_door");

        // Safe Hotspot (left wall)
        var safe = CreateInteractableSprite("SafeHotspot", roomRoot.transform,
            new Vector3(-5.5f, 0.5f, 0f), 1.8f, 1.8f,
            new Color(0.55f, 0.48f, 0.23f), new Color(0.4f, 0.35f, 0.15f),
            1, "safe_hotspot");
        AddZoomTrigger(safe, "safe_lock");

        // Code Panel Hotspot (right wall)
        var codePanel = CreateInteractableSprite("CodePanelHotspot", roomRoot.transform,
            new Vector3(6f, 1f, 0f), 1.5f, 2f,
            new Color(0.23f, 0.48f, 0.48f), new Color(0.15f, 0.35f, 0.35f),
            1, "code_panel_hotspot");
        AddZoomTrigger(codePanel, "code_panel");

        // Switch Panel Hotspot (left wall, lower)
        var switchPanel = CreateInteractableSprite("SwitchPanelHotspot", roomRoot.transform,
            new Vector3(-6.5f, -1.5f, 0f), 1.8f, 1.5f,
            new Color(0.55f, 0.35f, 0.23f), new Color(0.4f, 0.25f, 0.15f),
            1, "switch_panel_hotspot");
        AddZoomTrigger(switchPanel, "switch_puzzle");

        // Painting / Hidden Symbol Area (gated by switch_puzzle)
        var painting = CreateInteractableSprite("PaintingHotspot", roomRoot.transform,
            new Vector3(-2f, 3f, 0f), 2.2f, 1.6f,
            new Color(0.42f, 0.3f, 0.48f), new Color(0.3f, 0.2f, 0.38f),
            1, "painting_hotspot");
        AddZoomTrigger(painting, "symbol_puzzle");
        // Gate this behind switch puzzle
        SetInteractableGate(painting, "switch_puzzle");

        // Book Shelf
        var bookShelf = CreateInteractableSprite("BookShelf", roomRoot.transform,
            new Vector3(4.5f, -1.5f, 0f), 2.8f, 2.2f,
            new Color(0.36f, 0.23f, 0.17f), new Color(0.25f, 0.15f, 0.1f),
            1, "book_shelf");
        // Book shelf is one-time use
        SetInteractableOneTime(bookShelf);

        // Hidden digit child (hidden by default)
        var hiddenDigit = CreateSprite("HiddenDigit", bookShelf.transform,
            new Vector3(0.5f, 0.5f, 0f), 0.4f, 0.4f,
            new Color(0.9f, 0.85f, 0.5f), new Color(0.7f, 0.65f, 0.3f), 2, 40, 40);

        // Add BookCluePuzzle to the BookShelf
        var bookPuzzle = bookShelf.AddComponent<BookCluePuzzle>();
        var bookSo = new SerializedObject(bookPuzzle);
        bookSo.FindProperty("bookRenderer").objectReferenceValue = bookShelf.GetComponent<SpriteRenderer>();
        bookSo.FindProperty("hiddenDigitObject").objectReferenceValue = hiddenDigit;
        var hdRenderer = hiddenDigit.GetComponent<SpriteRenderer>();
        if (hdRenderer != null)
            bookSo.FindProperty("wallDigitRenderer").objectReferenceValue = hdRenderer;
        bookSo.ApplyModifiedProperties();
        hiddenDigit.SetActive(false);

        // Wire BookShelf click to BookCluePuzzle
        var bookClick = bookShelf.AddComponent<PuzzleElementClick>();
        bookClick.action = PuzzleElementClick.ClickAction.BookClick;
        bookClick.targetPuzzle = bookPuzzle;
        WireInteractableToMethod(bookShelf, bookClick, "Execute");

        // Security Camera (decorative)
        CreateSprite("SecurityCamera", roomRoot.transform,
            new Vector3(7.5f, 4f, 0f), 0.8f, 0.5f,
            new Color(0.3f, 0.3f, 0.3f), new Color(0.2f, 0.2f, 0.2f), 1, 80, 50);

        // Clock on wall (visual clue: 3-7-1)
        CreateSprite("ClockClue", roomRoot.transform,
            new Vector3(-4f, 3.5f, 0f), 1f, 1f,
            new Color(0.6f, 0.58f, 0.5f), new Color(0.45f, 0.43f, 0.35f), 1, 100, 100);

        // Pipe label (visual clue for switch pattern)
        CreateSprite("PipeLabel", roomRoot.transform,
            new Vector3(-7.5f, -0.3f, 0f), 0.8f, 0.4f,
            new Color(0.5f, 0.5f, 0.45f), new Color(0.35f, 0.35f, 0.3f), 1, 80, 40);

        // Floor plates
        CreateSprite("FloorPlateLeft", roomRoot.transform,
            new Vector3(-4f, -4.5f, 0f), 1.8f, 0.5f,
            new Color(0.22f, 0.26f, 0.32f), new Color(0.15f, 0.18f, 0.24f), -5, 90, 25);

        CreateSprite("FloorPlateRight", roomRoot.transform,
            new Vector3(4f, -4.5f, 0f), 1.8f, 0.5f,
            new Color(0.22f, 0.26f, 0.32f), new Color(0.15f, 0.18f, 0.24f), -5, 90, 25);

        return roomRoot;
    }

    // ============================================================
    // ZOOM VIEWS
    // ============================================================

    static List<ZoomViewController> CreateZoomViews(Dictionary<string, MonoBehaviour> managers)
    {
        var views = new List<ZoomViewController>();
        var zoomRoot = new GameObject("--- ZOOM VIEWS ---");

        views.Add(CreateZoomA_SafeLock(zoomRoot.transform));
        views.Add(CreateZoomB_CodePanel(zoomRoot.transform));
        views.Add(CreateZoomC_Switches(zoomRoot.transform));
        views.Add(CreateZoomD_Symbols(zoomRoot.transform));

        return views;
    }

    static ZoomViewController CreateZoomA_SafeLock(Transform parent)
    {
        var root = new GameObject("ZoomA_SafeLock");
        root.transform.SetParent(parent);
        var zvc = root.AddComponent<ZoomViewController>();
        SetZoomViewId(zvc, "safe_lock");

        CreateSprite("BG_SafeLock", root.transform, Vector3.zero, 19.2f, 10.8f,
            new Color(0.12f, 0.15f, 0.2f), new Color(0.08f, 0.1f, 0.15f), -10, 192, 108);

        var puzzle = root.AddComponent<DialPuzzle>();

        // Dial rings
        var colors = new Color[] {
            new Color(0.7f, 0.55f, 0.2f),
            new Color(0.6f, 0.45f, 0.18f),
            new Color(0.5f, 0.38f, 0.15f)
        };
        float[] xPos = { -2.5f, 0f, 2.5f };
        float[] sizes = { 2f, 1.7f, 1.4f };

        var rings = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            var ring = CreateInteractableSprite($"DialRing{i}", root.transform,
                new Vector3(xPos[i], 0.5f, 0f), sizes[i], sizes[i],
                colors[i], colors[i] * 0.7f, 2, $"dial_ring_{i}");

            var click = ring.AddComponent<PuzzleElementClick>();
            click.action = PuzzleElementClick.ClickAction.DialRotate;
            click.parameterValue = i;
            click.targetPuzzle = puzzle;
            WireInteractableToMethod(ring, click, "Execute");
            rings[i] = ring.transform;
        }

        // Set rings on DialPuzzle via serialized property
        var dso = new SerializedObject(puzzle);
        var ringsArr = dso.FindProperty("dialRings");
        ringsArr.arraySize = 3;
        for (int i = 0; i < 3; i++)
            ringsArr.GetArrayElementAtIndex(i).objectReferenceValue = rings[i];
        dso.ApplyModifiedProperties();

        // Confirm button
        var confirm = CreateInteractableSprite("ConfirmButton", root.transform,
            new Vector3(0f, -3f, 0f), 2f, 0.8f,
            new Color(0.2f, 0.6f, 0.3f), new Color(0.15f, 0.45f, 0.2f), 2, "dial_confirm");
        var confirmClick = confirm.AddComponent<PuzzleElementClick>();
        confirmClick.action = PuzzleElementClick.ClickAction.PuzzleCheck;
        confirmClick.targetPuzzle = puzzle;
        WireInteractableToMethod(confirm, confirmClick, "Execute");

        // Back button
        CreateBackButton(root.transform);

        root.SetActive(false);
        return zvc;
    }

    static ZoomViewController CreateZoomB_CodePanel(Transform parent)
    {
        var root = new GameObject("ZoomB_CodePanel");
        root.transform.SetParent(parent);
        var zvc = root.AddComponent<ZoomViewController>();
        SetZoomViewId(zvc, "code_panel");

        CreateSprite("BG_CodePanel", root.transform, Vector3.zero, 19.2f, 10.8f,
            new Color(0.1f, 0.18f, 0.2f), new Color(0.07f, 0.12f, 0.15f), -10, 192, 108);

        var puzzle = root.AddComponent<CodePanelPuzzle>();

        // Digit displays (TextMesh)
        var displays = new TextMesh[4];
        for (int i = 0; i < 4; i++)
        {
            var digitObj = new GameObject($"Digit{i}");
            digitObj.transform.SetParent(root.transform);
            digitObj.transform.localPosition = new Vector3(-2.25f + i * 1.5f, 3f, 0f);
            var tm = digitObj.AddComponent<TextMesh>();
            tm.text = "_";
            tm.fontSize = 80;
            tm.characterSize = 0.1f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
            var mr = digitObj.GetComponent<MeshRenderer>();
            mr.sortingOrder = 5;
            displays[i] = tm;
        }

        // Wire displays
        var cpso = new SerializedObject(puzzle);
        var dispArr = cpso.FindProperty("digitDisplays");
        dispArr.arraySize = 4;
        for (int i = 0; i < 4; i++)
            dispArr.GetArrayElementAtIndex(i).objectReferenceValue = displays[i];
        cpso.ApplyModifiedProperties();

        // Keypad buttons (0-9, Del, Submit)
        int[,] layout = { {1,2,3}, {4,5,6}, {7,8,9}, {-1,0,-2} }; // -1=Del, -2=Submit
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                int val = layout[row, col];
                float x = -2f + col * 2f;
                float y = 1f - row * 1.2f;
                string label, id;
                Color fill;

                if (val == -1) { label = "Del"; id = "key_del"; fill = new Color(0.6f, 0.3f, 0.3f); }
                else if (val == -2) { label = "OK"; id = "key_submit"; fill = new Color(0.3f, 0.6f, 0.3f); }
                else { label = val.ToString(); id = $"key_{val}"; fill = new Color(0.35f, 0.4f, 0.45f); }

                var btn = CreateInteractableSprite(label, root.transform,
                    new Vector3(x, y, 0f), 1.2f, 0.8f,
                    fill, fill * 0.7f, 3, id);

                // Add label text
                var txtObj = new GameObject("Label");
                txtObj.transform.SetParent(btn.transform);
                txtObj.transform.localPosition = new Vector3(0, 0, -0.01f);
                var txtMesh = txtObj.AddComponent<TextMesh>();
                txtMesh.text = label;
                txtMesh.fontSize = 60;
                txtMesh.characterSize = 0.08f;
                txtMesh.anchor = TextAnchor.MiddleCenter;
                txtMesh.alignment = TextAlignment.Center;
                txtMesh.color = Color.white;
                var txtMr = txtObj.GetComponent<MeshRenderer>();
                txtMr.sortingOrder = 5;

                var click = btn.AddComponent<PuzzleElementClick>();
                click.targetPuzzle = puzzle;

                if (val == -1)
                {
                    click.action = PuzzleElementClick.ClickAction.KeypadDelete;
                }
                else if (val == -2)
                {
                    click.action = PuzzleElementClick.ClickAction.PuzzleCheck;
                }
                else
                {
                    click.action = PuzzleElementClick.ClickAction.KeypadDigit;
                    click.parameterValue = val;
                }
                WireInteractableToMethod(btn, click, "Execute");
            }
        }

        CreateBackButton(root.transform);
        root.SetActive(false);
        return zvc;
    }

    static ZoomViewController CreateZoomC_Switches(Transform parent)
    {
        var root = new GameObject("ZoomC_Switches");
        root.transform.SetParent(parent);
        var zvc = root.AddComponent<ZoomViewController>();
        SetZoomViewId(zvc, "switch_puzzle");

        CreateSprite("BG_Switches", root.transform, Vector3.zero, 19.2f, 10.8f,
            new Color(0.15f, 0.13f, 0.1f), new Color(0.1f, 0.08f, 0.06f), -10, 192, 108);

        var puzzle = root.AddComponent<SwitchPuzzle>();

        // 5 switches
        var handles = new Transform[5];
        for (int i = 0; i < 5; i++)
        {
            var sw = CreateInteractableSprite($"Switch{i}", root.transform,
                new Vector3(-4f + i * 2f, 0f, 0f), 0.6f, 1.5f,
                new Color(0.6f, 0.6f, 0.6f), new Color(0.4f, 0.4f, 0.4f), 2, $"switch_{i}");

            var click = sw.AddComponent<PuzzleElementClick>();
            click.action = PuzzleElementClick.ClickAction.SwitchToggle;
            click.parameterValue = i;
            click.targetPuzzle = puzzle;
            WireInteractableToMethod(sw, click, "Execute");
            handles[i] = sw.transform;
        }

        // Wire handles
        var sso = new SerializedObject(puzzle);
        var hArr = sso.FindProperty("switchHandles");
        hArr.arraySize = 5;
        for (int i = 0; i < 5; i++)
            hArr.GetArrayElementAtIndex(i).objectReferenceValue = handles[i];
        sso.ApplyModifiedProperties();

        CreateBackButton(root.transform);
        root.SetActive(false);
        return zvc;
    }

    static ZoomViewController CreateZoomD_Symbols(Transform parent)
    {
        var root = new GameObject("ZoomD_Symbols");
        root.transform.SetParent(parent);
        var zvc = root.AddComponent<ZoomViewController>();
        SetZoomViewId(zvc, "symbol_puzzle");

        CreateSprite("BG_Symbols", root.transform, Vector3.zero, 19.2f, 10.8f,
            new Color(0.12f, 0.1f, 0.18f), new Color(0.08f, 0.06f, 0.14f), -10, 192, 108);

        var puzzle = root.AddComponent<SymbolPuzzle>();

        // 4 symbol buttons in 2x2
        Color[] symColors = {
            new Color(0.8f, 0.3f, 0.3f),
            new Color(0.3f, 0.7f, 0.4f),
            new Color(0.3f, 0.4f, 0.8f),
            new Color(0.8f, 0.7f, 0.2f)
        };
        Vector3[] symPos = {
            new Vector3(-2f, 1.5f, 0), new Vector3(2f, 1.5f, 0),
            new Vector3(-2f, -1f, 0), new Vector3(2f, -1f, 0)
        };

        var symbolRenderers = new SpriteRenderer[4];
        for (int i = 0; i < 4; i++)
        {
            var sym = CreateInteractableSprite($"Symbol{i}", root.transform,
                symPos[i], 1.5f, 1.5f, symColors[i], symColors[i] * 0.6f, 2, $"symbol_{i}");

            var click = sym.AddComponent<PuzzleElementClick>();
            click.action = PuzzleElementClick.ClickAction.SymbolPress;
            click.parameterValue = i;
            click.targetPuzzle = puzzle;
            WireInteractableToMethod(sym, click, "Execute");
            symbolRenderers[i] = sym.GetComponent<SpriteRenderer>();
        }

        // Sequence dots
        var dots = new SpriteRenderer[4];
        for (int i = 0; i < 4; i++)
        {
            var dot = CreateSprite($"Dot{i}", root.transform,
                new Vector3(-1.5f + i * 1f, -3.5f, 0), 0.3f, 0.3f,
                new Color(0.3f, 0.3f, 0.3f), new Color(0.2f, 0.2f, 0.2f), 2, 30, 30);
            dots[i] = dot.GetComponent<SpriteRenderer>();
        }

        // Wire to puzzle
        var spso = new SerializedObject(puzzle);
        var sbArr = spso.FindProperty("symbolButtons");
        sbArr.arraySize = 4;
        for (int i = 0; i < 4; i++)
            sbArr.GetArrayElementAtIndex(i).objectReferenceValue = symbolRenderers[i];
        var sdArr = spso.FindProperty("sequenceDots");
        sdArr.arraySize = 4;
        for (int i = 0; i < 4; i++)
            sdArr.GetArrayElementAtIndex(i).objectReferenceValue = dots[i];
        spso.ApplyModifiedProperties();

        CreateBackButton(root.transform);
        root.SetActive(false);
        return zvc;
    }

    // ============================================================
    // UI
    // ============================================================

    static void CreateUI(Dictionary<string, MonoBehaviour> managers)
    {
        var canvas = new GameObject("UICanvas");
        var c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;
        canvas.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvas.AddComponent<GraphicRaycaster>();

        // Fade Overlay
        var fadeObj = new GameObject("FadeOverlay");
        fadeObj.transform.SetParent(canvas.transform, false);
        var fadeImg = fadeObj.AddComponent<Image>();
        fadeImg.color = new Color(0, 0, 0, 0);
        fadeImg.raycastTarget = false;
        var fadeRect = fadeObj.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;
        var fadeCtrl = fadeObj.AddComponent<FadeController>();
        var fcSo = new SerializedObject(fadeCtrl);
        fcSo.FindProperty("fadeImage").objectReferenceValue = fadeImg;
        fcSo.ApplyModifiedProperties();

        // Solved Feedback
        var solvedObj = new GameObject("SolvedFeedback");
        solvedObj.transform.SetParent(canvas.transform, false);
        var solvedCg = solvedObj.AddComponent<CanvasGroup>();
        solvedCg.alpha = 0;
        var solvedFb = solvedObj.AddComponent<SolvedFeedback>();
        var solvedRect = solvedObj.GetComponent<RectTransform>();
        solvedRect.anchorMin = new Vector2(0.5f, 0.5f);
        solvedRect.anchorMax = new Vector2(0.5f, 0.5f);
        solvedRect.sizeDelta = new Vector2(200, 200);
        // Checkmark icon
        var checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(solvedObj.transform, false);
        var checkImg = checkObj.AddComponent<Image>();
        checkImg.color = new Color(0.3f, 0.9f, 0.4f);
        var checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.sizeDelta = new Vector2(100, 100);
        // Wire SolvedFeedback
        var sfSo = new SerializedObject(solvedFb);
        sfSo.FindProperty("canvasGroup").objectReferenceValue = solvedCg;
        sfSo.FindProperty("iconTransform").objectReferenceValue = checkObj.transform;
        sfSo.ApplyModifiedProperties();
        solvedObj.SetActive(false);

        // End Screen
        var endObj = new GameObject("EndScreen");
        endObj.transform.SetParent(canvas.transform, false);
        var endCg = endObj.AddComponent<CanvasGroup>();
        endCg.alpha = 0;
        var endCtrl = endObj.AddComponent<EndScreenController>();
        var endRect = endObj.GetComponent<RectTransform>();
        endRect.anchorMin = Vector2.zero;
        endRect.anchorMax = Vector2.one;
        endRect.offsetMin = Vector2.zero;
        endRect.offsetMax = Vector2.zero;
        // Victory text
        var victoryObj = new GameObject("VictoryText");
        victoryObj.transform.SetParent(endObj.transform, false);
        var victoryTxt = victoryObj.AddComponent<Text>();
        victoryTxt.text = "VAULT OPENED";
        victoryTxt.fontSize = 72;
        victoryTxt.alignment = TextAnchor.MiddleCenter;
        victoryTxt.color = Color.white;
        victoryTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var vtRect = victoryObj.GetComponent<RectTransform>();
        vtRect.anchorMin = Vector2.zero;
        vtRect.anchorMax = Vector2.one;
        vtRect.offsetMin = Vector2.zero;
        vtRect.offsetMax = Vector2.zero;
        // Wire EndScreenController
        var ecSo = new SerializedObject(endCtrl);
        ecSo.FindProperty("canvasGroup").objectReferenceValue = endCg;
        ecSo.FindProperty("fadeController").objectReferenceValue = fadeCtrl;
        ecSo.ApplyModifiedProperties();
        endObj.SetActive(false);

        // Wire UIManager
        var uiMgr = managers["UIManager"] as UIManager;
        var umSo = new SerializedObject(uiMgr);
        umSo.FindProperty("solvedFeedback").objectReferenceValue = solvedFb;
        umSo.FindProperty("endScreen").objectReferenceValue = endCtrl;
        umSo.ApplyModifiedProperties();
    }

    // ============================================================
    // EVENT SYSTEM
    // ============================================================

    static void CreateEventSystem()
    {
        if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    // ============================================================
    // WIRING
    // ============================================================

    static void WireTransitionManager(Dictionary<string, MonoBehaviour> managers,
        GameObject roomRoot, List<ZoomViewController> zoomViews, Camera cam)
    {
        var tm = managers["TransitionManager"] as TransitionManager;
        var tmSo = new SerializedObject(tm);

        // FadeController (find it)
        var fadeCtrl = GameObject.FindObjectOfType<FadeController>();
        tmSo.FindProperty("fadeController").objectReferenceValue = fadeCtrl;
        tmSo.FindProperty("roomRaycaster").objectReferenceValue = cam.GetComponent<InteractionRaycaster>();
        tmSo.FindProperty("roomRoot").objectReferenceValue = roomRoot;

        var zvArr = tmSo.FindProperty("zoomViews");
        zvArr.arraySize = zoomViews.Count;
        for (int i = 0; i < zoomViews.Count; i++)
            zvArr.GetArrayElementAtIndex(i).objectReferenceValue = zoomViews[i];

        tmSo.ApplyModifiedProperties();
    }

    // ============================================================
    // SCRIPTABLEOBJECT PUZZLE DATA
    // ============================================================

    static void CreatePuzzleDataAssets()
    {
        string dir = "Assets/ScriptableObjects/PuzzleData";

        CreatePuzzleData(dir, "DialSafe", "dial_safe", "Dial Safe", new int[] { 3, 7, 1 }, "digit_0", 3);
        CreatePuzzleData(dir, "SwitchPuzzle", "switch_puzzle", "Mechanical Switches", new int[] { 1, 0, 1, 1, 0 }, "digit_1", 8);
        CreatePuzzleData(dir, "SymbolPuzzle", "symbol_puzzle", "Hidden Symbols", new int[] { 2, 0, 3, 1 }, "digit_2", 5);
        CreatePuzzleData(dir, "BookClue", "book_clue", "Book Clue", new int[] { }, "digit_3", 2);
        CreatePuzzleData(dir, "CodePanel", "code_panel", "Wall Code Panel", new int[] { 3, 8, 5, 2 }, "", 0);
    }

    static PuzzleDataSO CreatePuzzleData(string dir, string fileName, string id, string displayName,
        int[] solution, string rewardKey, int rewardValue)
    {
        string path = $"{dir}/{fileName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<PuzzleDataSO>(path);
        if (existing != null) return existing;

        var so = ScriptableObject.CreateInstance<PuzzleDataSO>();
        so.puzzleId = id;
        so.displayName = displayName;
        so.solution = solution;
        so.rewardDigitKey = rewardKey;
        so.rewardDigitValue = rewardValue;

        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        return so;
    }

    static void WirePuzzleData(List<ZoomViewController> zoomViews, GameObject roomRoot)
    {
        string dir = "Assets/ScriptableObjects/PuzzleData";

        // Dial puzzle in ZoomA
        WirePuzzleDataToComponent<DialPuzzle>(zoomViews[0].gameObject, $"{dir}/DialSafe.asset");
        // Code panel in ZoomB
        WirePuzzleDataToComponent<CodePanelPuzzle>(zoomViews[1].gameObject, $"{dir}/CodePanel.asset");
        // Switch puzzle in ZoomC
        WirePuzzleDataToComponent<SwitchPuzzle>(zoomViews[2].gameObject, $"{dir}/SwitchPuzzle.asset");
        // Symbol puzzle in ZoomD
        WirePuzzleDataToComponent<SymbolPuzzle>(zoomViews[3].gameObject, $"{dir}/SymbolPuzzle.asset");
        // Book clue on BookShelf
        var bookShelf = roomRoot.transform.Find("BookShelf");
        if (bookShelf != null)
            WirePuzzleDataToComponent<BookCluePuzzle>(bookShelf.gameObject, $"{dir}/BookClue.asset");
    }

    static void WirePuzzleDataToComponent<T>(GameObject go, string assetPath) where T : PuzzleBase
    {
        var puzzle = go.GetComponent<T>();
        if (puzzle == null) return;
        var data = AssetDatabase.LoadAssetAtPath<PuzzleDataSO>(assetPath);
        if (data == null) return;
        var so = new SerializedObject(puzzle);
        so.FindProperty("data").objectReferenceValue = data;
        so.ApplyModifiedProperties();
    }

    // ============================================================
    // HELPERS
    // ============================================================

    static void EnsureDirectories()
    {
        string[] dirs = { SpritePath, "Assets/ScriptableObjects/PuzzleData" };
        foreach (var d in dirs)
        {
            if (!Directory.Exists(d))
            {
                Directory.CreateDirectory(d);
                AssetDatabase.Refresh();
            }
        }
    }

    static Sprite GetOrCreateSprite(string name, Color fill, Color border, int w, int h)
    {
        string key = $"{name}_{w}x{h}";
        if (spriteCache.ContainsKey(key)) return spriteCache[key];

        string path = $"{SpritePath}/{key}.png";
        if (!File.Exists(path))
        {
            var tex = new Texture2D(w, h);
            int bw = Mathf.Max(1, Mathf.Min(w, h) / 10);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    bool isBorder = x < bw || x >= w - bw || y < bw || y >= h - bw;
                    tex.SetPixel(x, y, isBorder ? border : fill);
                }
            }
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
        }

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        spriteCache[key] = sprite;
        return sprite;
    }

    static GameObject CreateSprite(string name, Transform parent, Vector3 pos,
        float scaleX, float scaleY, Color fill, Color border, int sortOrder, int texW = 100, int texH = 100)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(scaleX, scaleY, 1);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetOrCreateSprite(name, fill, border, texW, texH);
        sr.sortingOrder = sortOrder;

        return go;
    }

    static GameObject CreateInteractableSprite(string name, Transform parent, Vector3 pos,
        float scaleX, float scaleY, Color fill, Color border, int sortOrder, string interactableId,
        int texW = 100, int texH = 100)
    {
        var go = CreateSprite(name, parent, pos, scaleX, scaleY, fill, border, sortOrder, texW, texH);

        var collider = go.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;

        var interactable = go.AddComponent<Interactable>();
        var iSo = new SerializedObject(interactable);
        iSo.FindProperty("interactableId").stringValue = interactableId;
        iSo.ApplyModifiedProperties();

        go.AddComponent<HighlightEffect>();
        // Wire highlight effect
        var he = go.GetComponent<HighlightEffect>();
        var heSo = new SerializedObject(he);
        heSo.FindProperty("targetRenderer").objectReferenceValue = go.GetComponent<SpriteRenderer>();
        heSo.ApplyModifiedProperties();

        return go;
    }

    static void AddZoomTrigger(GameObject go, string viewId)
    {
        var trigger = go.AddComponent<ZoomTrigger>();
        trigger.zoomViewId = viewId;
        WireInteractableToMethod(go, trigger, "TriggerZoom");
    }

    static void CreateBackButton(Transform parent)
    {
        var back = CreateInteractableSprite("BackButton", parent,
            new Vector3(-8f, 4.2f, 0f), 1.5f, 0.7f,
            new Color(0.5f, 0.2f, 0.2f), new Color(0.35f, 0.12f, 0.12f), 5, "back_button");

        var txtObj = new GameObject("Label");
        txtObj.transform.SetParent(back.transform);
        txtObj.transform.localPosition = new Vector3(0, 0, -0.01f);
        var txtMesh = txtObj.AddComponent<TextMesh>();
        txtMesh.text = "< BACK";
        txtMesh.fontSize = 50;
        txtMesh.characterSize = 0.06f;
        txtMesh.anchor = TextAnchor.MiddleCenter;
        txtMesh.alignment = TextAlignment.Center;
        txtMesh.color = Color.white;
        var mr = txtObj.GetComponent<MeshRenderer>();
        mr.sortingOrder = 6;

        var bb = back.AddComponent<BackButton>();
        WireInteractableToMethod(back, bb, "OnBackClicked");
    }

    static void SetZoomViewId(ZoomViewController zvc, string id)
    {
        var so = new SerializedObject(zvc);
        so.FindProperty("viewId").stringValue = id;
        so.ApplyModifiedProperties();
    }

    static void SetInteractableGate(GameObject go, string requiredPuzzleId)
    {
        var interactable = go.GetComponent<Interactable>();
        if (interactable == null) return;
        var so = new SerializedObject(interactable);
        so.FindProperty("requiresPuzzleSolved").boolValue = true;
        so.FindProperty("requiredPuzzleId").stringValue = requiredPuzzleId;
        so.ApplyModifiedProperties();
    }

    static void SetInteractableOneTime(GameObject go)
    {
        var interactable = go.GetComponent<Interactable>();
        if (interactable == null) return;
        var so = new SerializedObject(interactable);
        so.FindProperty("oneTimeUse").boolValue = true;
        so.ApplyModifiedProperties();
    }

    static void WireInteractableToMethod(GameObject go, MonoBehaviour target, string methodName)
    {
        var interactable = go.GetComponent<Interactable>();
        if (interactable == null) return;

        var method = target.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method == null) return;

        var action = System.Delegate.CreateDelegate(typeof(UnityAction), target, method) as UnityAction;
        UnityEventTools.AddVoidPersistentListener(interactable.OnClicked, action);
    }
}
