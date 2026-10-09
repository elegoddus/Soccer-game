using UnityEngine;

namespace SoccerTest
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class BallController : MonoBehaviour
    {
        [SerializeField] private float flightDuration = 1.05f;

        private Rigidbody body;
        private SoccerGameManager manager;
        private bool hasBeenKicked;
        private bool hasScored;
        private Vector3 scoreTarget;
        private const float GoalDetectionRadius = 1.2f;
        private static PhysicsMaterial sharedBouncyMaterial;

        public bool IsAvailable => !hasBeenKicked && !hasScored;
        public bool HasScored => hasScored;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            GetComponent<SphereCollider>().material = GetBouncyMaterial();
        }

        private void Start()
        {
            if (manager == null)
            {
                SoccerGameManager.Instance?.RegisterBall(this);
            }
        }

        private void OnDisable()
        {
            manager?.UnregisterBall(this);
        }

        private void FixedUpdate()
        {
            if (hasBeenKicked && !hasScored &&
                Vector3.SqrMagnitude(transform.position - scoreTarget) <= GoalDetectionRadius * GoalDetectionRadius)
            {
                Score();
            }
        }

        public void Initialize(SoccerGameManager gameManager)
        {
            manager = gameManager;
        }

        public bool KickTo(Vector3 target)
        {
            if (!IsAvailable)
            {
                return false;
            }

            hasBeenKicked = true;
            scoreTarget = target;
            body.isKinematic = false;
            body.useGravity = true;
            body.angularVelocity = new Vector3(0f, 10f, -8f);

            float duration = Mathf.Max(0.2f, flightDuration);
            Vector3 displacement = target - transform.position;
            // Tinh van toc de bong toi dich dung thoi gian.
            Vector3 launchVelocity = new Vector3(
                displacement.x / duration,
                (displacement.y - 0.5f * Physics.gravity.y * duration * duration) / duration,
                displacement.z / duration);

#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = launchVelocity;
#else
            body.velocity = launchVelocity;
#endif
            return true;
        }

        public void Score()
        {
            if (hasScored)
            {
                return;
            }

            hasScored = true;
            if (manager == null)
            {
                manager = SoccerGameManager.Instance;
            }
            manager?.OnBallScored(this);
        }

        public static PhysicsMaterial GetBouncyMaterial()
        {
            if (sharedBouncyMaterial != null)
            {
                return sharedBouncyMaterial;
            }

            sharedBouncyMaterial = new PhysicsMaterial("Soccer Ball - Bouncy")
            {
                bounciness = 0.75f,
                dynamicFriction = 0.18f,
                staticFriction = 0.18f,
                bounceCombine = PhysicsMaterialCombine.Maximum,
                frictionCombine = PhysicsMaterialCombine.Minimum
            };
            return sharedBouncyMaterial;
        }
    }
}
