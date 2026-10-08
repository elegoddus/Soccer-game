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

        public bool IsAvailable => !hasBeenKicked && !hasScored;
        public bool HasScored => hasScored;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
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
            body.isKinematic = false;
            body.useGravity = true;
            body.angularVelocity = new Vector3(0f, 10f, -8f);

            float duration = Mathf.Max(0.2f, flightDuration);
            Vector3 displacement = target - transform.position;
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
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity = Vector3.zero;
#else
            body.velocity = Vector3.zero;
#endif
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            manager?.OnBallScored(this);
        }
    }
}
