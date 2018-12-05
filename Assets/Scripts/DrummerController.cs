using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class DrummerController : Singleton<DrummerController>
    {
        public float HandMovementTime = 0.2f;
        public bool IsAlive { get; private set; }
        private NavMeshAgent _agent;
        private Camera _camera;
        private Animator _animator;
        private RagdollController _ragdollController;
        private IKController _ikController;

        private Vector3 _rhVelocity;
        private Vector3 _lhVelocity;

        private Cursor _cursor;
        private DrumController _drumController;

        void Start ()
        {
            IsAlive = true;
            _ikController = GetComponent<IKController>();
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _camera = Camera.main;
            _ragdollController = GetComponent<RagdollController>();
            CameraController.Instance.SetTarget(transform);

            _cursor = Cursor.Instance;
            _drumController = DrumController.Instance;

            if (_cursor != null)
            {
                CameraController.Instance.SetSecondaryTarget(Cursor.Instance.transform);

                if (_ikController != null)
                {
                    _ikController.IkActive = true;
                    _ikController.LookObj = _cursor.transform;
                }
            }
        }
        
        void Update ()
        {
            if(!IsAlive)
                return;

            var movement = Vector3.zero;

            var forward = _camera.transform.forward;
            forward.y = 0f;
            movement += forward.normalized * Input.GetAxis("Vertical");

            var side = _camera.transform.right;
            side.y = 0f;
            movement += side.normalized * Input.GetAxis("Horizontal");

            if (_agent.enabled)
            {
                _agent.destination = transform.position + movement.normalized;
                _animator.SetFloat("Speed", _agent.velocity.magnitude);
            }


            // HAND IK
            if (_ikController != null)
            {
                if (_drumController != null)
                {
                    var lfDcLocalTarget = _drumController.LeftHandSourcePosition;
                    var rfDcLocalTarget = _drumController.RightHandSourcePosition;

                    if (Input.GetMouseButton(0))
                        lfDcLocalTarget = _drumController.DrumHitCenter;

                    if (Input.GetMouseButton(1))
                        rfDcLocalTarget = _drumController.DrumHitCenter;

                    _ikController.LeftHandTargetPosition = Vector3.SmoothDamp(_ikController.LeftHandTargetPosition,
                        _drumController.transform.TransformPoint(lfDcLocalTarget),
                        ref _lhVelocity, HandMovementTime);
                    _ikController.RightHandTargetPosition = Vector3.SmoothDamp(_ikController.RightHandTargetPosition,
                        _drumController.transform.TransformPoint(rfDcLocalTarget),
                        ref _rhVelocity, HandMovementTime);
                }
            }
        }

        public void Die()
        {
            IsAlive = false;
            DrumController.Instance.enabled = false;
            LevelManager.Instance.Restart();

            if (_ragdollController != null)
                _ragdollController.EnableRagdoll();

            Debug.Log("[DRUMMER] Death");
        }

        void OnTriggerEnter(Collider col)
        {
            Debug.Log(string.Format("[DRUMMER] trigger with {0}", col.name));
            if (col.CompareTag("Killer"))
            {
                Die();
            }
        }

        void OnCollisionEnter(Collision col)
        {
            Debug.Log(string.Format("[DRUMMER] collision with {0}", col.collider.name));
            if (col.collider.CompareTag("Killer"))
            {
                Die();
            }
        }
    }
}
