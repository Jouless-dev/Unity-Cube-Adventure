#pragma warning disable 0649
using UnityEngine;
using CubeAdventure.Managers;

namespace CubeAdventure.Gameplay
{
    public class Goal : MonoBehaviour
    {
        [Header("Goal Requirements")]
        [Tooltip("Coins needed to unlock the goal.")]
        [SerializeField] private int requiredCoins = 5;

        [Header("Goal Effects")]
        [SerializeField] private ParticleSystem victoryParticles;
        [SerializeField] private AudioClip victorySound;
        [SerializeField] private AudioClip lockedSound;

        private bool isGoalTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (isGoalTriggered) return;

            if (other.CompareTag("Player"))
            {
                int currentCoins = GameManager.Instance != null ? GameManager.Instance.Coins : 0;

                if (currentCoins >= requiredCoins)
                {
                    TriggerVictory();
                }
                else
                {
                    TriggerIncompleteFeedback(currentCoins);
                }
            }
        }

        private void TriggerVictory()
        {
            isGoalTriggered = true;

            // Play particles
            if (victoryParticles != null)
            {
                victoryParticles.Play();
            }

            // Play sound
            if (victorySound != null)
            {
                AudioSource.PlayClipAtPoint(victorySound, transform.position, 1.0f);
            }

            // Notify GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }

        private void TriggerIncompleteFeedback(int currentCoins)
        {
            if (lockedSound != null)
            {
                AudioSource.PlayClipAtPoint(lockedSound, transform.position, 0.8f);
            }

            if (GameManager.Instance != null && GameManager.Instance.UIManager != null)
            {
                int missing = requiredCoins - currentCoins;
                GameManager.Instance.UIManager.ShowNotification($"Collect {missing} more coin{(missing > 1 ? "s" : "")} to finish!");
            }
        }
    }
}
