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

    [MenuItem("Tools/Beneath the Silence/Build Main Menu UI")]
    public static void Build()
    {
        // Wipe any prior build so re-running doesn't leave duplicate Canvas/MenuManager objects.
        CleanPreviousBuild();

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
        var mainMenu = CreateImage("MainMenuPanel", canvasGO.transform,
            LoadSprite(SpriteRoot + "Main menu_.png"));
        mainMenu.preserveAspect = false;
        StretchAll(mainMenu.rectTransform);

        // Logo "Beneath the Silence" — top right (cropped tight, sized big)
        var logoSprite = LoadAndCrop(SpriteRoot + "Logo_main menu.png");
        var logo = CreateImage("Logo", mainMenu.transform, logoSprite);
        logo.raycastTarget = false;
        var logoRT = logo.rectTransform;
        logoRT.anchorMin = logoRT.anchorMax = new Vector2(1f, 1f);
        logoRT.pivot = new Vector2(1f, 1f);
        logoRT.sizeDelta = FitSize(logoSprite, 600);
        logoRT.anchoredPosition = new Vector2(-80, -60);

        // Buttons — middle right, vertical stack, big and readable
        Vector2 btnSize = new Vector2(380, 90);
        float spacingY = 110f;
        float xRight = -180f;

        var btnStart = CreateButton("BtnStart", mainMenu.transform,
            SpriteRoot + "Start.png",
            new Vector2(1f, 0.5f), new Vector2(xRight,  1.5f * spacingY), btnSize);
        var btnOptions = CreateButton("BtnOptions", mainMenu.transform,
            SpriteRoot + "Options.png",
            new Vector2(1f, 0.5f), new Vector2(xRight,  0.5f * spacingY), btnSize);
        var btnCredits = CreateButton("BtnCredits", mainMenu.transform,
            SpriteRoot + "Credits.png",
            new Vector2(1f, 0.5f), new Vector2(xRight, -0.5f * spacingY), btnSize);
        var btnExit = CreateButton("BtnExit", mainMenu.transform,
            SpriteRoot + "Exit.png",
            new Vector2(1f, 0.5f), new Vector2(xRight, -1.5f * spacingY), btnSize);

        // ===== START SUBMENU PANEL (overlay) =====
        var startPanel = CreateImage("StartPanel", canvasGO.transform,
            LoadSprite(StartRoot + "Bg_start.png"));
        startPanel.preserveAspect = false;
        startPanel.color = new Color(1f, 1f, 1f, 0.7f); // soft dim so main menu shows through
        startPanel.raycastTarget = true;
        StretchAll(startPanel.rectTransform);

        Vector2 startBtnSize = new Vector2(360, 90);
        var btnNewGame = CreateButton("BtnNewGame", startPanel.transform,
            StartRoot + "New game.png",
            new Vector2(0.5f, 0.5f), new Vector2(-260,  60), startBtnSize);
        var btnContinue = CreateButton("BtnContinue", startPanel.transform,
            StartRoot + "Continue.png",
            new Vector2(0.5f, 0.5f), new Vector2(-260, -50), startBtnSize);
        var btnStartBack = CreateButton("BtnStartBack", startPanel.transform,
            StartRoot + "Back.png",
            new Vector2(0.5f, 0.5f), new Vector2(-260, -200), startBtnSize);

        // ===== OPTIONS PANEL =====
        var optionsPanel = CreateImage("OptionsPanel", canvasGO.transform,
            LoadSprite(OptionsRoot + "Options_bg.png"));
        var optRT = optionsPanel.rectTransform;
        optRT.anchorMin = optRT.anchorMax = new Vector2(0.5f, 0.5f);
        optRT.pivot = new Vector2(0.5f, 0.5f);
        optRT.anchoredPosition = Vector2.zero;
        optRT.sizeDelta = new Vector2(1100, 650);

        // BGM row (label kiri, slider kanan)
        var bgmLabelSprite = LoadAndCrop(OptionsRoot + "Background music_options.png");
        AddImageAt("BGMLabel", optionsPanel.transform, bgmLabelSprite,
            new Vector2(0f, 1f), new Vector2(60, -200), FitSize(bgmLabelSprite, 320));

        var bgmSlider = CreateSlider("BGMSlider", optionsPanel.transform,
            LoadAndCrop(OptionsRoot + "Volume_1.png"),
            LoadAndCrop(OptionsRoot + "Volume_2.png"),
            new Vector2(0f, 1f), new Vector2(440, -215), new Vector2(600, 32), 1f);

        // SFX row
        var sfxLabelSprite = LoadAndCrop(OptionsRoot + "Sound effect_options.png");
        AddImageAt("SFXLabel", optionsPanel.transform, sfxLabelSprite,
            new Vector2(0f, 1f), new Vector2(60, -310), FitSize(sfxLabelSprite, 320));

        var sfxSlider = CreateSlider("SFXSlider", optionsPanel.transform,
            LoadAndCrop(OptionsRoot + "Volume_1.png"),
            LoadAndCrop(OptionsRoot + "Volume_2.png"),
            new Vector2(0f, 1f), new Vector2(440, -325), new Vector2(600, 32), 0.5f);

        // How to play (bottom left), Back (bottom right)
        var btnHowToPlay = CreateButton("BtnHowToPlay", optionsPanel.transform,
            OptionsRoot + "How to play.png",
            new Vector2(0f, 0f), new Vector2(60, 60), new Vector2(280, 80));
        var btnBack = CreateButton("BtnBack", optionsPanel.transform,
            OptionsRoot + "Back.png",
            new Vector2(1f, 0f), new Vector2(-60, 60), new Vector2(220, 80));

        // ===== MENU MANAGER =====
        var managerGO = new GameObject("MenuManager");
        Undo.RegisterCreatedObjectUndo(managerGO, "Create MenuManager");
        var controller = managerGO.AddComponent<MenuController>();
        controller.mainMenuPanel = mainMenu.gameObject;
        controller.startPanel    = startPanel.gameObject;
        controller.optionsPanel  = optionsPanel.gameObject;
        controller.bgmSlider     = bgmSlider;
        controller.sfxSlider     = sfxSlider;

        UnityEventTools.AddPersistentListener(btnStart.onClick,   controller.OnStartClick);
        UnityEventTools.AddPersistentListener(btnOptions.onClick, controller.OnOptionsClick);
        UnityEventTools.AddPersistentListener(btnCredits.onClick, controller.OnCreditsClick);
        UnityEventTools.AddPersistentListener(btnExit.onClick,    controller.OnExitClick);

        UnityEventTools.AddPersistentListener(btnNewGame.onClick,   controller.OnNewGameClick);
        UnityEventTools.AddPersistentListener(btnContinue.onClick,  controller.OnContinueClick);
        UnityEventTools.AddPersistentListener(btnStartBack.onClick, controller.OnBackClick);

        UnityEventTools.AddPersistentListener(btnHowToPlay.onClick, controller.OnHowToPlayClick);
        UnityEventTools.AddPersistentListener(btnBack.onClick,      controller.OnBackClick);

        startPanel.gameObject.SetActive(false);
        optionsPanel.gameObject.SetActive(false);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;

        EditorUtility.DisplayDialog("Main Menu",
            "Hierarchy berhasil dibuat (clean rebuild).\n\n" +
            "- 1 Canvas + 1 MenuManager\n" +
            "- Sprite teks di-crop tight, button 380x90 (terlihat besar & clickable)\n\n" +
            "Save scene (Ctrl+S).",
            "OK");
    }

    // ===== HELPERS =====

    static void CleanPreviousBuild()
    {
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
            if (c != null) Object.DestroyImmediate(c.gameObject);

        var managers = Object.FindObjectsByType<MenuController>(FindObjectsSortMode.None);
        foreach (var m in managers)
            if (m != null) Object.DestroyImmediate(m.gameObject);
    }

    static Sprite LoadSprite(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null)
            Debug.LogWarning($"[MainMenuBuilder] Sprite tidak ketemu: {path}");
        return s;
    }

    /// <summary>
    /// Loads a sprite, ensures Read/Write is enabled on the texture importer,
    /// and returns a runtime sprite cropped tight to its non-transparent pixels.
    /// Caches results so multiple uses of the same path don't re-crop.
    /// </summary>
    static readonly System.Collections.Generic.Dictionary<string, Sprite> _cropCache
        = new System.Collections.Generic.Dictionary<string, Sprite>();

    static Sprite LoadAndCrop(string path)
    {
        if (_cropCache.TryGetValue(path, out var cached) && cached != null)
            return cached;

        EnableTextureReadWrite(path);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) return null;

        var cropped = CropToContent(sprite) ?? sprite;
        _cropCache[path] = cropped;
        return cropped;
    }

    static void EnableTextureReadWrite(string assetPath)
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

    static Sprite CropToContent(Sprite original)
    {
        if (original == null) return null;
        var tex = original.texture;
        if (tex == null || !tex.isReadable) return original;

        int w = tex.width, h = tex.height;
        var pixels = tex.GetPixels32();

        int minX = w, maxX = -1, minY = h, maxY = -1;
        const byte alphaCutoff = 8;

        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
            {
                if (pixels[row + x].a > alphaCutoff)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }
        if (maxX < minX) return original; // fully transparent — return original

        const int pad = 6;
        minX = Mathf.Max(0, minX - pad);
        minY = Mathf.Max(0, minY - pad);
        maxX = Mathf.Min(w - 1, maxX + pad);
        maxY = Mathf.Min(h - 1, maxY + pad);

        var rect = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    static Vector2 FitSize(Sprite s, float targetWidth)
    {
        if (s == null) return new Vector2(targetWidth, targetWidth * 0.3f);
        float aspect = s.rect.width / Mathf.Max(1f, s.rect.height);
        return new Vector2(targetWidth, targetWidth / aspect);
    }

    static Image CreateImage(string name, Transform parent, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        return img;
    }

    static Image AddImageAt(string name, Transform parent, Sprite sprite,
        Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        var img = CreateImage(name, parent, sprite);
        img.raycastTarget = false;
        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPosition;
        return img;
    }

    static Button CreateButton(string name, Transform parent, string spritePath,
        Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        var sprite = LoadAndCrop(spritePath);
        var img = CreateImage(name, parent, sprite);
        img.raycastTarget = true;

        var rt = img.rectTransform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPosition;

        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        return btn;
    }

    static Slider CreateSlider(string name, Transform parent,
        Sprite trackSprite, Sprite handleSprite,
        Vector2 anchor, Vector2 anchoredPosition, Vector2 size, float defaultValue)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPosition;

        // Background (track) — uses the cropped bar art scaled to fit the slider rect.
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

        // Fill — invisible (the bar art already shows the visual track)
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
        fillImg.color = new Color(1f, 1f, 1f, 0f);
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // Handle (X icon)
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

    static void StretchAll(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif
