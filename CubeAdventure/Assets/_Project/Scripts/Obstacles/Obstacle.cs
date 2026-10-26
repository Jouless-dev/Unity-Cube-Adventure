#pragma warning disable 0649
using UnityEngine;
using CubeAdventure.Player;

namespace CubeAdventure.Obstacles
{
    public class Obstacle : MonoBehaviour
    {
        [Header("Obstacle Effects")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private GameObject hitEffectPrefab;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                HandlePlayerHit(collision.gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                HandlePlayerHit(other.gameObject);
            }
        }

        private void HandlePlayerHit(GameObject playerObj)
        {
            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, transform.position, 1.0f);
            }

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, playerObj.transform.position, Quaternion.identity);
            }

            PlayerController player = playerObj.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeHitAndRespawn();
            }

            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.OnPlayerHitObstacle();
            }
        }
    }
}
