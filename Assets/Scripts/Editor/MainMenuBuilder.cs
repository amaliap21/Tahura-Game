#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class MainMenuBuilder
{
    const string SpriteRoot   = "Assets/Main menu/";
    const string OptionsRoot  = "Assets/Main menu/Options main menu/";
    const string StartRoot    = "Assets/Main menu/Start main menu/";

    // Sprite art was authored at the canvas reference resolution: each text/button
    // sprite is a full 1920x1080 overlay with the visible glyphs baked at their
    // intended screen position. Stretch them to fill, and use alpha hit-testing
    // so only the visible pixels are clickable.
    const float AlphaHitThreshold = 0.1f;

    [MenuItem("Tools/Beneath the Silence/Build Main Menu UI")]
    public static void Build()
    {
        // EventSystem
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        // Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // ===== MAIN MENU PANEL =====
        var mainMenu = CreateStretchedImage("MainMenuPanel", canvasGO.transform,
            LoadSprite(SpriteRoot + "Main menu_.png"));

        CreateStretchedImage("Logo", mainMenu.transform,
            LoadSprite(SpriteRoot + "Logo_main menu.png"));

        var btnStart   = CreateStretchedButton("BtnStart",   mainMenu.transform, SpriteRoot + "Start.png");
        var btnOptions = CreateStretchedButton("BtnOptions", mainMenu.transform, SpriteRoot + "Options.png");
        var btnCredits = CreateStretchedButton("BtnCredits", mainMenu.transform, SpriteRoot + "Credits.png");
        var btnExit    = CreateStretchedButton("BtnExit",    mainMenu.transform, SpriteRoot + "Exit.png");

        // ===== START SUBMENU PANEL (overlay) =====
        var startPanel = CreateStretchedImage("StartPanel", canvasGO.transform,
            LoadSprite(StartRoot + "Bg_start.png"));
        startPanel.raycastTarget = true; // block clicks on main menu beneath
        // Bg_start might be fully opaque — soften so the main menu shows through.
        startPanel.color = new Color(1f, 1f, 1f, 0.7f);

        var btnNewGame   = CreateStretchedButton("BtnNewGame",   startPanel.transform, StartRoot + "New game.png");
        var btnContinue  = CreateStretchedButton("BtnContinue",  startPanel.transform, StartRoot + "Continue.png");
        var btnStartBack = CreateStretchedButton("BtnStartBack", startPanel.transform, StartRoot + "Back.png");

        // ===== OPTIONS PANEL (full-canvas — sprites have positions baked in) =====
        var optionsPanel = CreateStretchedImage("OptionsPanel", canvasGO.transform,
            LoadSprite(OptionsRoot + "Options_bg.png"));
        optionsPanel.raycastTarget = true;

        CreateStretchedImage("BGMLabel", optionsPanel.transform,
            LoadSprite(OptionsRoot + "Background music_options.png"));
        CreateStretchedImage("SFXLabel", optionsPanel.transform,
            LoadSprite(OptionsRoot + "Sound effect_options.png"));

        // Sliders — positioned to overlap the bar artwork drawn in the mockup.
        // Coordinates relative to canvas center (1920x1080 reference).
        var bgmSlider = CreateSlider(
            "BGMSlider", optionsPanel.transform,
            LoadSprite(OptionsRoot + "Volume_1.png"),
            LoadSprite(OptionsRoot + "Volume_2.png"),
            new Vector2(220, 105), new Vector2(700, 32), 1f);

        var sfxSlider = CreateSlider(
            "SFXSlider", optionsPanel.transform,
            LoadSprite(OptionsRoot + "Volume_1.png"),
            LoadSprite(OptionsRoot + "Volume_2.png"),
            new Vector2(220, 0), new Vector2(700, 32), 0.5f);

        var btnHowToPlay = CreateStretchedButton("BtnHowToPlay", optionsPanel.transform,
            OptionsRoot + "How to play.png");
        var btnBack = CreateStretchedButton("BtnBack", optionsPanel.transform,
            OptionsRoot + "Back.png");

        // ===== MENU MANAGER =====
        var managerGO = new GameObject("MenuManager");
        Undo.RegisterCreatedObjectUndo(managerGO, "Create MenuManager");
        var controller = managerGO.AddComponent<MenuController>();
        controller.mainMenuPanel = mainMenu.gameObject;
        controller.startPanel    = startPanel.gameObject;
        controller.optionsPanel  = optionsPanel.gameObject;
        controller.bgmSlider     = bgmSlider;
        controller.sfxSlider     = sfxSlider;

        // Hook onClicks (persistent, survives play mode)
        UnityEventTools.AddPersistentListener(btnStart.onClick,   controller.OnStartClick);
        UnityEventTools.AddPersistentListener(btnOptions.onClick, controller.OnOptionsClick);
        UnityEventTools.AddPersistentListener(btnCredits.onClick, controller.OnCreditsClick);
        UnityEventTools.AddPersistentListener(btnExit.onClick,    controller.OnExitClick);

        UnityEventTools.AddPersistentListener(btnNewGame.onClick,   controller.OnNewGameClick);
        UnityEventTools.AddPersistentListener(btnContinue.onClick,  controller.OnContinueClick);
        UnityEventTools.AddPersistentListener(btnStartBack.onClick, controller.OnBackClick);

        UnityEventTools.AddPersistentListener(btnHowToPlay.onClick, controller.OnHowToPlayClick);
        UnityEventTools.AddPersistentListener(btnBack.onClick,      controller.OnBackClick);

        // Disable sub-panels last (after listeners hooked)
        startPanel.gameObject.SetActive(false);
        optionsPanel.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Selection.activeGameObject = canvasGO;
        EditorUtility.DisplayDialog("Main Menu",
            "Hierarchy berhasil dibuat (sprites di-stretch full-canvas)!\n\n" +
            "- MainMenuPanel + Logo + 4 buttons\n" +
            "- StartPanel (New game / Continue / Back)\n" +
            "- OptionsPanel (BGM / SFX / How to play / Back)\n\n" +
            "Sprite buttons sudah di-Read/Write enable + Full Rect mesh,\n" +
            "klik akan akurat ke teks yang terlihat.\n\n" +
            "Jangan lupa save scene (Ctrl+S).",
            "OK");
    }

    // ===== HELPERS =====

    static Sprite LoadSprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null)
            Debug.LogWarning($"[MainMenuBuilder] Sprite tidak ketemu: {path}. " +
                             "Pastikan Texture Type = Sprite (2D and UI) di Inspector.");
        return s;
    }

    /// <summary>
    /// Sets Texture Read/Write = true and Sprite Mesh Type = Full Rect on the
    /// importer for the given asset path. Required for Image.alphaHitTestMinimumThreshold.
    /// </summary>
    static void PrepareSpriteForAlphaHit(string assetPath)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        bool dirty = false;
        if (!importer.isReadable)
        {
            importer.isReadable = true;
            dirty = true;
        }

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        if (settings.spriteMeshType != SpriteMeshType.FullRect)
        {
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            dirty = true;
        }

        if (dirty)
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    static Image CreateStretchedImage(string name, Transform parent, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = false;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return img;
    }

    static Button CreateStretchedButton(string name, Transform parent, string spritePath)
    {
        PrepareSpriteForAlphaHit(spritePath);
        var sprite = LoadSprite(spritePath);

        var img = CreateStretchedImage(name, parent, sprite);
        img.raycastTarget = true;
        if (sprite != null && sprite.texture != null && sprite.texture.isReadable)
            img.alphaHitTestMinimumThreshold = AlphaHitThreshold;

        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        return btn;
    }

    static Slider CreateSlider(string name, Transform parent,
        Sprite trackSprite, Sprite handleSprite,
        Vector2 anchoredPosition, Vector2 size, float defaultValue)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPosition;
        rt.sizeDelta = size;

        // Background (track) — uses the bar art scaled to fit the slider rect.
        var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(go.transform, false);
        var bgImg = background.GetComponent<Image>();
        bgImg.sprite = trackSprite;
        bgImg.type = Image.Type.Sliced;
        bgImg.preserveAspect = false;
        var bgRT = background.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        // Fill Area + Fill (invisible — track art already shows the bar)
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero;
        faRT.anchorMax = Vector2.one;
        faRT.offsetMin = new Vector2(8, 4);
        faRT.offsetMax = new Vector2(-8, -4);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color(1f, 1f, 1f, 0f); // invisible — visual track is the background
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // Handle Slide Area + Handle
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        var haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = Vector2.zero;
        haRT.anchorMax = Vector2.one;
        haRT.offsetMin = new Vector2(16, 0);
        haRT.offsetMax = new Vector2(-16, 0);

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var handleImg = handle.GetComponent<Image>();
        handleImg.sprite = handleSprite;
        handleImg.preserveAspect = true;
        var handleRT = handle.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(40, 40);

        var slider = go.GetComponent<Slider>();
        slider.fillRect = fillRT;
        slider.handleRect = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = defaultValue;

        return slider;
    }
}
#endif
