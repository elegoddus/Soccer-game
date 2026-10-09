using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SoccerTest
{
    public sealed class SoccerGameManager : MonoBehaviour
    {
        public static SoccerGameManager Instance { get; private set; }

        [Header("Gameplay")]
        [SerializeField] private PlayerController player;
        [SerializeField] private Transform targetGoal;
        [FormerlySerializedAs("balls")]
        [SerializeField] private BallController[] startingBalls;
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
            Instance = this;

            foreach (BallController ball in startingBalls ?? Array.Empty<BallController>())
            {
                RegisterBall(ball);
            }

            DiscoverAndConfigureSceneBalls();

            followCamera.Initialize(player.transform);
            kickButton.onClick.AddListener(KickNearbyBall);
            autoKickButton.onClick.AddListener(AutoKick);
            SetStatus("Di chuyển bằng W A S D và đến gần quả bóng");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            nearbyBall = FindNearestAvailableBall(true);
            bool canKick = nearbyBall != null;
            if (kickButton.gameObject.activeSelf != canKick)
            {
                kickButton.gameObject.SetActive(canKick);
            }

            autoKickButton.interactable = FindFarthestAvailableBall() != null;
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
            BallController farthestBall = FindFarthestAvailableBall();
            if (farthestBall != null)
            {
                Kick(farthestBall);
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
                Vector3 effectPosition = ball.transform.position + Vector3.up * 0.5f;
                GameObject effect = Instantiate(goalVfxPrefab, effectPosition, Quaternion.identity);
                effect.transform.localScale *= 1.35f;

                ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    particleSystem.Clear(true);
                    particleSystem.Play(true);
                }

                Destroy(effect, 5f);
            }

            followCamera.ReturnToPlayerAfter(2f);
        }

        public void RegisterBall(BallController ball)
        {
            if (ball == null || activeBalls.Contains(ball))
            {
                return;
            }

            ball.Initialize(this);
            activeBalls.Add(ball);
        }

        public void UnregisterBall(BallController ball)
        {
            activeBalls.Remove(ball);
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
            float bestDistanceSqr = float.MaxValue;
            float kickRangeSqr = kickRange * kickRange;

            foreach (BallController ball in activeBalls)
            {
                if (ball == null || !ball.IsAvailable)
                {
                    continue;
                }

                float distanceSqr = Vector3.SqrMagnitude(player.Position - ball.transform.position);
                if (enforceRange && distanceSqr > kickRangeSqr)
                {
                    continue;
                }

                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    best = ball;
                }
            }

            return best;
        }

        private BallController FindFarthestAvailableBall()
        {
            BallController farthest = null;
            float farthestDistanceSqr = -1f;

            foreach (BallController ball in activeBalls)
            {
                if (ball == null || !ball.IsAvailable)
                {
                    continue;
                }

                float distanceSqr = Vector3.SqrMagnitude(player.Position - ball.transform.position);
                if (distanceSqr > farthestDistanceSqr)
                {
                    farthestDistanceSqr = distanceSqr;
                    farthest = ball;
                }
            }

            return farthest;
        }

        private void DiscoverAndConfigureSceneBalls()
        {
            foreach (SphereCollider sphere in FindObjectsByType<SphereCollider>())
            {
                if (!sphere.gameObject.name.StartsWith("Soccer Ball", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Rigidbody body = sphere.GetComponent<Rigidbody>();
                if (body == null)
                {
                    body = sphere.gameObject.AddComponent<Rigidbody>();
                    body.mass = 0.45f;
                    body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    body.interpolation = RigidbodyInterpolation.Interpolate;
                    body.maxAngularVelocity = 30f;
                }

                BallController ball = sphere.GetComponent<BallController>();
                if (ball == null)
                {
                    ball = sphere.gameObject.AddComponent<BallController>();
                }

                RegisterBall(ball);
            }
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
