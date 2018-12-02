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
        private float _timeFromLastCommandLoose = 0f;
        public float TimeToWaitAfterLastCommandLoose = 2f;
        private float _lastCommandTime = 0f;
        public float TimeToRunAfterCommand = 1f;

        // movement anchor point
        private Vector3 _commandTarget = Vector3.zero;

        // random movements
        private float _lastRandomMovementTime = 0f;
        private float _lastRandomMovementInterval = 0.5f;
        private Vector3 _randomCommandTarget = Vector3.zero;
        // distance for ranmom movement targets
        private float _randomMovementDistance = 2f;
        // max distance from movement anchor point
        private float _maxRandomMovementDistance = 5f;

        private NavMeshAgent _agent;

        void Start()
        {
            _commandTarget = transform.position;
            _camera = Camera.main;
            _agent = GetComponent<NavMeshAgent>();
        }

        void Update()
        {
            if (DirectMouseMovement)
                CheckClick();
            
            if (_commanded)
            {
                var timeFromLastCommandPassed = CheckInterva(_lastCommandTime, TimeToRunAfterCommand);
                var reachedDestination = (_agent.transform.position - _commandTarget).magnitude <= DistanceToLooseControl;
                if (timeFromLastCommandPassed || reachedDestination)
                {
                    LooseDestination();
                }
            }
            else
            {
                var timeFromLastDestinationLoosePassed = CheckInterva(_timeFromLastCommandLoose, TimeToWaitAfterLastCommandLoose);
                var timeFromLastRandomMovementPassed = CheckInterva(_lastRandomMovementTime, _lastRandomMovementInterval);
                if (timeFromLastDestinationLoosePassed && timeFromLastRandomMovementPassed)
                {
                    Debug.Log("RandonMove();");
                    MakeRandomMovement();
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(_commandTarget, _maxRandomMovementDistance);
        }

        public void MakeRandomMovement()
        {
            _randomCommandTarget = CalculateRandomTarget();
            _lastRandomMovementTime = Time.time;
            _agent.SetDestination(_randomCommandTarget);
        }

        private Vector3 CalculateRandomTarget()
        {
            Vector3 newTarget;
            var interationCount = 0;
            var distanceFromNewPosition = 0f;
            var distanceFromCurrentPosition = (_agent.transform.position - _commandTarget).magnitude;

            do
            {
                if (++interationCount > 5)
                {
                    interationCount = 0;
                    _randomMovementDistance *= 0.5f;
                }
                newTarget = _agent.transform.position + Random.insideUnitSphere * _randomMovementDistance;
                distanceFromNewPosition = (newTarget - _commandTarget).magnitude;
            } while (
                _randomMovementDistance > 0.1f &&
                distanceFromNewPosition > _maxRandomMovementDistance &&
                distanceFromNewPosition > distanceFromCurrentPosition
            );

            return newTarget;
        }

        public void SetDestination(Vector3 command, float bmp  = 120f)
        {
            _commanded = true;
            _commandTarget = command;

            _lastCommandTime = Time.time;
            _agent.SetDestination(_commandTarget);
        }

        public void LooseDestination()
        {
            _commanded = false;
            var movementDirection = _commandTarget - transform.position;
            _commandTarget = transform.position;
            _agent.SetDestination(_commandTarget + movementDirection.normalized);

            _timeFromLastCommandLoose = Time.time;
        }

        private void CheckClick()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    SetDestination(hit.point);
                }
            }
        }

        private bool CheckInterva(float from, float duration)
        {
            return from + duration - Time.time < 0;
        }
    }
}
