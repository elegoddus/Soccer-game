#if UNITY_EDITOR
using System.Linq;
using SoccerTest;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SoccerTest.Editor
{
    public static class SoccerSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Location soccer field.unity";
        private const string PlayerPath = "Assets/Jammo-Character/Models/Jammo_LowPoly.fbx";
        private const string ControllerPath = "Assets/Jammo-Character/Animations/AnimatorController_Jamo.controller";
        private const string BallPath = "Assets/Soccer Ball/Prefabs/Soccer Ball.prefab";
        private const string VfxPath = "Assets/Effect/Prefabs/Confetti Explosion - Stars.prefab";

        [MenuItem("Soccer Test/Build Gameplay Scene")]
        public static void BuildGameplayScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemovePreviousGameplayObjects();

            PlayerController player = CreatePlayer();
            BallController ball = CreateBall();
            Transform goalTarget = CreateGoalTrigger();
            TopDownFollowCamera followCamera = ConfigureCamera();
            SoccerGameManager manager = CreateManager(player, ball, goalTarget, followCamera);
            CreateUi(manager, out Button kickButton, out Button autoKickButton, out Text statusText);

            SerializedObject managerSo = new SerializedObject(manager);
            managerSo.FindProperty("kickButton").objectReferenceValue = kickButton;
            managerSo.FindProperty("autoKickButton").objectReferenceValue = autoKickButton;
            managerSo.FindProperty("statusText").objectReferenceValue = statusText;
            managerSo.ApplyModifiedPropertiesWithoutUndo();

            EnsureEventSystem();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("[Soccer Test] Gameplay scene created successfully.");
        }

        public static void BuildWindowsPlayer()
        {
            BuildGameplayScene();
            const string output = "Build/Windows/SoccerTest.exe";
            System.IO.Directory.CreateDirectory("Build/Windows");
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new System.Exception("Windows build failed: " + report.summary.result);
            }
            Debug.Log($"[Soccer Test] Windows build completed: {output}");
        }

        private static void RemovePreviousGameplayObjects()
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name.StartsWith("Soccer Ball") ||
                    root.name == "Jammo Player" ||
                    root.name == "Soccer Gameplay" ||
                    root.name == "Goal Trigger" ||
                    root.name == "Gameplay UI" ||
                    root.name == "EventSystem")
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static PlayerController CreatePlayer()
        {
            GameObject playerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPath);
            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerAsset);
            player.name = "Jammo Player";
            player.transform.position = new Vector3(-5.5f, 4.66f, 0f);
            player.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            Animator animator = player.GetComponent<Animator>();
            if (animator == null)
            {
                animator = player.AddComponent<Animator>();
            }
            animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            animator.applyRootMotion = false;

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.stepOffset = 0.25f;

            PlayerController playerController = player.AddComponent<PlayerController>();
            SerializedObject so = new SerializedObject(playerController);
            so.FindProperty("animator").objectReferenceValue = animator;
            so.ApplyModifiedPropertiesWithoutUndo();
            return playerController;
        }

        private static BallController CreateBall()
        {
            GameObject ballAsset = AssetDatabase.LoadAssetAtPath<GameObject>(BallPath);
            GameObject ball = (GameObject)PrefabUtility.InstantiatePrefab(ballAsset);
            ball.name = "Soccer Ball";
            ball.transform.position = new Vector3(-1.5f, 4.95f, 0f);
            ball.transform.localScale = Vector3.one * 3f;

            Rigidbody body = ball.AddComponent<Rigidbody>();
            body.mass = 0.45f;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.maxAngularVelocity = 30f;
            return ball.AddComponent<BallController>();
        }

        private static Transform CreateGoalTrigger()
        {
            GameObject trigger = new GameObject("Goal Trigger");
            trigger.transform.position = new Vector3(12.25f, 5.45f, 0.06f);
            BoxCollider collider = trigger.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1.8f, 2.2f, 3.35f);
            trigger.AddComponent<GoalTrigger>();

            GameObject target = new GameObject("Kick Target");
            target.transform.SetParent(trigger.transform, false);
            target.transform.localPosition = Vector3.zero;

            foreach (GameObject goal in Object.FindObjectsByType<GameObject>()
                         .Where(go => go.name.StartsWith("soccer goal")))
            {
                foreach (Collider goalCollider in goal.GetComponentsInChildren<Collider>())
                {
                    goalCollider.enabled = false;
                }
            }
            return target.transform;
        }

        private static TopDownFollowCamera ConfigureCamera()
        {
            Camera camera = Camera.main;
            camera.fieldOfView = 55f;
            camera.nearClipPlane = 0.1f;
            TopDownFollowCamera follow = camera.gameObject.GetComponent<TopDownFollowCamera>();
            if (follow == null)
            {
                follow = camera.gameObject.AddComponent<TopDownFollowCamera>();
            }
            return follow;
        }

        private static SoccerGameManager CreateManager(
            PlayerController player,
            BallController ball,
            Transform goalTarget,
            TopDownFollowCamera followCamera)
        {
            GameObject root = new GameObject("Soccer Gameplay");
            SoccerGameManager manager = root.AddComponent<SoccerGameManager>();
            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("player").objectReferenceValue = player;
            so.FindProperty("targetGoal").objectReferenceValue = goalTarget;
            so.FindProperty("balls").arraySize = 1;
            so.FindProperty("balls").GetArrayElementAtIndex(0).objectReferenceValue = ball;
            so.FindProperty("followCamera").objectReferenceValue = followCamera;
            so.FindProperty("goalVfxPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(VfxPath);
            so.ApplyModifiedPropertiesWithoutUndo();
            return manager;
        }

        private static void CreateUi(
            SoccerGameManager manager,
            out Button kickButton,
            out Button autoKickButton,
            out Text statusText)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GameObject canvasObject = new GameObject("Gameplay UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            Image header = CreatePanel(canvas.transform, "Header", new Color(0.035f, 0.08f, 0.12f, 0.88f));
            RectTransform headerRect = header.rectTransform;
            headerRect.anchorMin = new Vector2(0.02f, 0.82f);
            headerRect.anchorMax = new Vector2(0.42f, 0.97f);
            headerRect.offsetMin = headerRect.offsetMax = Vector2.zero;

            Text title = CreateText(header.transform, "Title", "SOCCER KICK TEST", font, 34, FontStyle.Bold);
            SetAnchors(title.rectTransform, 0.04f, 0.56f, 0.96f, 0.94f);
            Text hint = CreateText(header.transform, "Hint", "WASD  Di chuyển   •   Đến gần bóng để Kick", font, 23, FontStyle.Normal);
            SetAnchors(hint.rectTransform, 0.04f, 0.27f, 0.96f, 0.58f);
            statusText = CreateText(header.transform, "Status", "", font, 20, FontStyle.Italic);
            statusText.color = new Color(0.4f, 0.9f, 1f);
            SetAnchors(statusText.rectTransform, 0.04f, 0.03f, 0.96f, 0.3f);

            kickButton = CreateButton(canvas.transform, "Kick Button", "KICK", font, new Color(0.95f, 0.36f, 0.08f));
            SetAnchors(kickButton.GetComponent<RectTransform>(), 0.77f, 0.08f, 0.96f, 0.19f);

            autoKickButton = CreateButton(canvas.transform, "Auto Kick Button", "AUTO KICK", font, new Color(0.05f, 0.58f, 0.82f));
            SetAnchors(autoKickButton.GetComponent<RectTransform>(), 0.77f, 0.21f, 0.96f, 0.32f);

            Button resetButton = CreateButton(canvas.transform, "Reset Button", "RESET", font, new Color(0.25f, 0.28f, 0.32f));
            SetAnchors(resetButton.GetComponent<RectTransform>(), 0.03f, 0.05f, 0.14f, 0.13f);
            UnityEventTools.AddPersistentListener(resetButton.onClick, manager.ResetScene);
        }

        private static Image CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(Transform parent, string name, string value, Font font, int size, FontStyle style)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            Text text = textObject.GetComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 14;
            text.resizeTextMaxSize = size;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label, Font font, Color color)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.25f);
            button.colors = colors;

            Text text = CreateText(buttonObject.transform, "Label", label, font, 32, FontStyle.Bold);
            text.alignment = TextAnchor.MiddleCenter;
            SetAnchors(text.rectTransform, 0f, 0f, 1f, 1f);
            return button;
        }

        private static void SetAnchors(RectTransform rect, float minX, float minY, float maxX, float maxY)
        {
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }
    }
}
#endif
