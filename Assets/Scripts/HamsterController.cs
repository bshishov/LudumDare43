using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class HamsterController : MonoBehaviour
    {
        public float DistanceToLooseControl = 1f;
        public bool DirectMouseMovement = false;

        private Camera _camera;

        // store if hamster is under drum-control
        private bool _commanded = false;

        // movement anchor point
        private Vector3 _commandTarget = Vector3.zero;

        // random movements
        private float _lastRandomMovementTime = 0f;
        private float _lastRandomMovementInterval = 0.5f;
        private Vector3 _randomCommandTarget = Vector3.zero;
        // distance for ranmom movement targets
        private float _randomMovementDistance = 2.3f;
        // max distance from movement anchor point
        private float _maxRandomMovementDistance = 5f;

        private NavMeshAgent _agent;

        void Start()
        {
            _commandTarget = this.transform.position;
            _camera = Camera.main;
            _agent = GetComponent<NavMeshAgent>();
        }

        void Update()
        {
            if(DirectMouseMovement)
                CheckClick();

            if (_commanded)
            {
                if (_agent.remainingDistance <= DistanceToLooseControl)
                {
                    _commanded = false;
                }
            }
            else
            {
                MakeRandomMovement();
            }
        }

        public void MakeRandomMovement()
        {
            if (Time.time - _lastRandomMovementTime > _lastRandomMovementInterval)
            {
                var interationCount = 0;
                do
                {
                    if (++interationCount > 100)
                    {
                        _randomMovementDistance *= 0.1f;
                    }
                    _randomCommandTarget = _agent.transform.position + new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f)).normalized * _randomMovementDistance;
                } while ((_randomCommandTarget - _commandTarget).magnitude > _maxRandomMovementDistance);
                _lastRandomMovementTime = Time.time;
                Debug.Log(new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f)).x);
                _agent.SetDestination(_randomCommandTarget);
            }
        }

        public void SetDestination(Vector3 command)
        {
            _commanded = true;
            _commandTarget = command;
            _agent.SetDestination(_commandTarget);
        }

        private void CheckClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    _commanded = true;
                    _commandTarget = hit.point;
                    _agent.SetDestination(hit.point);
                }
            }
        }
    }
}
