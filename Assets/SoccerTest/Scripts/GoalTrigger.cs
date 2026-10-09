using UnityEngine;

namespace SoccerTest
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class GoalTrigger : MonoBehaviour
    {
        [Header("Tam chan bong")]
        [SerializeField] private Vector3 backstopCenter = new Vector3(0.75f, 0f, 0f);
        [SerializeField] private Vector3 backstopSize = new Vector3(0.15f, 2.2f, 3.35f);

        private void Awake()
        {
            BoxCollider backstop = gameObject.AddComponent<BoxCollider>();
            backstop.isTrigger = false;
            Vector3 center = backstopCenter;
            center.x = Mathf.Abs(center.x) * (transform.position.x < 0f ? -1f : 1f);
            backstop.center = center;
            backstop.size = backstopSize;
            backstop.material = BallController.GetBouncyMaterial();
        }

        private void OnTriggerEnter(Collider other)
        {
            BallController ball = other.GetComponentInParent<BallController>();
            if (ball != null)
            {
                ball.Score();
            }
        }
    }
}
