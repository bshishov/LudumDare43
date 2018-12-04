using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class DrumDetector : MonoBehaviour
    {
        public Drum.CommandSequence Sequence;
        [Range(0f, 5f)]
        public float Radius = 3f;
        public bool Listen = false;
        public float MinBpm = 100f;

        [Header("Activation")]
        public ActivatorProxy[] Targets;

        [Header("FX")]
        public Flame FlameFx;
        public Color ActivatedColor = Color.blue;
        public Color UnactivatedColor = Color.black;
        public Color NoteHitColorA = Color.red;
        public Color NoteHitColorB = Color.magenta;
        [Range(0, 1)] public float NoteImpact = 0.5f;
        [Range(0, 1)] public float SuccessImpact = 0.5f;

        private bool _isActivated;
        private Drum _drum;

        void Start ()
        {
            _drum = FindObjectOfType<Drum>();
            if(_drum == null)
                Debug.LogError("[DRUM DETECTOR] Can't find drum!");
            else
                _drum.OnNotePlayed += DrumOnOnNotePlayed;

            if (FlameFx != null)
                FlameFx.BaseColor = UnactivatedColor;
        }

        private void DrumOnOnNotePlayed(Drum.Note note)
        {
            var distanceToCursor = Vector3.Distance(transform.position, Cursor.Instance.transform.position);
            if (!_isActivated && Listen && distanceToCursor < Radius)
            {
                ReactToNote(note);
                // If sequence match
                if (_drum.CommandSequenceAtTheEnd(Sequence) && _drum.Bpm > MinBpm)
                {
                    ReactToSuccessfulCommand();
                    _isActivated = true;
                    SendActivationEvents();
                }
            }
        }

        void Update ()
        {
        }

        void SendActivationEvents()
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

        void SendDeactivationEvents()
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

        /// <summary>
        /// FX
        /// </summary>
        void ReactToNote(Drum.Note note)
        {
            if (FlameFx != null)
            {
                if(note.Type == Drum.NoteType.A)
                    FlameFx.AddTrauma(NoteImpact, NoteHitColorA);

                if (note.Type == Drum.NoteType.B)
                    FlameFx.AddTrauma(NoteImpact, NoteHitColorB);
            }
        }

        /// <summary>
        /// FX
        /// </summary>
        void ReactToSuccessfulCommand()
        {
            if (FlameFx != null)
            {
                FlameFx.BaseColor = ActivatedColor;
                FlameFx.AddTrauma(SuccessImpact, ActivatedColor);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);

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
