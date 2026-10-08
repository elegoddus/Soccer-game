using UnityEngine;

namespace SoccerTest
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float rotationSpeed = 12f;
        [SerializeField] private Animator animator;

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
            characterController.SimpleMove(direction * moveSpeed);

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
    }
}
