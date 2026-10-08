using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SoccerTest
{
    public sealed class SoccerGameManager : MonoBehaviour
    {
        [Header("Gameplay")]
        [SerializeField] private PlayerController player;
        [SerializeField] private Transform targetGoal;
        [SerializeField] private BallController[] balls;
        [SerializeField] private float kickRange = 2.5f;

        [Header("Presentation")]
        [SerializeField] private TopDownFollowCamera followCamera;
        [SerializeField] private GameObject goalVfxPrefab;
        [SerializeField] private Button kickButton;
        [SerializeField] private Button autoKickButton;
        [SerializeField] private Text statusText;

        private BallController nearbyBall;
        private readonly List<BallController> activeBalls = new List<BallController>();

        private void Awake()
        {
            foreach (BallController ball in balls)
            {
                if (ball == null)
                {
                    continue;
                }

                ball.Initialize(this);
                activeBalls.Add(ball);
            }

            followCamera.Initialize(player.transform);
            kickButton.onClick.AddListener(KickNearbyBall);
            autoKickButton.onClick.AddListener(AutoKick);
            SetStatus("Di chuyển bằng W A S D và đến gần quả bóng");
        }

        private void Update()
        {
            nearbyBall = FindNearestAvailableBall(true);
            bool canKick = nearbyBall != null;
            if (kickButton.gameObject.activeSelf != canKick)
            {
                kickButton.gameObject.SetActive(canKick);
            }

            autoKickButton.interactable = FindNearestAvailableBall(false) != null;
        }

        public void KickNearbyBall()
        {
            if (nearbyBall != null)
            {
                Kick(nearbyBall);
            }
        }

        public void AutoKick()
        {
            BallController closestBall = FindNearestAvailableBall(false);
            if (closestBall != null)
            {
                Kick(closestBall);
            }
        }

        public void ResetScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OnBallScored(BallController ball)
        {
            SetStatus("GOAL! Camera sẽ trở lại nhân vật sau 2 giây");
            if (goalVfxPrefab != null)
            {
                GameObject effect = Instantiate(goalVfxPrefab, targetGoal.position, Quaternion.identity);
                Destroy(effect, 4f);
            }

            followCamera.ReturnToPlayerAfter(2f);
        }

        private void Kick(BallController ball)
        {
            if (!ball.KickTo(targetGoal.position))
            {
                return;
            }

            SetStatus("Sút bóng!");
            followCamera.FollowBall(ball.transform);
        }

        private BallController FindNearestAvailableBall(bool enforceRange)
        {
            BallController best = null;
            float bestDistance = float.MaxValue;

            foreach (BallController ball in activeBalls)
            {
                if (ball == null || !ball.IsAvailable)
                {
                    continue;
                }

                float distance = Vector3.Distance(player.Position, ball.transform.position);
                if (enforceRange && distance > kickRange)
                {
                    continue;
                }

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = ball;
                }
            }

            return best;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
