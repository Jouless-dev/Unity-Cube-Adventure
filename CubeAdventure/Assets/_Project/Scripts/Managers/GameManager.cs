#pragma warning disable 0649
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CubeAdventure.Collectibles;
using CubeAdventure.UI;

namespace CubeAdventure.Managers
{
    public enum GameState
    {
        Playing,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Configuration")]
        [SerializeField] private int requiredCoins = 5;

        [Header("UI Reference")]
        [SerializeField] private UIManager uiManager;

        // Runtime state
        public int Coins { get; private set; } = 0;
        public int RequiredCoins => requiredCoins;
        public GameState CurrentState { get; private set; } = GameState.Playing;
        public UIManager UIManager => uiManager;

        private readonly List<Coin> registeredCoins = new List<Coin>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (uiManager == null)
            {
                uiManager = FindAnyObjectByType<UIManager>();
            }

            ResetGameState();
        }

        public void RegisterCoin(Coin coin)
        {
            if (coin != null && !registeredCoins.Contains(coin))
            {
                registeredCoins.Add(coin);
            }
        }

        public void AddCoin(int amount)
        {
            if (CurrentState != GameState.Playing) return;

            Coins += amount;
            if (uiManager != null)
            {
                uiManager.UpdateCoinDisplay(Coins, requiredCoins);
            }
        }

        /// <summary>
        /// Geometry Dash style death: resets collected coins to 0 and restores all coins in the level.
        /// </summary>
        public void OnPlayerHitObstacle()
        {
            OnPlayerDeath();
        }

        public void OnPlayerDeath()
        {
            ResetCoins();

            if (uiManager != null)
            {
                uiManager.ShowNotification("<color=#FF4444>CRASH!</color> Coins Reset: 00 / 05");
            }
        }

        public void ResetCoins()
        {
            Coins = 0;

            // Find any coins if registeredCoins was not populated yet
            if (registeredCoins.Count == 0)
            {
                registeredCoins.AddRange(FindObjectsByType<Coin>(FindObjectsInactive.Include));
            }

            foreach (var coin in registeredCoins)
            {
                if (coin != null)
                {
                    coin.ResetCoin();
                }
            }

            if (uiManager != null)
            {
                uiManager.UpdateCoinDisplay(Coins, requiredCoins);
            }
        }

        public void CompleteLevel()
        {
            CurrentState = GameState.Victory;
            if (uiManager != null)
            {
                uiManager.ShowVictoryScreen();
            }
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ResetGameState()
        {
            Coins = 0;
            CurrentState = GameState.Playing;
            if (uiManager != null)
            {
                uiManager.UpdateCoinDisplay(Coins, requiredCoins);
                uiManager.ShowGameHUD();
            }
        }
    }
}
