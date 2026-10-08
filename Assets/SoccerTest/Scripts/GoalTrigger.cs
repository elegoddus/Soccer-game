using UnityEngine;

namespace SoccerTest
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class GoalTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            BallController ball = other.GetComponent<BallController>();
            if (ball != null)
            {
                ball.Score();
            }
        }
    }
}
