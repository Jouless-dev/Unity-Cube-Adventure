#pragma warning disable 0649
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CubeAdventure.Player;
using CubeAdventure.CameraControl;
using CubeAdventure.Collectibles;
using CubeAdventure.Obstacles;
using CubeAdventure.Gameplay;
using CubeAdventure.Managers;
using CubeAdventure.UI;
using CubeAdventure.Audio;

namespace CubeAdventure.Editor
{
    public static class LevelBuilder
    {
        [MenuItem("Cube Adventure/Build Complete Level Scene", false, 1)]
        [MenuItem("Tools/Cube Adventure/Build Complete Level Scene", false, 1)]
        [MenuItem("GameObject/Cube Adventure/Build Complete Level Scene", false, 10)]
        public static void BuildCompleteLevel()
        {
            // Ensure Tags exist
            EnsureTag("Player");
            EnsureTag("Coin");
            EnsureTag("Obstacle");
            EnsureTag("Goal");
            EnsureTag("Ground");

            // Load Materials
            Material matPlayer = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_MarioRed.mat");
            Material matCactusGreen = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_CactusGreen.mat");
            Material matCactusSpine = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_CactusSpine.mat");
            Material matGrassTop = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_GrassTop.mat");
            Material matDirtBrown = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_DirtBrown.mat");
            Material matGoldCoin = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_GoldCoin.mat");
            Material matCloudWhite = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_CloudWhite.mat");
            Material matFlagPole = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_FlagPole.mat");
            Material matFlagFabric = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_FlagFabric.mat");
            Material matBrick = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Materials/Mat_BrickBlock.mat");

            // Load Audio Clips
            AudioClip sfxCoin = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_coin.wav");
            AudioClip sfxJump = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_jump.wav");
            AudioClip sfxHit = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_hit.wav");
            AudioClip sfxVictory = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_victory.wav");
            AudioClip bgmMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/bgm_ambient.wav");

            // 1. Setup Camera with Mario Blue Sky
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.36f, 0.58f, 0.98f, 1f); // Mario Sky Blue #5c94fc
            mainCam.transform.position = new Vector3(3.5f, 2.5f, -10f);
            mainCam.transform.rotation = Quaternion.Euler(5f, 0f, 0f);

            // 2. Level Geometry Root
            GameObject levelRoot = GameObject.Find("--- LEVEL GEOMETRY ---");
            if (levelRoot != null) Object.DestroyImmediate(levelRoot);
            levelRoot = new GameObject("--- LEVEL GEOMETRY ---");

            // Ground: Grass Top Layer
            GameObject grassTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            grassTop.name = "Ground_GrassTop";
            grassTop.tag = "Ground";
            grassTop.transform.SetParent(levelRoot.transform);
            grassTop.transform.position = new Vector3(25f, -0.15f, 0f);
            grassTop.transform.localScale = new Vector3(65f, 0.3f, 4f);
            if (matGrassTop != null) grassTop.GetComponent<MeshRenderer>().sharedMaterial = matGrassTop;

            // Ground: Earth Dirt Foundation
            GameObject dirtBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dirtBottom.name = "Ground_DirtBottom";
            dirtBottom.tag = "Ground";
            dirtBottom.transform.SetParent(levelRoot.transform);
            dirtBottom.transform.position = new Vector3(25f, -0.9f, 0f);
            dirtBottom.transform.localScale = new Vector3(65f, 1.2f, 4f);
            if (matDirtBrown != null) dirtBottom.GetComponent<MeshRenderer>().sharedMaterial = matDirtBrown;

            // Start Indicator Marker
            GameObject startMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            startMarker.name = "Start_Marker";
            startMarker.transform.SetParent(levelRoot.transform);
            startMarker.transform.position = new Vector3(0f, 0.02f, 0f);
            startMarker.transform.localScale = new Vector3(2f, 0.05f, 2.5f);
            if (matBrick != null) startMarker.GetComponent<MeshRenderer>().sharedMaterial = matBrick;

            // Decorative Clouds in Background
            CreateMarioClouds(levelRoot.transform, matCloudWhite);

            // 3. Player: Pure Vibrant Solid Red Cube (sin adornos)
            GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
            if (existingPlayer != null) Object.DestroyImmediate(existingPlayer);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
            player.name = "Red_Cube_Player";
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.6f, 0f);
            player.transform.localScale = Vector3.one;
            if (matPlayer != null) player.GetComponent<MeshRenderer>().sharedMaterial = matPlayer;

            Rigidbody rb = player.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                             RigidbodyConstraints.FreezeRotationY |
                             RigidbodyConstraints.FreezeRotationZ |
                             RigidbodyConstraints.FreezePositionZ;

            PlayerController playerCtrl = player.AddComponent<PlayerController>();
            AudioSource playerAudio = player.AddComponent<AudioSource>();
            playerAudio.playOnAwake = false;
            playerAudio.clip = sfxJump;

            // Camera Follow Hook (Locked Y for completely stable, vibration-free platforming)
            CameraFollow camFollow = mainCam.GetComponent<CameraFollow>();
            if (camFollow == null) camFollow = mainCam.gameObject.AddComponent<CameraFollow>();
            camFollow.SetTarget(player.transform);

            SerializedObject soCam = new SerializedObject(camFollow);
            soCam.FindProperty("lockY").boolValue = true;
            soCam.FindProperty("fixedCameraY").floatValue = 2.5f;
            soCam.FindProperty("smoothSpeedX").floatValue = 8.5f;
            soCam.ApplyModifiedProperties();

            // 4. Mario Coins (Rotating Gold Cylinders - Alturas alcanzables con el salto calibrado)
            GameObject coinsRoot = GameObject.Find("--- COINS ---");
            if (coinsRoot != null) Object.DestroyImmediate(coinsRoot);
            coinsRoot = new GameObject("--- COINS ---");

            Vector3[] coinPositions = new Vector3[]
            {
                new Vector3(6f, 1.4f, 0f),
                new Vector3(14.5f, 2.2f, 0f),
                new Vector3(23.5f, 1.4f, 0f),
                new Vector3(33.5f, 2.2f, 0f),
                new Vector3(43f, 1.4f, 0f)
            };

            for (int i = 0; i < coinPositions.Length; i++)
            {
                GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                coin.name = $"Mario_Coin_{i + 1}";
                coin.tag = "Coin";
                coin.transform.SetParent(coinsRoot.transform);
                coin.transform.position = coinPositions[i];
                coin.transform.localScale = new Vector3(0.75f, 0.08f, 0.75f);
                coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                if (matGoldCoin != null) coin.GetComponent<MeshRenderer>().sharedMaterial = matGoldCoin;

                Collider col = coin.GetComponent<Collider>();
                col.isTrigger = true;

                Coin coinScript = coin.AddComponent<Coin>();
                SerializedObject soCoin = new SerializedObject(coinScript);
                soCoin.FindProperty("pickupSound").objectReferenceValue = sfxCoin;
                soCoin.ApplyModifiedProperties();
            }

            // 5. Obstacles: 100% Cactus con Alturas y Espaciamiento Calibrados
            GameObject obsRoot = GameObject.Find("--- OBSTACLES ---");
            if (obsRoot != null) Object.DestroyImmediate(obsRoot);
            obsRoot = new GameObject("--- OBSTACLES ---");

            // Obstáculo 1: Cactus individual pequeño (Altura 1.0m)
            CreateCactus(obsRoot.transform, new Vector3(10.5f, 0f, 0f), 1.0f, matCactusGreen, matCactusSpine, sfxHit);

            // Obstáculo 2: Cactus individual mediano (Altura 1.35m)
            CreateCactus(obsRoot.transform, new Vector3(19.0f, 0f, 0f), 1.35f, matCactusGreen, matCactusSpine, sfxHit);

            // Obstáculo 3: Dúo de Cactus juntos (Alturas 1.15m y 1.25m - se supera en un único salto)
            CreateCactus(obsRoot.transform, new Vector3(28.0f, 0f, 0f), 1.15f, matCactusGreen, matCactusSpine, sfxHit);
            CreateCactus(obsRoot.transform, new Vector3(29.1f, 0f, 0f), 1.25f, matCactusGreen, matCactusSpine, sfxHit);

            // Obstáculo 4: Cactus alto final (Altura 1.55m)
            CreateCactus(obsRoot.transform, new Vector3(38.0f, 0f, 0f), 1.55f, matCactusGreen, matCactusSpine, sfxHit);

            // 6. Mario Goal Flagpole (Mástil limpio directo sobre el césped sin bloque base)
            GameObject existingGoal = GameObject.FindGameObjectWithTag("Goal");
            if (existingGoal != null) Object.DestroyImmediate(existingGoal);

            CreateMarioFlagpole(new Vector3(48f, 0f, 0f), matFlagPole, matGoldCoin, matFlagFabric, sfxVictory);

            // 7. Managers
            GameObject managersRoot = GameObject.Find("--- MANAGERS ---");
            if (managersRoot != null) Object.DestroyImmediate(managersRoot);
            managersRoot = new GameObject("--- MANAGERS ---");

            GameManager gm = managersRoot.AddComponent<GameManager>();
            AudioManager am = managersRoot.AddComponent<AudioManager>();

            SerializedObject soAm = new SerializedObject(am);
            soAm.FindProperty("backgroundMusic").objectReferenceValue = bgmMusic;
            soAm.ApplyModifiedProperties();

            // 8. Mario Retro UI
            CreateMarioUI(managersRoot, gm);

            // Save the scene permanently to disk
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("<color=green><b>[Cube Adventure]</b> Complete level built and saved with red cube, all-cactus obstacles, and clean flagpole!</color>");
        }

        private static void CreateCactus(Transform parent, Vector3 basePos, float height, Material cactusMat, Material thornMat, AudioClip hitSound)
        {
            GameObject cactus = new GameObject($"Cactus_H{height}");
            cactus.transform.SetParent(parent);
            cactus.transform.position = basePos;
            cactus.tag = "Obstacle";

            // Main Trunk (Cylinder)
            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Cactus_Trunk";
            trunk.tag = "Obstacle";
            trunk.transform.SetParent(cactus.transform);
            trunk.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            trunk.transform.localScale = new Vector3(0.65f, height * 0.5f, 0.65f);
            if (cactusMat != null) trunk.GetComponent<MeshRenderer>().sharedMaterial = cactusMat;
            Object.DestroyImmediate(trunk.GetComponent<Collider>());

            // Left Arm
            GameObject armL1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armL1.name = "ArmL_Horiz";
            armL1.transform.SetParent(cactus.transform);
            armL1.transform.localPosition = new Vector3(-0.4f, height * 0.45f, 0f);
            armL1.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            armL1.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            if (cactusMat != null) armL1.GetComponent<MeshRenderer>().sharedMaterial = cactusMat;
            Object.DestroyImmediate(armL1.GetComponent<Collider>());

            GameObject armL2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armL2.name = "ArmL_Vert";
            armL2.transform.SetParent(cactus.transform);
            armL2.transform.localPosition = new Vector3(-0.62f, height * 0.62f, 0f);
            armL2.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            armL2.GetComponent<MeshRenderer>().sharedMaterial = thornMat != null ? thornMat : cactusMat;
            Object.DestroyImmediate(armL2.GetComponent<Collider>());

            // Right Arm
            GameObject armR1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armR1.name = "ArmR_Horiz";
            armR1.transform.SetParent(cactus.transform);
            armR1.transform.localPosition = new Vector3(0.4f, height * 0.58f, 0f);
            armR1.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            armR1.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            if (cactusMat != null) armR1.GetComponent<MeshRenderer>().sharedMaterial = cactusMat;
            Object.DestroyImmediate(armR1.GetComponent<Collider>());

            GameObject armR2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            armR2.name = "ArmR_Vert";
            armR2.transform.SetParent(cactus.transform);
            armR2.transform.localPosition = new Vector3(0.62f, height * 0.76f, 0f);
            armR2.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            armR2.GetComponent<MeshRenderer>().sharedMaterial = thornMat != null ? thornMat : cactusMat;
            Object.DestroyImmediate(armR2.GetComponent<Collider>());

            // Unified Box Collider
            BoxCollider box = cactus.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, height * 0.5f, 0f);
            box.size = new Vector3(1.5f, height, 1.4f);

            Obstacle obsScript = cactus.AddComponent<Obstacle>();
            SerializedObject so = new SerializedObject(obsScript);
            so.FindProperty("hitSound").objectReferenceValue = hitSound;
            so.ApplyModifiedProperties();
        }

        private static void CreateMarioFlagpole(Vector3 basePos, Material poleMat, Material ballMat, Material flagMat, AudioClip sfxVictory)
        {
            GameObject goalRoot = new GameObject("Goal_MarioFlagpole");
            goalRoot.tag = "Goal";
            goalRoot.transform.position = basePos;

            // Silver Pole (Cylinder resting directly on the ground)
            GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pole.name = "Flag_Pole";
            pole.transform.SetParent(goalRoot.transform);
            pole.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            pole.transform.localScale = new Vector3(0.2f, 2.5f, 0.2f);
            if (poleMat != null) pole.GetComponent<MeshRenderer>().sharedMaterial = poleMat;
            Object.DestroyImmediate(pole.GetComponent<Collider>());

            // Gold Sphere Top
            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "Flag_GoldBall";
            ball.transform.SetParent(goalRoot.transform);
            ball.transform.localPosition = new Vector3(0f, 5.0f, 0f);
            ball.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            if (ballMat != null) ball.GetComponent<MeshRenderer>().sharedMaterial = ballMat;
            Object.DestroyImmediate(ball.GetComponent<Collider>());

            // Green Victory Flag (Cloth)
            GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
            flag.name = "Flag_Cloth";
            flag.transform.SetParent(goalRoot.transform);
            flag.transform.localPosition = new Vector3(0.85f, 4.4f, 0f);
            flag.transform.localScale = new Vector3(1.5f, 0.8f, 0.05f);
            if (flagMat != null) flag.GetComponent<MeshRenderer>().sharedMaterial = flagMat;
            Object.DestroyImmediate(flag.GetComponent<Collider>());

            // Trigger Collider on Goal (Ground level to top)
            BoxCollider goalCol = goalRoot.AddComponent<BoxCollider>();
            goalCol.isTrigger = true;
            goalCol.center = new Vector3(0f, 2.5f, 0f);
            goalCol.size = new Vector3(2.5f, 5.5f, 3f);

            Goal goalScript = goalRoot.AddComponent<Goal>();

            // Confetti Victory Particle System (Green victory burst)
            ParticleSystem ps = goalRoot.AddComponent<ParticleSystem>();
            var mainPs = ps.main;
            mainPs.playOnAwake = false;
            mainPs.loop = false;
            mainPs.startLifetime = 1.8f;
            mainPs.startSpeed = 8.5f;
            mainPs.startSize = 0.35f;
            mainPs.startColor = new Color(0.15f, 0.95f, 0.25f, 1f); // Vibrant Emerald Green
            mainPs.gravityModifier = 1.0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.6f;

            var emit = ps.emission;
            emit.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 80) });

            // Assign green flag material to ParticleSystemRenderer (prevents URP unassigned pink)
            ParticleSystemRenderer psRenderer = goalRoot.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null && flagMat != null)
            {
                psRenderer.sharedMaterial = flagMat;
            }

            SerializedObject soGoal = new SerializedObject(goalScript);
            soGoal.FindProperty("requiredCoins").intValue = 5;
            soGoal.FindProperty("victoryParticles").objectReferenceValue = ps;
            soGoal.FindProperty("victorySound").objectReferenceValue = sfxVictory;
            soGoal.ApplyModifiedProperties();
        }

        private static void CreateMarioClouds(Transform parent, Material cloudMat)
        {
            Vector3[] cloudPositions = new Vector3[]
            {
                new Vector3(8f, 6.5f, 6f),
                new Vector3(24f, 7.5f, 7f),
                new Vector3(38f, 6.0f, 6.5f),
                new Vector3(50f, 7.0f, 6f)
            };

            for (int i = 0; i < cloudPositions.Length; i++)
            {
                GameObject cloud = new GameObject($"Mario_Cloud_{i + 1}");
                cloud.transform.SetParent(parent);
                cloud.transform.position = cloudPositions[i];

                GameObject c1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c1.transform.SetParent(cloud.transform);
                c1.transform.localPosition = Vector3.zero;
                c1.transform.localScale = new Vector3(2.2f, 1.2f, 1.2f);
                if (cloudMat != null) c1.GetComponent<MeshRenderer>().sharedMaterial = cloudMat;
                Object.DestroyImmediate(c1.GetComponent<Collider>());

                GameObject c2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c2.transform.SetParent(cloud.transform);
                c2.transform.localPosition = new Vector3(-1.0f, -0.15f, 0f);
                c2.transform.localScale = new Vector3(1.5f, 0.9f, 0.9f);
                if (cloudMat != null) c2.GetComponent<MeshRenderer>().sharedMaterial = cloudMat;
                Object.DestroyImmediate(c2.GetComponent<Collider>());

                GameObject c3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                c3.transform.SetParent(cloud.transform);
                c3.transform.localPosition = new Vector3(1.0f, -0.15f, 0f);
                c3.transform.localScale = new Vector3(1.5f, 0.9f, 0.9f);
                if (cloudMat != null) c3.GetComponent<MeshRenderer>().sharedMaterial = cloudMat;
                Object.DestroyImmediate(c3.GetComponent<Collider>());
            }
        }

        private static void CreateMarioUI(GameObject managersRoot, GameManager gm)
        {
            GameObject existingCanvas = GameObject.Find("UI_Canvas");
            if (existingCanvas != null) Object.DestroyImmediate(existingCanvas);

            GameObject canvasObj = new GameObject("UI_Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();

            // Top Header Bar: Player Name (Left)
            GameObject playerLblObj = new GameObject("Text_PlayerLabel");
            playerLblObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI playerTmp = playerLblObj.AddComponent<TextMeshProUGUI>();
            playerTmp.text = "<size=28>PLAYER</size>\n<color=#FFFFFF>CUBE</color>";
            playerTmp.fontSize = 32;
            playerTmp.fontStyle = FontStyles.Bold;
            playerTmp.alignment = TextAlignmentOptions.TopLeft;
            playerTmp.color = Color.white;
            RectTransform pRect = playerLblObj.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0f, 1f);
            pRect.anchorMax = new Vector2(0f, 1f);
            pRect.pivot = new Vector2(0f, 1f);
            pRect.anchoredPosition = new Vector2(50f, -30f);
            pRect.sizeDelta = new Vector2(300f, 80f);

            // Top Header Bar: Coin Counter (Center)
            GameObject coinTextObj = new GameObject("Text_CoinCount");
            coinTextObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI coinTmp = coinTextObj.AddComponent<TextMeshProUGUI>();
            coinTmp.text = "<color=#FFD700>COIN</color>  x 00 / 05";
            coinTmp.fontSize = 36;
            coinTmp.fontStyle = FontStyles.Bold;
            coinTmp.alignment = TextAlignmentOptions.Center;
            coinTmp.color = Color.white;
            RectTransform coinRect = coinTextObj.GetComponent<RectTransform>();
            coinRect.anchorMin = new Vector2(0.5f, 1f);
            coinRect.anchorMax = new Vector2(0.5f, 1f);
            coinRect.pivot = new Vector2(0.5f, 1f);
            coinRect.anchoredPosition = new Vector2(0f, -35f);
            coinRect.sizeDelta = new Vector2(400f, 60f);

            // Top Header Bar: World 1-1 (Right)
            GameObject worldLblObj = new GameObject("Text_WorldLabel");
            worldLblObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI worldTmp = worldLblObj.AddComponent<TextMeshProUGUI>();
            worldTmp.text = "<size=28>STAGE</size>\n<color=#FFFFFF> 1 - 1 </color>";
            worldTmp.fontSize = 32;
            worldTmp.fontStyle = FontStyles.Bold;
            worldTmp.alignment = TextAlignmentOptions.TopRight;
            worldTmp.color = Color.white;
            RectTransform wRect = worldLblObj.GetComponent<RectTransform>();
            wRect.anchorMin = new Vector2(1f, 1f);
            wRect.anchorMax = new Vector2(1f, 1f);
            wRect.pivot = new Vector2(1f, 1f);
            wRect.anchoredPosition = new Vector2(-50f, -30f);
            wRect.sizeDelta = new Vector2(300f, 80f);

            // Title Banner
            GameObject titleObj = new GameObject("Banner_Title");
            titleObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
            titleTmp.text = "<color=#E52521>CUBE</color> <color=#FFD700>ADVENTURE</color>\n<size=28><color=#FFFFFF>Avoid the Cacti, Collect 5 Coins!</color></size>";
            titleTmp.fontSize = 58;
            titleTmp.fontStyle = FontStyles.Bold;
            titleTmp.alignment = TextAlignmentOptions.Center;
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.75f);
            titleRect.anchorMax = new Vector2(0.5f, 0.75f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(1000f, 180f);

            // Notification Text
            GameObject notifObj = new GameObject("Text_Notification");
            notifObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI notifTmp = notifObj.AddComponent<TextMeshProUGUI>();
            notifTmp.text = "";
            notifTmp.fontSize = 38;
            notifTmp.fontStyle = FontStyles.Bold;
            notifTmp.alignment = TextAlignmentOptions.Center;
            notifTmp.color = new Color(1f, 0.25f, 0.25f, 1f);
            RectTransform notifRect = notifObj.GetComponent<RectTransform>();
            notifRect.anchorMin = new Vector2(0.5f, 0.5f);
            notifRect.anchorMax = new Vector2(0.5f, 0.5f);
            notifRect.pivot = new Vector2(0.5f, 0.5f);
            notifRect.anchoredPosition = new Vector2(0f, 40f);
            notifRect.sizeDelta = new Vector2(800f, 80f);

            // Victory Panel
            GameObject victoryPanel = new GameObject("Panel_Victory");
            victoryPanel.transform.SetParent(canvasObj.transform, false);
            Image panelImg = victoryPanel.AddComponent<Image>();
            panelImg.color = new Color(0f, 0f, 0f, 0.82f);
            RectTransform vicRect = victoryPanel.GetComponent<RectTransform>();
            vicRect.anchorMin = Vector2.zero;
            vicRect.anchorMax = Vector2.one;
            vicRect.sizeDelta = Vector2.zero;

            GameObject vicTextObj = new GameObject("Text_Victory");
            vicTextObj.transform.SetParent(victoryPanel.transform, false);
            TextMeshProUGUI vicTmp = vicTextObj.AddComponent<TextMeshProUGUI>();
            vicTmp.text = "<color=#FFD700>STAGE CLEAR!</color>\n\n<size=34><color=#FFFFFF>YOU COLLECTED ALL 5 COINS!</color></size>";
            vicTmp.fontSize = 62;
            vicTmp.fontStyle = FontStyles.Bold;
            vicTmp.alignment = TextAlignmentOptions.Center;
            RectTransform vicTextRect = vicTextObj.GetComponent<RectTransform>();
            vicTextRect.anchorMin = new Vector2(0.5f, 0.65f);
            vicTextRect.anchorMax = new Vector2(0.5f, 0.65f);
            vicTextRect.pivot = new Vector2(0.5f, 0.5f);
            vicTextRect.sizeDelta = new Vector2(1000f, 220f);

            // Play Again Button
            GameObject btnObj = new GameObject("Button_PlayAgain");
            btnObj.transform.SetParent(victoryPanel.transform, false);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.12f, 0.56f, 1.0f, 1f); // Vibrant Blue Button
            Button btn = btnObj.AddComponent<Button>();

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.35f);
            btnRect.anchorMax = new Vector2(0.5f, 0.35f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.sizeDelta = new Vector2(340f, 75f);

            GameObject btnTextObj = new GameObject("Text_Btn");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTmp.text = "PLAY AGAIN  [ R ]";
            btnTmp.fontSize = 32;
            btnTmp.fontStyle = FontStyles.Bold;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = Color.white;
            RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
            btnTextRect.anchorMin = Vector2.zero;
            btnTextRect.anchorMax = Vector2.one;
            btnTextRect.sizeDelta = Vector2.zero;

            // Wire UIManager
            UIManager uim = canvasObj.AddComponent<UIManager>();
            SerializedObject soUi = new SerializedObject(uim);
            soUi.FindProperty("playerLabelText").objectReferenceValue = playerTmp;
            soUi.FindProperty("coinText").objectReferenceValue = coinTmp;
            soUi.FindProperty("worldLabelText").objectReferenceValue = worldTmp;
            soUi.FindProperty("titleBanner").objectReferenceValue = titleObj;
            soUi.FindProperty("notificationText").objectReferenceValue = notifTmp;
            soUi.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
            soUi.FindProperty("playAgainButton").objectReferenceValue = btn;
            soUi.ApplyModifiedProperties();

            // Wire GameManager to UIManager
            SerializedObject soGm = new SerializedObject(gm);
            soGm.FindProperty("uiManager").objectReferenceValue = uim;
            soGm.ApplyModifiedProperties();
        }

        private static void EnsureTag(string tag)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            bool exists = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue.Equals(tag))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                tagManager.ApplyModifiedProperties();
            }
        }
    }
}
#endif
