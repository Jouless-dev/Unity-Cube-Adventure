using UnityEngine;

namespace CubeAdventure.CameraControl
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Target transform to follow. If null, will search for Player tag.")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [Tooltip("Offset relative to the player position.")]
        [SerializeField] private Vector3 offset = new Vector3(3.5f, 2.0f, -10f);

        [Tooltip("Horizontal follow speed factor.")]
        [SerializeField] private float smoothSpeedX = 8.5f;

        [Header("Vertical Stability")]
        [Tooltip("Lock vertical camera movement for rock-solid, jitter-free 2.5D platforming.")]
        [SerializeField] private bool lockY = true;

        [Tooltip("Fixed camera height when lockY is enabled.")]
        [SerializeField] private float fixedCameraY = 2.5f;

        [Tooltip("Vertical follow speed factor if lockY is false.")]
        [SerializeField] private float smoothSpeedY = 4f;

        private void Start()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target != null)
            {
                float startY = lockY ? fixedCameraY : (target.position.y + offset.y);
                transform.position = new Vector3(target.position.x + offset.x, startY, offset.z);
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Smooth horizontal follow along the player's movement
            float desiredX = target.position.x + offset.x;
            float newX = Mathf.Lerp(transform.position.x, desiredX, smoothSpeedX * Time.deltaTime);

            // Rock-solid vertical camera: eliminates camera bouncing/vibration on jumps
            float newY;
            if (lockY)
            {
                newY = fixedCameraY;
            }
            else
            {
                float desiredY = target.position.y + offset.y;
                newY = Mathf.Lerp(transform.position.y, desiredY, smoothSpeedY * Time.deltaTime);
            }

            transform.position = new Vector3(newX, newY, offset.z);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
