using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class DrummerController : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private Camera _camera;

        // Use this for initialization
        void Start ()
        {
            _agent = GetComponent<NavMeshAgent>();
            _camera = Camera.main;
            CameraController.Instance.SetTarget(transform);
            CameraController.Instance.SetSecondaryTarget(Cursor.Instance.transform);
        }
        
        void Update ()
        {
            var movement = Vector3.zero;

            var forward = _camera.transform.forward;
            forward.y = 0f;
            movement += forward.normalized * Input.GetAxis("Vertical");

            var side = _camera.transform.right;
            side.y = 0f;
            movement += side.normalized * Input.GetAxis("Horizontal");

            _agent.destination = transform.position + movement.normalized;
        }
    }
}
