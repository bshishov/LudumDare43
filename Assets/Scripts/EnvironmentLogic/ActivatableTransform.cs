using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class ActivatableTransform : MonoBehaviour
    {
        public Transform TargetTransform;
        public Vector3 DeltaPosition = Vector3.zero;
        public float ActivationSpeed = 2f;
        public float DeactivationSpeed = 2f;
        public AnimationCurve ActivationMovement = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve DeactivationMovement = AnimationCurve.Linear(0, 0, 1, 1);

        private float _state = 0f;
        private ActivatorProxy _activator;
        private Vector3 _initialPosition;
        private Vector3 _targedPosition;

        void Awake()
        {
            // If target is not set then select self object
            if (TargetTransform == null)
                TargetTransform = transform;

            _initialPosition = TargetTransform.position;
            _targedPosition = _initialPosition + DeltaPosition;
        }

        void Start()
        {
            _activator = GetComponent<ActivatorProxy>();
        }

        void Update()
        {
            if (_activator.IsActivated && _state < 1f)
            {
                _state += Time.deltaTime * DeactivationSpeed;
                TargetTransform.position = Vector3.Lerp(_initialPosition, _targedPosition, DeactivationMovement.Evaluate(_state));
            }

            if (!_activator.IsActivated && _state > 0f)
            {
                _state -= Time.deltaTime * ActivationSpeed;
                TargetTransform.position = Vector3.Lerp(_initialPosition, _targedPosition, ActivationMovement.Evaluate(_state));
            }
        }

        void OnDrawGizmosSelected()
        {
            var t = transform;
            if (TargetTransform != null)
                t = TargetTransform;
            var meshFilter = t.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                if (Application.isPlaying)
                    Gizmos.DrawWireMesh(meshFilter.sharedMesh, _targedPosition,
                        t.rotation, t.localScale);
                else
                    Gizmos.DrawWireMesh(meshFilter.sharedMesh, t.position + DeltaPosition,
                        t.rotation, t.localScale);
            }

            if (Application.isPlaying)
                Gizmos.DrawLine(_initialPosition, _targedPosition);
            else
                Gizmos.DrawLine(t.position, t.position + DeltaPosition);
        }
    }
}
