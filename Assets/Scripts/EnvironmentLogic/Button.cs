using System.Linq;
using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    [RequireComponent(typeof(Collider))]
    public class Button : MonoBehaviour {

        public ActivatorProxy[] Targets;
        public float PressDepth = 0.1f;
        public float PressingSpeed = 4f;
        public string[] RequiredTags;
        public AudioClipWithVolume PressSound;

        private int _playersTriggered = 0;
        private float _currentState = 0f;
        private Vector3 _initialPosition;
        private Vector3 _pressedPosition;

        public bool IsActivated { get; private set; }

        void Start()
        {
            _initialPosition = transform.position;
            _pressedPosition = transform.position + Vector3.down * PressDepth;
            IsActivated = false;

            if (!GetComponent<Collider>().isTrigger)
                Debug.LogWarning("Button collider must be a trigger", this);
        }

        void Update()
        {
            if (IsActivated && _currentState < 1f)
            {
                _currentState += Time.deltaTime * PressingSpeed;
                transform.position = Vector3.Lerp(_initialPosition, _pressedPosition, _currentState);
            }

            if (!IsActivated && _currentState > 0f)
            {
                _currentState -= Time.deltaTime * PressingSpeed;
                transform.position = Vector3.Lerp(_initialPosition, _pressedPosition, _currentState);
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (RequiredTags.Contains(col.gameObject.tag))
            {
                _playersTriggered++;

                if (!IsActivated && _playersTriggered == 1)
                {
                    SoundManager.Instance.Play(PressSound);
                    IsActivated = true;
                    foreach (var target in Targets)
                    {
                        if (target != null)
                        {
                            target.Activate();
                        }
                    }
                }
            }
        }

        void OnTriggerExit(Collider col)
        {
            if (RequiredTags.Contains(col.gameObject.tag))
            {
                _playersTriggered--;

                if (IsActivated && _playersTriggered == 0)
                {
                    IsActivated = false;
                    foreach (var target in Targets)
                    {
                        if (target != null)
                        {
                            target.Deactivate();
                        }
                    }
                }
            }
        }

        void OnDrawGizmosSelected()
        {
            if (Targets != null)
            {
                foreach (var target in Targets)
                {
                    if (target != null)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(transform.position, target.transform.position);
                    }
                }
            }
        }
    }
}
