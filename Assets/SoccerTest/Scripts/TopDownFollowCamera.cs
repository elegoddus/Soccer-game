using System.Collections;
using UnityEngine;

namespace SoccerTest
{
    [RequireComponent(typeof(Camera))]
    public sealed class TopDownFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 playerOffset = new Vector3(-2f, 15f, -10f);
        [SerializeField] private Vector3 ballOffset = new Vector3(-4f, 11f, -7f);
        [SerializeField] private float positionSmoothTime = 0.18f;
        [SerializeField] private float lookHeight = 0.8f;
        [SerializeField] private float ballFollowTimeout = 5f;

        private Transform target;
        private Vector3 currentOffset;
        private Vector3 velocity;
        private Coroutine returnRoutine;

        public void Initialize(Transform playerTarget)
        {
            player = playerTarget;
            FollowPlayerImmediate();
        }

        public void FollowBall(Transform ball)
        {
            if (returnRoutine != null)
            {
                StopCoroutine(returnRoutine);
                returnRoutine = null;
            }

            target = ball;
            currentOffset = ballOffset;
            returnRoutine = StartCoroutine(ReturnRoutine(ballFollowTimeout));
        }

        public void ReturnToPlayerAfter(float delay)
        {
            if (returnRoutine != null)
            {
                StopCoroutine(returnRoutine);
            }

            returnRoutine = StartCoroutine(ReturnRoutine(delay));
        }

        private IEnumerator ReturnRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            target = player;
            currentOffset = playerOffset;
            returnRoutine = null;
        }

        private void FollowPlayerImmediate()
        {
            target = player;
            currentOffset = playerOffset;
            if (target == null)
            {
                return;
            }

            transform.position = target.position + currentOffset;
            LookAtTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + currentOffset;
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                positionSmoothTime);
            LookAtTarget();
        }

        private void LookAtTarget()
        {
            Vector3 lookPoint = target.position + Vector3.up * lookHeight;
            transform.rotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
        }
    }
}
