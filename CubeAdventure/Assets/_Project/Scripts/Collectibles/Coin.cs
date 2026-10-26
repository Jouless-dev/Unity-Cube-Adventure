#pragma warning disable 0649
using UnityEngine;

namespace CubeAdventure.Collectibles
{
    public class Coin : MonoBehaviour
    {
        [Header("Coin Settings")]
        [SerializeField] private int value = 1;
        [SerializeField] private float rotationSpeed = 120f;
        [SerializeField] private float bobbingSpeed = 2f;
        [SerializeField] private float bobbingHeight = 0.15f;

        [Header("Effects")]
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private GameObject pickupEffectPrefab;

        private Vector3 startPos;
        private bool isCollected = false;
        private MeshRenderer meshRenderer;
        private Collider coinCollider;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            coinCollider = GetComponent<Collider>();
        }

        private void Start()
        {
            startPos = transform.position;

            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.RegisterCoin(this);
            }
        }

        private void Update()
        {
            if (isCollected) return;

            // Continuous spinning
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // Gentle up/down bobbing
            float newY = startPos.y + Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }

        private void Collect()
        {
            isCollected = true;

            // Notify GameManager
            if (Managers.GameManager.Instance != null)
            {
                Managers.GameManager.Instance.AddCoin(value);
            }

            // Play pickup sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1.0f);
            }

            // Instantiate pickup particle effect
            if (pickupEffectPrefab != null)
            {
                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
            }

            // Geometry Dash style: disable renderer and collider so it can be restored on death
            if (meshRenderer != null) meshRenderer.enabled = false;
            if (coinCollider != null) coinCollider.enabled = false;
        }

        public void ResetCoin()
        {
            isCollected = false;
            transform.position = startPos;
            if (meshRenderer != null) meshRenderer.enabled = true;
            if (coinCollider != null) coinCollider.enabled = true;
        }
    }
}
