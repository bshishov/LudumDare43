using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    [RequireComponent(typeof(Collider))]
    public class Button : MonoBehaviour
    {
        [Header("General")]
        public string[] RequiredTags;
        [Range(1, 10)]
        public int Required = 1;

        public ActivatorProxy[] Targets;
        public AudioClipWithVolume PressSound;

        [Header("Visuals")]
        public float PressDepth = 0.1f;
        public float PressingSpeed = 4f;

        [Header("Indicators")]
        public AudioClipWithVolume IndicatorActivateSound;
        public bool UseIndicators = false;
        public GameObject IndicatorPrefab;
        public float IndicatorsRadius = 1f;
        public float IndicatorOffset = 0f;
        public float IndicatorAngleStep = 1f;
        public float IndicatorY = 0f;

        private int _playersTriggered = 0;
        private float _currentState = 0f;
        private Vector3 _initialPosition;
        private Vector3 _pressedPosition;

        private Flame[] _indicatorFlames;

        public bool IsActivated { get; private set; }

        void Start()
        {
            _initialPosition = transform.position;
            _pressedPosition = transform.position + Vector3.down * PressDepth;
            IsActivated = false;

            if (!GetComponent<Collider>().isTrigger)
                Debug.LogWarning("Button collider must be a trigger", this);

            if (UseIndicators && IndicatorPrefab != null)
            {
                _indicatorFlames = new Flame[Required];
                for (var i = 0; i < Required; i++)
                {
                    var localPos = GetIndicatorPosition(i);
                    var go = GameObject.Instantiate(IndicatorPrefab, transform.TransformPoint(localPos), IndicatorPrefab.transform.rotation);
                    go.transform.SetParent(transform, true);

                    _indicatorFlames[i] = go.GetComponentInChildren<Flame>();
                }
            }
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

                if (UseIndicators && _indicatorFlames != null && _playersTriggered <= Required)
                {
                    var indicator = _indicatorFlames[_playersTriggered - 1];
                    indicator.StartEmission();
                    indicator.AddTrauma(1f);
                }

                SoundManager.Instance.Play(IndicatorActivateSound);

                if (!IsActivated && _playersTriggered == Required)
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

                if (UseIndicators && _indicatorFlames != null && _playersTriggered < Required)
                    _indicatorFlames[_playersTriggered].StopEmission();

                if (IsActivated && _playersTriggered < Required)
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

            
            for (var i = 0; i < Required; i++)
            {
                Gizmos.DrawSphere(transform.TransformPoint(GetIndicatorPosition(i)), 0.1f);
            }
        }

        private Vector3 GetIndicatorPosition(int i)
        {
            var angle = IndicatorOffset + i * IndicatorAngleStep;
            return new Vector3(IndicatorsRadius * Mathf.Sin(angle), IndicatorY, IndicatorsRadius * Mathf.Cos(angle));
        }
    }
}
