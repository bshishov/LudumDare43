using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class DrummerController : MonoBehaviour
    {
        public bool IsAlive { get; private set; }
        private NavMeshAgent _agent;
        private Camera _camera;
        private Animator _animator;
        private RagdollController _ragdollController;


        // Use this for initialization
        void Start ()
        {
            IsAlive = true;
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _camera = Camera.main;
            _ragdollController = GetComponent<RagdollController>();
            CameraController.Instance.SetTarget(transform);
            CameraController.Instance.SetSecondaryTarget(Cursor.Instance.transform);
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

            _agent.destination = transform.position + movement.normalized;
            _animator.SetFloat("Speed", _agent.velocity.magnitude);
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
