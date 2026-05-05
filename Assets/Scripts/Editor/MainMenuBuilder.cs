#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class MainMenuBuilder
{
    const string SpriteRoot = "Assets/Main menu/";
    const string OptionsRoot = "Assets/Main menu/Options main menu/";

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
        var mainMenu = CreateImage("MainMenuPanel", canvasGO.transform, LoadSprite(SpriteRoot + "Main menu_.png"));
        StretchAll(mainMenu.rectTransform);

        // Buttons - anchored Middle Right, vertical stack
        string[] btnNames = { "BtnStart", "BtnOptions", "BtnCredits", "BtnExit" };
        string[] btnSprites = { "Start.png", "Options.png", "Credits.png", "Exit.png" };
        Vector2 btnSize = new Vector2(360, 90);
        float spacing = 100f;
        float startY = 1.5f * spacing; // 4 buttons stacked around center
        float rightOffset = -120f;

        var buttons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            var b = CreateButton(btnNames[i], mainMenu.transform, LoadSprite(SpriteRoot + btnSprites[i]));
            var rt = b.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
            rt.pivot = new Vector2(1f, 0.5f);
            rt.sizeDelta = btnSize;
            rt.anchoredPosition = new Vector2(rightOffset, startY - i * spacing);
            buttons[i] = b;
        }

        // ===== OPTIONS PANEL =====
        var optionsPanel = CreateImage("OptionsPanel", canvasGO.transform, LoadSprite(OptionsRoot + "Options_bg.png"));
        var optRT = optionsPanel.rectTransform;
        optRT.anchorMin = optRT.anchorMax = new Vector2(0.5f, 0.5f);
        optRT.pivot = new Vector2(0.5f, 0.5f);
        optRT.anchoredPosition = Vector2.zero;
        optRT.sizeDelta = new Vector2(900, 600);

        // BGM row
        var bgmLabel = CreateImage("BGMLabel", optionsPanel.transform, LoadSprite(OptionsRoot + "Background music_options.png"));
        AnchorTopLeft(bgmLabel.rectTransform, new Vector2(60, -130), new Vector2(320, 60));

        var bgmSlider = CreateSlider("BGMSlider", optionsPanel.transform,
            LoadSprite(OptionsRoot + "Volume_1.png"),
            LoadSprite(OptionsRoot + "Volume_2.png"),
            1f);
        AnchorTopLeft(bgmSlider.GetComponent<RectTransform>(), new Vector2(420, -150), new Vector2(420, 30));

        // SFX row
        var sfxLabel = CreateImage("SFXLabel", optionsPanel.transform, LoadSprite(OptionsRoot + "Sound effect_options.png"));
        AnchorTopLeft(sfxLabel.rectTransform, new Vector2(60, -240), new Vector2(320, 60));

        var sfxSlider = CreateSlider("SFXSlider", optionsPanel.transform,
            LoadSprite(OptionsRoot + "Volume_1.png"),
            LoadSprite(OptionsRoot + "Volume_2.png"),
            0.5f);
        AnchorTopLeft(sfxSlider.GetComponent<RectTransform>(), new Vector2(420, -260), new Vector2(420, 30));

        // How to play (kiri bawah panel)
        var btnHowTo = CreateButton("BtnHowToPlay", optionsPanel.transform, LoadSprite(OptionsRoot + "How to play.png"));
        var htRT = btnHowTo.GetComponent<RectTransform>();
        htRT.anchorMin = htRT.anchorMax = new Vector2(0f, 0f);
        htRT.pivot = new Vector2(0f, 0f);
        htRT.sizeDelta = new Vector2(260, 80);
        htRT.anchoredPosition = new Vector2(60, 60);

        // Back (kanan bawah panel)
        var btnBack = CreateButton("BtnBack", optionsPanel.transform, LoadSprite(OptionsRoot + "Back.png"));
        var bkRT = btnBack.GetComponent<RectTransform>();
        bkRT.anchorMin = bkRT.anchorMax = new Vector2(1f, 0f);
        bkRT.pivot = new Vector2(1f, 0f);
        bkRT.sizeDelta = new Vector2(220, 80);
        bkRT.anchoredPosition = new Vector2(-60, 60);

        // ===== MENU MANAGER =====
        var managerGO = new GameObject("MenuManager");
        Undo.RegisterCreatedObjectUndo(managerGO, "Create MenuManager");
        var controller = managerGO.AddComponent<MenuController>();
        controller.mainMenuPanel = mainMenu.gameObject;
        controller.optionsPanel = optionsPanel.gameObject;
        controller.bgmSlider = bgmSlider;
        controller.sfxSlider = sfxSlider;

        // Hook onClicks (persistent, survives play mode)
        UnityEventTools.AddPersistentListener(buttons[0].onClick, controller.OnStartClick);
        UnityEventTools.AddPersistentListener(buttons[1].onClick, controller.OnOptionsClick);
        UnityEventTools.AddPersistentListener(buttons[2].onClick, controller.OnCreditsClick);
        UnityEventTools.AddPersistentListener(buttons[3].onClick, controller.OnExitClick);
        UnityEventTools.AddPersistentListener(btnHowTo.onClick, controller.OnHowToPlayClick);
        UnityEventTools.AddPersistentListener(btnBack.onClick, controller.OnBackClick);

        // Disable options panel last (after listeners hooked)
        optionsPanel.gameObject.SetActive(false);

        Selection.activeGameObject = canvasGO;
        EditorUtility.DisplayDialog("Main Menu",
            "Hierarchy berhasil dibuat!\n\nJangan lupa save scene (Ctrl+S).",
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

    static Button CreateButton(string name, Transform parent, Sprite sprite)
    {
        var img = CreateImage(name, parent, sprite);
        img.raycastTarget = true;
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        return btn;
    }

    static Slider CreateSlider(string name, Transform parent, Sprite fillSprite, Sprite handleSprite, float defaultValue)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Slider));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);

        // Fill Area
        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        var faRT = fillArea.GetComponent<RectTransform>();
        faRT.anchorMin = new Vector2(0, 0.25f);
        faRT.anchorMax = new Vector2(1, 0.75f);
        faRT.offsetMin = new Vector2(10, 0);
        faRT.offsetMax = new Vector2(-10, 0);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fillImg = fill.GetComponent<Image>();
        fillImg.sprite = fillSprite;
        fillImg.type = Image.Type.Sliced;
        var fillRT = fill.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        // Handle Slide Area
        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        var haRT = handleArea.GetComponent<RectTransform>();
        haRT.anchorMin = new Vector2(0, 0);
        haRT.anchorMax = new Vector2(1, 1);
        haRT.offsetMin = new Vector2(10, 0);
        haRT.offsetMax = new Vector2(-10, 0);

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var handleImg = handle.GetComponent<Image>();
        handleImg.sprite = handleSprite;
        handleImg.preserveAspect = true;
        var handleRT = handle.GetComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(30, 30);

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

    static void AnchorTopLeft(RectTransform rt, Vector2 anchoredPos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
    }
}
#endif
