using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class DrumDetector : MonoBehaviour
    {
        public Drum.CommandSequence Sequence;  
        public float MinBpm = 100f;
        public bool HideCursor = false;
        public bool OneTime = true;

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
        public AudioClipWithVolume SuccessPatternSound;

        private bool _isActivated;
        private Drum _drum;
        private bool _cursorEntered;

        void Start ()
        {
            _drum = FindObjectOfType<Drum>();
            if(_drum == null)
                Debug.LogError("[DRUM DETECTOR] Can't find drum!");

            if (FlameFx != null)
                FlameFx.BaseColor = UnactivatedColor;
        }

        private void DrumOnOnNotePlayed(Drum.Note note)
        {
            if (_cursorEntered && !_isActivated)
            {
                ReactToNote(note);
                // If sequence match
                if (_drum.CommandSequenceAtTheEnd(Sequence) && _drum.Bpm > MinBpm)
                {
                    Debug.Log(string.Format("[DRUM DETECTOR] [{0}] Success!", gameObject.name));
                    ReactToSuccessfulCommand();

                    if(OneTime)
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

        public void OnCursorEnter()
        {
            _cursorEntered = true;
            _drum.OnNotePlayed += DrumOnOnNotePlayed;

            if(HideCursor)
                Cursor.Instance.Show();
        }

        public void OnCursorLeave()
        {
            _cursorEntered = false;
            _drum.OnNotePlayed -= DrumOnOnNotePlayed;

            if (HideCursor)
                Cursor.Instance.Hide();
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

            SoundManager.Instance.Play(SuccessPatternSound);
        }

        void OnDrawGizmosSelected()
        {
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
