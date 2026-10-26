#pragma warning disable 0649
#pragma warning disable 0169
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CubeAdventure.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Mario Style HUD Elements")]
        [SerializeField] private TextMeshProUGUI playerLabelText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI worldLabelText;
        [SerializeField] private TextMeshProUGUI notificationText;

        [Header("Panels")]
        [SerializeField] private GameObject titleBanner;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Button playAgainButton;

        private Coroutine notificationCoroutine;

        private void Start()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }

            if (titleBanner != null)
            {
                titleBanner.SetActive(true);
                StartCoroutine(HideTitleBannerAfterDelay(3.5f));
            }

            if (notificationText != null)
            {
                notificationText.gameObject.SetActive(false);
            }

            if (playAgainButton != null)
            {
                playAgainButton.onClick.AddListener(OnRestartButtonClicked);
            }
        }

        public void UpdateCoinDisplay(int currentCoins, int requiredCoins)
        {
            if (coinText != null)
            {
                coinText.text = $"<color=#FFD700>COIN</color>  x {currentCoins:00} / {requiredCoins:00}";
            }
        }

        public void ShowVictoryScreen()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            if (notificationText != null)
            {
                notificationText.gameObject.SetActive(false);
            }
        }

        public void ShowGameHUD()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
        }

        public void ShowNotification(string message, float duration = 2.0f)
        {
            if (notificationText == null) return;

            if (notificationCoroutine != null)
            {
                StopCoroutine(notificationCoroutine);
            }

            notificationCoroutine = StartCoroutine(DisplayNotificationRoutine(message, duration));
        }

        public void OnRestartButtonClicked()
        {
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.RestartGame();
            }
        }

        private IEnumerator HideTitleBannerAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (titleBanner != null)
            {
                titleBanner.SetActive(false);
            }
        }

        private IEnumerator DisplayNotificationRoutine(string message, float duration)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);

            yield return new WaitForSeconds(duration);

            notificationText.gameObject.SetActive(false);
            notificationCoroutine = null;
        }
    }
}
