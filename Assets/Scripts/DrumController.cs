using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Drum))]
    public class DrumController : MonoBehaviour
    {
        [Header("Cursor")]
        [Range(0.5f, 10f)]
        public float GatherRadius = 5f;

        [Header("Drum")]
        public float MaxEnergy = 1000f;
        public float DrainEnergyPerNote = 1f;
        public float DrainEnergyPerСommand = 2f;

        public float CurrentEnergy
        {
            get { return _currentEnergy; }
        }

        public float EnergyFraction
        {
            get { return _currentEnergy / MaxEnergy; }
        }

        private float _currentEnergy;
        private Drum _drum;
        private Camera _camera;
        
        private bool _cursorIsHittingGround;
        private Vector3 _cursorWorldPosition;
        private Vector3 _gatherPosition;

        void Start ()
        {
            _drum = GetComponent<Drum>();
            _drum.OnCommandSequence += DrumOnOnCommandSequence;
            _drum.OnNotePlayed += DrumOnOnNotePlayed;

            _currentEnergy = MaxEnergy;
            _camera = GameObject.FindObjectOfType<Camera>();
        }

        void Update ()
        {
            // Raycast cursor
            RaycastHit hit;
            _cursorIsHittingGround = Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 100f);
            if (_cursorIsHittingGround)
            {
                _cursorWorldPosition = hit.point;
                Cursor.Instance.transform.position = _cursorWorldPosition;
            }
        }

        private void DrumOnOnCommandSequence(Drum.CommandSequence seq)
        {
            _currentEnergy -= DrainEnergyPerСommand;
            if (!_cursorIsHittingGround)
                return;

            _gatherPosition = _cursorWorldPosition;

            var colliders = Physics.OverlapSphere(_gatherPosition, GatherRadius, LayerMask.NameToLayer("Hamster"),
                QueryTriggerInteraction.Ignore);

            foreach (var hamsterCollider in colliders)
            {
                if (!hamsterCollider.CompareTag("Hamster"))
                    continue;

                var hamsterCtrl = hamsterCollider.GetComponent<HamsterController>();
                if (hamsterCtrl != null)
                {
                    hamsterCtrl.SetDestination(_gatherPosition);
                }
            }
        }

        private void DrumOnOnNotePlayed(Drum.Note note)
        {
            if (note.Type != Drum.NoteType.SequenceEnd)
            {
                _currentEnergy -= DrainEnergyPerNote;
            }
        }

        void OnDrawGizmos()
        {
            if (_cursorIsHittingGround)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_cursorWorldPosition, GatherRadius);
                Gizmos.DrawSphere(_cursorWorldPosition, 0.5f);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_gatherPosition, GatherRadius);
        }
    }
}
