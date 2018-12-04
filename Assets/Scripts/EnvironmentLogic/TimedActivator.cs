using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class TimedActivator : MonoBehaviour
    {
        public float TimeOffset = 0f;
        public float ActivateAtTime = 1f;
        public float DeactivateAtTime = 2f;
        public float TotalDuration = 4f;

        public ActivatorProxy[] Targets;
        public bool Loop = true;

        private float _currentTime = 0f;
        private bool _activated = false;

        void Start()
        {
            _currentTime = TimeOffset;
            if (DeactivateAtTime < ActivateAtTime)
                Debug.LogWarning("DeactivateAtTime time must be more than ActivateAtTime");

            if (DeactivateAtTime > TotalDuration)
                Debug.LogWarning("Duration must be more than DeactivateAtTime");
        }

        void Update()
        {
            _currentTime += Time.deltaTime;

            if (!_activated && _currentTime > ActivateAtTime && _currentTime < DeactivateAtTime)
            {
                _activated = true;
                Activate();
            }

            if (_activated && _currentTime > DeactivateAtTime)
            {
                _activated = false;
                Deactivate();
            }

            if (_currentTime > TotalDuration && Loop)
                _currentTime = 0;
        }

        void Activate()
        {
            if (Targets != null)
            {
                foreach (var target in Targets)
                {
                    if (target != null)
                        target.Activate();
                }
            }
        }

        void Deactivate()
        {
            if (Targets != null)
            {
                foreach (var target in Targets)
                {
                    if (target != null)
                        target.Deactivate();
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (Targets != null)
            {
                foreach (var target in Targets)
                {
                    if (target != null && target != gameObject)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(transform.position, target.transform.position);
                    }
                }
            }
        }
    }
}
