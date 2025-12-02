using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace NeonArena.Core
{
    /// <summary>
    /// Procedurally generated UI system - all UI elements created from code
    /// </summary>
    public class UISystem : MonoBehaviour
    {
        private Canvas hudCanvas;
        private Canvas overlayCanvas;

        // HUD Elements
        private Text healthText;
        private Image healthBar;
        private Text scoreText;
        private Text waveText;
        private Text fpsText;
        private Image crosshair;
        
        // Overlay Elements
        private GameObject pausePanel;
        private GameObject gameOverPanel;
        private Text waveNotificationText;

        private float fpsUpdateTimer;
        private int frameCount;

        public void Initialize()
        {
            CreateHUDCanvas();
            CreateOverlayCanvas();
            CreateHUDElements();
            CreateOverlays();
        }

        private void CreateHUDCanvas()
        {
            GameObject canvasObj = new GameObject("HUD_Canvas");
            canvasObj.transform.SetParent(transform);

            hudCanvas = canvasObj.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        private void CreateOverlayCanvas()
        {
            GameObject canvasObj = new GameObject("Overlay_Canvas");
            canvasObj.transform.SetParent(transform);

            overlayCanvas = canvasObj.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        private void CreateHUDElements()
        {
            CreateCrosshair();
            CreateHealthDisplay();
            CreateScoreDisplay();
            CreateWaveDisplay();
            CreateFPSCounter();
        }

        private void CreateCrosshair()
        {
            GameObject crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(hudCanvas.transform, false);

            RectTransform rect = crosshairObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(20, 20);

            crosshair = crosshairObj.AddComponent<Image>();
            crosshair.color = new Color(1f, 1f, 1f, 0.6f);

            // Create crosshair lines
            CreateCrosshairLine(crosshairObj, new Vector2(12, 2), Vector2.zero);
            CreateCrosshairLine(crosshairObj, new Vector2(2, 12), Vector2.zero);
        }

        private void CreateCrosshairLine(GameObject parent, Vector2 size, Vector2 position)
        {
            GameObject line = new GameObject("CrosshairLine");
            line.transform.SetParent(parent.transform, false);

            RectTransform rect = line.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            Image img = line.AddComponent<Image>();
            img.color = Rendering.NeonMaterialFactory.GetUIAccentColor();
        }

        private void CreateHealthDisplay()
        {
            // Health container
            GameObject healthContainer = new GameObject("HealthDisplay");
            healthContainer.transform.SetParent(hudCanvas.transform, false);

            RectTransform containerRect = healthContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0f, 0f);
            containerRect.anchorMax = new Vector2(0f, 0f);
            containerRect.pivot = new Vector2(0f, 0f);
            containerRect.anchoredPosition = new Vector2(30, 30);
            containerRect.sizeDelta = new Vector2(300, 60);

            // Health text
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(healthContainer.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = new Vector2(0, -20);

            healthText = textObj.AddComponent<Text>();
            healthText.font = Font.CreateDynamicFontFromOSFont("Arial", 24);
            healthText.fontSize = 24;
            healthText.fontStyle = FontStyle.Bold;
            healthText.alignment = TextAnchor.LowerLeft;
            healthText.color = Color.white;
            healthText.text = "HEALTH";

            // Health bar background
            GameObject barBg = new GameObject("HealthBarBG");
            barBg.transform.SetParent(healthContainer.transform, false);

            RectTransform bgRect = barBg.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(1, 0);
            bgRect.pivot = new Vector2(0, 0);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(0, 15);

            Image bgImage = barBg.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Health bar fill
            GameObject barFill = new GameObject("HealthBarFill");
            barFill.transform.SetParent(barBg.transform, false);

            RectTransform fillRect = barFill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.zero;
            fillRect.pivot = new Vector2(0, 0);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = new Vector2(300, 15);

            healthBar = barFill.AddComponent<Image>();
            healthBar.color = Rendering.NeonMaterialFactory.GetUIAccentColor();
            healthBar.type = Image.Type.Filled;
            healthBar.fillMethod = Image.FillMethod.Horizontal;
            healthBar.fillAmount = 1f;
        }

        private void CreateScoreDisplay()
        {
            GameObject scoreObj = new GameObject("ScoreDisplay");
            scoreObj.transform.SetParent(hudCanvas.transform, false);

            RectTransform rect = scoreObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-30, -30);
            rect.sizeDelta = new Vector2(300, 60);

            scoreText = scoreObj.AddComponent<Text>();
            scoreText.font = Font.CreateDynamicFontFromOSFont("Arial", 36);
            scoreText.fontSize = 36;
            scoreText.fontStyle = FontStyle.Bold;
            scoreText.alignment = TextAnchor.UpperRight;
            scoreText.color = Rendering.NeonMaterialFactory.GetUIAccentColor();
            scoreText.text = "SCORE: 0";
        }

        private void CreateWaveDisplay()
        {
            GameObject waveObj = new GameObject("WaveDisplay");
            waveObj.transform.SetParent(hudCanvas.transform, false);

            RectTransform rect = waveObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -30);
            rect.sizeDelta = new Vector2(400, 50);

            waveText = waveObj.AddComponent<Text>();
            waveText.font = Font.CreateDynamicFontFromOSFont("Arial", 32);
            waveText.fontSize = 32;
            waveText.fontStyle = FontStyle.Bold;
            waveText.alignment = TextAnchor.UpperCenter;
            waveText.color = Color.white;
            waveText.text = "WAVE 1";
        }

        private void CreateFPSCounter()
        {
            GameObject fpsObj = new GameObject("FPSCounter");
            fpsObj.transform.SetParent(hudCanvas.transform, false);

            RectTransform rect = fpsObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(30, -30);
            rect.sizeDelta = new Vector2(150, 40);

            fpsText = fpsObj.AddComponent<Text>();
            fpsText.font = Font.CreateDynamicFontFromOSFont("Arial", 20);
            fpsText.fontSize = 20;
            fpsText.alignment = TextAnchor.UpperLeft;
            fpsText.color = new Color(0.7f, 0.7f, 0.7f, 0.8f);
            fpsText.text = "FPS: 60";
        }

        private void CreateOverlays()
        {
            CreatePauseMenu();
            CreateGameOverScreen();
            CreateWaveNotification();
        }

        private void CreatePauseMenu()
        {
            pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(overlayCanvas.transform, false);

            RectTransform rect = pausePanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = pausePanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.85f);

            // Title
            GameObject titleObj = CreateTextElement(pausePanel, "PauseTitle", "PAUSED", 72, new Vector2(0, 100));
            
            // Instructions
            CreateTextElement(pausePanel, "PauseInstructions", "Press ESC to Resume", 28, new Vector2(0, 0));

            pausePanel.SetActive(false);
        }

        private void CreateGameOverScreen()
        {
            gameOverPanel = new GameObject("GameOverPanel");
            gameOverPanel.transform.SetParent(overlayCanvas.transform, false);

            RectTransform rect = gameOverPanel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image bg = gameOverPanel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.9f);

            // Title
            GameObject titleObj = CreateTextElement(gameOverPanel, "GameOverTitle", "GAME OVER", 72, new Vector2(0, 150));
            titleObj.GetComponent<Text>().color = new Color(1f, 0.3f, 0.3f);

            // Stats will be updated dynamically
            CreateTextElement(gameOverPanel, "FinalScore", "SCORE: 0", 48, new Vector2(0, 50));
            CreateTextElement(gameOverPanel, "FinalWave", "WAVE: 1", 36, new Vector2(0, 0));
            CreateTextElement(gameOverPanel, "FinalTime", "TIME: 00:00", 36, new Vector2(0, -50));

            gameOverPanel.SetActive(false);
        }

        private void CreateWaveNotification()
        {
            GameObject notifObj = new GameObject("WaveNotification");
            notifObj.transform.SetParent(overlayCanvas.transform, false);

            RectTransform rect = notifObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(800, 200);

            waveNotificationText = notifObj.AddComponent<Text>();
            waveNotificationText.font = Font.CreateDynamicFontFromOSFont("Arial", 64);
            waveNotificationText.fontSize = 64;
            waveNotificationText.fontStyle = FontStyle.Bold;
            waveNotificationText.alignment = TextAnchor.MiddleCenter;
            waveNotificationText.color = Rendering.NeonMaterialFactory.GetUIAccentColor();
            
            notifObj.SetActive(false);
        }

        private GameObject CreateTextElement(GameObject parent, string name, string text, int fontSize, Vector2 position)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(800, 100);

            Text txt = textObj.AddComponent<Text>();
            txt.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            txt.fontSize = fontSize;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = text;

            return textObj;
        }

        private void Update()
        {
            UpdateFPS();
        }

        private void UpdateFPS()
        {
            frameCount++;
            fpsUpdateTimer += Time.unscaledDeltaTime;

            if (fpsUpdateTimer >= 0.5f)
            {
                int fps = Mathf.RoundToInt(frameCount / fpsUpdateTimer);
                if (fpsText != null)
                {
                    fpsText.text = $"FPS: {fps}";
                }
                
                frameCount = 0;
                fpsUpdateTimer = 0f;
            }
        }

        public void UpdateHealth(float current, float max)
        {
            if (healthBar != null)
            {
                healthBar.fillAmount = current / max;
                
                // Color based on health percentage
                float percentage = current / max;
                if (percentage > 0.5f)
                {
                    healthBar.color = Rendering.NeonMaterialFactory.GetUIAccentColor();
                }
                else if (percentage > 0.25f)
                {
                    healthBar.color = new Color(1f, 0.7f, 0f);
                }
                else
                {
                    healthBar.color = new Color(1f, 0.3f, 0.3f);
                }
            }

            if (healthText != null)
            {
                healthText.text = $"HEALTH: {Mathf.RoundToInt(current)}";
            }
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {score}";
            }
        }

        public void UpdateWaveDisplay(int wave)
        {
            if (waveText != null)
            {
                waveText.text = $"WAVE {wave}";
            }
        }

        public void ShowWaveNotification(string message)
        {
            if (waveNotificationText != null)
            {
                StartCoroutine(WaveNotificationCoroutine(message));
            }
        }

        private IEnumerator WaveNotificationCoroutine(string message)
        {
            GameObject notifObj = waveNotificationText.gameObject;
            notifObj.SetActive(true);
            waveNotificationText.text = message;

            // Fade in
            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                Color c = waveNotificationText.color;
                c.a = t / 0.5f;
                waveNotificationText.text = message;
                yield return null;
            }

            yield return new WaitForSeconds(2f);

            // Fade out
            for (float t = 0; t < 0.5f; t += Time.deltaTime)
            {
                Color c = waveNotificationText.color;
                c.a = 1f - (t / 0.5f);
                waveNotificationText.color = c;
                yield return null;
            }

            notifObj.SetActive(false);
        }

        public void SetPauseScreen(bool active)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(active);
            }
        }

        public void ShowGameOver(int finalScore, int finalWave, float finalTime)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);

                // Update stats
                Transform scoreObj = gameOverPanel.transform.Find("FinalScore");
                if (scoreObj != null)
                {
                    scoreObj.GetComponent<Text>().text = $"SCORE: {finalScore}";
                }

                Transform waveObj = gameOverPanel.transform.Find("FinalWave");
                if (waveObj != null)
                {
                    waveObj.GetComponent<Text>().text = $"WAVE: {finalWave}";
                }

                Transform timeObj = gameOverPanel.transform.Find("FinalTime");
                if (timeObj != null)
                {
                    int minutes = Mathf.FloorToInt(finalTime / 60f);
                    int seconds = Mathf.FloorToInt(finalTime % 60f);
                    timeObj.GetComponent<Text>().text = $"TIME: {minutes:00}:{seconds:00}";
                }
            }
        }
    }
}

