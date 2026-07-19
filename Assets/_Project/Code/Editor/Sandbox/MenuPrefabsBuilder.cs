using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Runtime.UI;
using Game.Runtime.Audio;

namespace Game.Editor.Sandbox
{
    public static class MenuPrefabsBuilder
    {
        private const string ResourcesDir = "Assets/_Project/Resources";
        private const string TransitionPrefabPath = ResourcesDir + "/ScreenTransition.prefab";
        private const string PauseMenuPrefabPath = ResourcesDir + "/PauseMenu.prefab";
        private const string MainMenuScenePath = "Assets/Scenes/mainmenu.unity";

        // Nature Palette colors
        private static readonly Color DeepGreen = new(0.106f, 0.263f, 0.196f, 0.90f); // #1B4332 90%
        private static readonly Color ButtonGreen = new(0.25f, 0.568f, 0.424f, 1f);   // #40916C
        private static readonly Color WoodBrown = new(0.612f, 0.40f, 0.267f, 1f);     // #9C6644
        private static readonly Color CreamWhite = new(0.957f, 0.945f, 0.87f, 1f);     // #F4F1DE

        [MenuItem("Tools/IloveNature/Build Menu Prefabs and Setup MainMenu Scene")]
        public static void BuildMenuSystem()
        {
            // 1. Ensure Resources directory exists
            if (!Directory.Exists(ResourcesDir))
            {
                Directory.CreateDirectory(ResourcesDir);
                AssetDatabase.Refresh();
            }

            // 2. Build ScreenTransition Prefab
            BuildScreenTransitionPrefab();

            // 3. Build PauseMenu Prefab
            BuildPauseMenuPrefab();

            // 4. Setup MainMenu Scene
            SetupMainMenuScene();

            Debug.Log("[MenuPrefabsBuilder] Successfully completed building all menu systems!");
        }

        private static void BuildScreenTransitionPrefab()
        {
            var go = new GameObject("ScreenTransition", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(ScreenTransition));
            
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Ensure it stays on top of everything

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var cg = go.GetComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;

            // Fade Image child
            var imgGo = new GameObject("FadeImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imgGo.transform.SetParent(go.transform, false);
            var imgRt = imgGo.GetComponent<RectTransform>();
            imgRt.anchorMin = Vector2.zero;
            imgRt.anchorMax = Vector2.one;
            imgRt.sizeDelta = Vector2.zero;

            var img = imgGo.GetComponent<Image>();
            img.color = new Color(0.08f, 0.12f, 0.08f, 1f); // Deep nature dark green fade color

            // Link ScreenTransition
            var st = go.GetComponent<ScreenTransition>();
            var stSerialized = new SerializedObject(st);
            stSerialized.FindProperty("_canvasGroup").objectReferenceValue = cg;
            stSerialized.FindProperty("_fadeImage").objectReferenceValue = img;
            stSerialized.ApplyModifiedProperties();

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(go, TransitionPrefabPath);
            Object.DestroyImmediate(go);
            Debug.Log($"[MenuPrefabsBuilder] Created transition prefab at: {TransitionPrefabPath}");
        }

        private static void BuildPauseMenuPrefab()
        {
            var go = new GameObject("PauseMenu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(PauseMenu));
            
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 990;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // 1. Panel Background
            var panelGo = new GameObject("PausePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGo.transform.SetParent(go.transform, false);
            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.sizeDelta = Vector2.zero;

            var panelImg = panelGo.GetComponent<Image>();
            panelImg.color = DeepGreen;

            // 2. Title Text
            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.85f);
            titleRt.anchorMax = new Vector2(0.5f, 0.85f);
            titleRt.sizeDelta = new Vector2(500, 100);
            titleRt.anchoredPosition = Vector2.zero;

            var titleTxt = titleGo.GetComponent<TextMeshProUGUI>();
            titleTxt.text = "PAUSED";
            titleTxt.fontSize = 72f;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = CreamWhite;
            titleTxt.fontStyle = FontStyles.Bold;

            // 3. Buttons
            var resumeBtn = CreateButton(panelGo, "Resume", new Vector2(300, 60), new Vector2(0, 150), ButtonGreen, CreamWhite);
            var mainMenuBtn = CreateButton(panelGo, "Main Menu", new Vector2(300, 60), new Vector2(0, 70), WoodBrown, CreamWhite);
            var exitBtn = CreateButton(panelGo, "Exit", new Vector2(300, 60), new Vector2(0, -10), new Color(0.6f, 0.2f, 0.2f, 1f), CreamWhite);

            // 4. Volume Slider (positioned at Y=-110)
            var sliderContainer = new GameObject("VolumeSliderContainer", typeof(RectTransform));
            sliderContainer.transform.SetParent(panelGo.transform, false);
            var scRt = sliderContainer.GetComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0.5f, 0.5f);
            scRt.anchorMax = new Vector2(0.5f, 0.5f);
            scRt.sizeDelta = new Vector2(300, 80);
            scRt.anchoredPosition = new Vector2(0, -110);

            // Label text
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(sliderContainer.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0.5f, 1f);
            labelRt.anchorMax = new Vector2(0.5f, 1f);
            labelRt.sizeDelta = new Vector2(300, 30);
            labelRt.anchoredPosition = new Vector2(0, 0);

            var labelTxt = labelGo.GetComponent<TextMeshProUGUI>();
            labelTxt.text = "Master Volume";
            labelTxt.fontSize = 20f;
            labelTxt.alignment = TextAlignmentOptions.Center;
            labelTxt.color = CreamWhite;

            // Actual Slider
            var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(sliderContainer.transform, false);
            var sliderRt = sliderGo.GetComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0.5f, 0.2f);
            sliderRt.anchorMax = new Vector2(0.5f, 0.2f);
            sliderRt.sizeDelta = new Vector2(250, 20);
            sliderRt.anchoredPosition = Vector2.zero;

            var slider = sliderGo.GetComponent<Slider>();
            
            // Slider Background
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(sliderGo.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgGo.GetComponent<Image>().color = new Color(0.2f, 0.3f, 0.2f, 1f);

            // Slider Fill Area
            var fillAreaGo = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var faRt = fillAreaGo.GetComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0.5f, 1f); // default half
            fillRt.sizeDelta = Vector2.zero;
            var fillImg = fillGo.GetComponent<Image>();
            fillImg.color = ButtonGreen;

            slider.fillRect = fillRt;

            // 5. Create VSync Toggle (Y=-200)
            var vsyncToggle = CreateToggle(panelGo, "VSyncToggle", new Vector2(200, 30), new Vector2(-50, -200), "VSync Enabled");

            // 6. Create Frame Limit Dropdown (Y=-270)
            var frameDropdown = CreateDropdown(panelGo, "FrameLimitDropdown", new Vector2(250, 40), new Vector2(0, -270));

            // Link PauseMenu
            var pm = go.GetComponent<PauseMenu>();
            var pmSerialized = new SerializedObject(pm);
            pmSerialized.FindProperty("_pausePanel").objectReferenceValue = panelGo;
            pmSerialized.FindProperty("_resumeButton").objectReferenceValue = resumeBtn;
            pmSerialized.FindProperty("_mainMenuButton").objectReferenceValue = mainMenuBtn;
            pmSerialized.FindProperty("_exitButton").objectReferenceValue = exitBtn;
            pmSerialized.FindProperty("_volumeSlider").objectReferenceValue = slider;
            pmSerialized.FindProperty("_vSyncToggle").objectReferenceValue = vsyncToggle;
            pmSerialized.FindProperty("_frameLimitDropdown").objectReferenceValue = frameDropdown;
            pmSerialized.ApplyModifiedProperties();

            // 7. Add JuicyPanel to Pause Panel (very subtle float)
            var jp = panelGo.AddComponent<JuicyPanel>();
            var jpSerialized = new SerializedObject(jp);
            jpSerialized.FindProperty("_floatDistance").floatValue = 6f;
            jpSerialized.FindProperty("_swayAngle").floatValue = 0.4f;
            jpSerialized.ApplyModifiedProperties();

            // 8. Add JuicyButton to buttons
            var clickSfx = AssetDatabase.LoadAssetAtPath<SfxDefinition>("Assets/_Project/Data/Audio/Sfx_Pickup.asset");
            Button[] pmButtons = { resumeBtn, mainMenuBtn, exitBtn };
            foreach (var btn in pmButtons)
            {
                var jb = btn.gameObject.AddComponent<JuicyButton>();
                var jbSerialized = new SerializedObject(jb);
                if (clickSfx != null)
                {
                    jbSerialized.FindProperty("_clickSfx").objectReferenceValue = clickSfx;
                }
                jbSerialized.ApplyModifiedProperties();
            }

            // Save Prefab
            PrefabUtility.SaveAsPrefabAsset(go, PauseMenuPrefabPath);
            Object.DestroyImmediate(go);
            Debug.Log($"[MenuPrefabsBuilder] Created pause menu prefab at: {PauseMenuPrefabPath}");
        }

        private static void SetupMainMenuScene()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            // Find the Canvas
            var canvasGo = GameObject.Find("Canvas");
            if (canvasGo == null)
            {
                BuildDefaultMainMenuUI();
                return;
            }

            // Find or add MainMenu controller
            var mm = Object.FindFirstObjectByType<MainMenu>();
            if (mm == null)
            {
                var mmHost = GameObject.Find("MainMenuController");
                if (mmHost == null)
                {
                    mmHost = new GameObject("MainMenuController");
                }
                mm = mmHost.AddComponent<MainMenu>();
            }

            // Locate components in the Canvas
            var allButtons = canvasGo.GetComponentsInChildren<Button>(true);
            var playBtn = FindButtonByName(allButtons, "Play");
            var settingsBtn = FindButtonByName(allButtons, "Settings");
            var exitBtn = FindButtonByName(allButtons, "Exit");
            var backBtn = FindButtonByName(allButtons, "Back");

            var volumeSlider = canvasGo.GetComponentInChildren<Slider>(true);
            var vsyncToggle = canvasGo.GetComponentInChildren<Toggle>(true);
            var frameLimitDropdown = canvasGo.GetComponentInChildren<TMP_Dropdown>(true);

            // Identify the panels
            var mainMenuPanel = canvasGo.transform.Find("MainMenuPanel")?.gameObject;
            if (mainMenuPanel == null) mainMenuPanel = canvasGo.transform.Find("Main Menu")?.gameObject;
            if (mainMenuPanel == null) mainMenuPanel = playBtn != null ? playBtn.transform.parent.gameObject : null;

            var settingsPanel = canvasGo.transform.Find("SettingsPanel")?.gameObject;
            if (settingsPanel == null) settingsPanel = canvasGo.transform.Find("Settings")?.gameObject;

            // Wire MainMenu fields
            var mmSerialized = new SerializedObject(mm);
            if (mainMenuPanel != null) mmSerialized.FindProperty("_mainMenuPanel").objectReferenceValue = mainMenuPanel;
            if (settingsPanel != null) mmSerialized.FindProperty("_settingsPanel").objectReferenceValue = settingsPanel;
            if (playBtn != null) mmSerialized.FindProperty("_playButton").objectReferenceValue = playBtn;
            if (settingsBtn != null) mmSerialized.FindProperty("_settingsButton").objectReferenceValue = settingsBtn;
            if (exitBtn != null) mmSerialized.FindProperty("_exitButton").objectReferenceValue = exitBtn;
            if (backBtn != null) mmSerialized.FindProperty("_settingsBackButton").objectReferenceValue = backBtn;
            if (volumeSlider != null) mmSerialized.FindProperty("_volumeSlider").objectReferenceValue = volumeSlider;
            if (vsyncToggle != null) mmSerialized.FindProperty("_vSyncToggle").objectReferenceValue = vsyncToggle;
            if (frameLimitDropdown != null) mmSerialized.FindProperty("_frameLimitDropdown").objectReferenceValue = frameLimitDropdown;
            mmSerialized.ApplyModifiedProperties();

            // Load default audio asset
            var clickSfx = AssetDatabase.LoadAssetAtPath<SfxDefinition>("Assets/_Project/Data/Audio/Sfx_Pickup.asset");

            // Attach JuicyButton component to all buttons non-destructively
            foreach (var btn in allButtons)
            {
                var jb = btn.GetComponent<JuicyButton>();
                if (jb == null)
                {
                    jb = btn.gameObject.AddComponent<JuicyButton>();
                }
                var jbSerialized = new SerializedObject(jb);
                if (clickSfx != null)
                {
                    jbSerialized.FindProperty("_clickSfx").objectReferenceValue = clickSfx;
                }
                jbSerialized.ApplyModifiedProperties();
            }

            // Find the wooden panel and attach JuicyPanel for sways & floating loops!
            if (mainMenuPanel != null && mainMenuPanel != canvasGo)
            {
                var jp = mainMenuPanel.GetComponent<JuicyPanel>();
                if (jp == null)
                {
                    jp = mainMenuPanel.AddComponent<JuicyPanel>();
                }
            }

            if (settingsPanel != null && settingsPanel != canvasGo)
            {
                var jp = settingsPanel.GetComponent<JuicyPanel>();
                if (jp == null)
                {
                    jp = settingsPanel.AddComponent<JuicyPanel>();
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MenuPrefabsBuilder] Configured juicy animations and preserved user's MainMenu UI setup.");
        }

        private static void BuildDefaultMainMenuUI()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = mainCam;
                canvas.planeDistance = 5f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            var mmHost = new GameObject("MainMenuController", typeof(MainMenu));
            var mm = mmHost.GetComponent<MainMenu>();

            var mainPanel = new GameObject("MainMenuPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            mainPanel.transform.SetParent(canvasGo.transform, false);
            var mpRt = mainPanel.GetComponent<RectTransform>();
            mpRt.anchorMin = Vector2.zero;
            mpRt.anchorMax = Vector2.one;
            mpRt.sizeDelta = Vector2.zero;
            mainPanel.GetComponent<Image>().color = DeepGreen;

            var titleGo = new GameObject("TitleText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(mainPanel.transform, false);
            var titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.75f);
            titleRt.anchorMax = new Vector2(0.5f, 0.75f);
            titleRt.sizeDelta = new Vector2(800, 150);
            titleRt.anchoredPosition = Vector2.zero;

            var titleTxt = titleGo.GetComponent<TextMeshProUGUI>();
            titleTxt.text = "I LOVE NATURE";
            titleTxt.fontSize = 84f;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = CreamWhite;
            titleTxt.fontStyle = FontStyles.Bold;

            var playBtn = CreateButton(mainPanel, "Play", new Vector2(350, 70), new Vector2(0, 40), ButtonGreen, CreamWhite);
            var settingsBtn = CreateButton(mainPanel, "Settings", new Vector2(350, 70), new Vector2(0, -40), WoodBrown, CreamWhite);
            var exitBtn = CreateButton(mainPanel, "Exit", new Vector2(350, 70), new Vector2(0, -120), new Color(0.6f, 0.2f, 0.2f, 1f), CreamWhite);

            var settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            settingsPanel.transform.SetParent(canvasGo.transform, false);
            var spRt = settingsPanel.GetComponent<RectTransform>();
            spRt.anchorMin = Vector2.zero;
            spRt.anchorMax = Vector2.one;
            spRt.sizeDelta = Vector2.zero;
            settingsPanel.GetComponent<Image>().color = DeepGreen;
            settingsPanel.SetActive(false);

            var sTitleGo = new GameObject("SettingsTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            sTitleGo.transform.SetParent(settingsPanel.transform, false);
            var sTitleRt = sTitleGo.GetComponent<RectTransform>();
            sTitleRt.anchorMin = new Vector2(0.5f, 0.75f);
            sTitleRt.anchorMax = new Vector2(0.5f, 0.75f);
            sTitleRt.sizeDelta = new Vector2(500, 100);
            sTitleRt.anchoredPosition = Vector2.zero;

            var sTitleTxt = sTitleGo.GetComponent<TextMeshProUGUI>();
            sTitleTxt.text = "SETTINGS";
            sTitleTxt.fontSize = 64f;
            sTitleTxt.alignment = TextAlignmentOptions.Center;
            sTitleTxt.color = CreamWhite;
            sTitleTxt.fontStyle = FontStyles.Bold;

            var sliderContainer = new GameObject("VolumeSliderContainer", typeof(RectTransform));
            sliderContainer.transform.SetParent(settingsPanel.transform, false);
            var scRt = sliderContainer.GetComponent<RectTransform>();
            scRt.anchorMin = new Vector2(0.5f, 0.5f);
            scRt.anchorMax = new Vector2(0.5f, 0.5f);
            scRt.sizeDelta = new Vector2(400, 100);
            scRt.anchoredPosition = new Vector2(0, 20);

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(sliderContainer.transform, false);
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = new Vector2(0.5f, 1f);
            labelRt.anchorMax = new Vector2(0.5f, 1f);
            labelRt.sizeDelta = new Vector2(400, 30);
            labelRt.anchoredPosition = Vector2.zero;

            var labelTxt = labelGo.GetComponent<TextMeshProUGUI>();
            labelTxt.text = "Master Volume";
            labelTxt.fontSize = 24f;
            labelTxt.alignment = TextAlignmentOptions.Center;
            labelTxt.color = CreamWhite;

            var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(sliderContainer.transform, false);
            var sliderRt = sliderGo.GetComponent<RectTransform>();
            sliderRt.anchorMin = new Vector2(0.5f, 0.2f);
            sliderRt.anchorMax = new Vector2(0.5f, 0.2f);
            sliderRt.sizeDelta = new Vector2(300, 30);
            sliderRt.anchoredPosition = Vector2.zero;

            var slider = sliderGo.GetComponent<Slider>();

            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            bgGo.transform.SetParent(sliderGo.transform, false);
            var bgRt = bgGo.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;
            bgGo.GetComponent<Image>().color = new Color(0.2f, 0.3f, 0.2f, 1f);

            var fillAreaGo = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var faRt = fillAreaGo.GetComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0.5f, 1f);
            fillRt.sizeDelta = Vector2.zero;
            fillGo.GetComponent<Image>().color = ButtonGreen;

            slider.fillRect = fillRt;

            var vsyncToggle = CreateToggle(settingsPanel, "VSyncToggle", new Vector2(250, 30), new Vector2(-50, -70), "VSync Enabled");
            var frameDropdown = CreateDropdown(settingsPanel, "FrameLimitDropdown", new Vector2(250, 40), new Vector2(0, -140));
            var backBtn = CreateButton(settingsPanel, "Back", new Vector2(350, 70), new Vector2(0, -230), WoodBrown, CreamWhite);

            var mmSerialized = new SerializedObject(mm);
            mmSerialized.FindProperty("_mainMenuPanel").objectReferenceValue = mainPanel;
            mmSerialized.FindProperty("_settingsPanel").objectReferenceValue = settingsPanel;
            mmSerialized.FindProperty("_playButton").objectReferenceValue = playBtn;
            mmSerialized.FindProperty("_settingsButton").objectReferenceValue = settingsBtn;
            mmSerialized.FindProperty("_exitButton").objectReferenceValue = exitBtn;
            mmSerialized.FindProperty("_settingsBackButton").objectReferenceValue = backBtn;
            mmSerialized.FindProperty("_volumeSlider").objectReferenceValue = slider;
            mmSerialized.FindProperty("_vSyncToggle").objectReferenceValue = vsyncToggle;
            mmSerialized.FindProperty("_frameLimitDropdown").objectReferenceValue = frameDropdown;
            mmSerialized.ApplyModifiedProperties();

            // Add sfx
            var clickSfx = AssetDatabase.LoadAssetAtPath<SfxDefinition>("Assets/_Project/Data/Audio/Sfx_Pickup.asset");
            Button[] buttons = { playBtn, settingsBtn, exitBtn, backBtn };
            foreach (var btn in buttons)
            {
                var jb = btn.gameObject.AddComponent<JuicyButton>();
                var jbSerialized = new SerializedObject(jb);
                if (clickSfx != null)
                {
                    jbSerialized.FindProperty("_clickSfx").objectReferenceValue = clickSfx;
                }
                jbSerialized.ApplyModifiedProperties();
            }

            var jp = mainPanel.AddComponent<JuicyPanel>();
            var jpSerialized = new SerializedObject(jp);
            jpSerialized.FindProperty("_floatDistance").floatValue = 10f;
            jpSerialized.FindProperty("_swayAngle").floatValue = 1.2f;
            jpSerialized.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("[MenuPrefabsBuilder] Built default MainMenu UI hierarchy.");
        }

        private static Button FindButtonByName(Button[] buttons, string keyword)
        {
            foreach (var btn in buttons)
            {
                if (btn.name.ToLower().Contains(keyword.ToLower()))
                {
                    return btn;
                }
            }
            return null;
        }

        private static Button CreateButton(GameObject parent, string textStr, Vector2 size, Vector2 anchoredPos, Color bgColor, Color textColor)
        {
            var btnGo = new GameObject(textStr + " Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(parent.transform, false);
            
            var rt = btnGo.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var img = btnGo.GetComponent<Image>();
            img.color = bgColor;

            var txtGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtGo.transform.SetParent(btnGo.transform, false);
            var txtRt = txtGo.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            var tmp = txtGo.GetComponent<TextMeshProUGUI>();
            tmp.text = textStr;
            tmp.fontSize = 24f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.fontStyle = FontStyles.Bold;

            return btnGo.GetComponent<Button>();
        }

        private static Toggle CreateToggle(GameObject parent, string name, Vector2 size, Vector2 anchoredPos, string labelText)
        {
            var prevSelection = Selection.activeGameObject;
            Selection.activeGameObject = parent;
            EditorApplication.ExecuteMenuItem("GameObject/UI/Toggle");
            var go = Selection.activeGameObject;
            go.name = name;
            Selection.activeGameObject = prevSelection;

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var toggle = go.GetComponent<Toggle>();

            var textComp = go.transform.Find("Label")?.GetComponent<Text>();
            if (textComp != null)
            {
                textComp.text = labelText;
                textComp.color = CreamWhite;
            }
            var tmpComp = go.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (tmpComp != null)
            {
                tmpComp.text = labelText;
                tmpComp.color = CreamWhite;
            }

            return toggle;
        }

        private static TMP_Dropdown CreateDropdown(GameObject parent, string name, Vector2 size, Vector2 anchoredPos)
        {
            var prevSelection = Selection.activeGameObject;
            Selection.activeGameObject = parent;
            EditorApplication.ExecuteMenuItem("GameObject/UI/Dropdown - TextMeshPro");
            var go = Selection.activeGameObject;
            go.name = name;
            Selection.activeGameObject = prevSelection;

            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var dropdown = go.GetComponent<TMP_Dropdown>();
            dropdown.options.Clear();
            dropdown.options.Add(new TMP_Dropdown.OptionData("30 FPS"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("60 FPS"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("120 FPS"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("144 FPS"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("Unlimited"));

            return dropdown;
        }
    }
}
