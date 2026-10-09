using UnityEngine;

namespace SoccerTest
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private Animator animator;

        [Header("Gioi han san (XZ)")]
        [Tooltip("Tam khung gioi han theo toa do world X/Z.")]
        [SerializeField] private Vector2 fieldCenter = Vector2.zero;
        [Tooltip("Kich thuoc khung theo X/Z, co the chinh trong Inspector.")]
        [SerializeField] private Vector2 fieldSize = new Vector2(22.5f, 16f);

        private CharacterController characterController;
        private static readonly int BlendHash = Animator.StringToHash("Blend");

        public Vector3 Position => transform.position;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Update()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 direction = new Vector3(input.x, 0f, input.y);
            Vector3 currentPosition = transform.position;
            Vector3 desiredPosition = currentPosition + direction * moveSpeed * Time.deltaTime;
            Vector2 halfSize = fieldSize * 0.5f;
            // Giu nhan vat ben trong san.
            desiredPosition.x = Mathf.Clamp(
                desiredPosition.x,
                fieldCenter.x - halfSize.x,
                fieldCenter.x + halfSize.x);
            desiredPosition.z = Mathf.Clamp(
                desiredPosition.z,
                fieldCenter.y - halfSize.y,
                fieldCenter.y + halfSize.y);

            Vector3 constrainedVelocity = Time.deltaTime > 0f
                ? (desiredPosition - currentPosition) / Time.deltaTime
                : Vector3.zero;
            characterController.SimpleMove(constrainedVelocity);

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);
            }

            if (animator != null)
            {
                animator.SetFloat(BlendHash, input.magnitude * 0.6f, 0.08f, Time.deltaTime);
            }
        }

        private void OnValidate()
        {
            fieldSize.x = Mathf.Max(0.1f, Mathf.Abs(fieldSize.x));
            fieldSize.y = Mathf.Max(0.1f, Mathf.Abs(fieldSize.y));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.8f, 0.1f, 1f);
            Vector3 center = new Vector3(fieldCenter.x, transform.position.y + 0.05f, fieldCenter.y);
            Vector3 size = new Vector3(fieldSize.x, 0.1f, fieldSize.y);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
