using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    [RequireComponent(typeof(DrumArea))]
    public class DrumDetector : MonoBehaviour
    {
        [Header("General")]
        public Drum.CommandSequence Sequence;
        [Range(0f, 1000f)]
        public float MinBpm = 100f;
        public bool OneTime = true;

        [Header("Activation")]
        public ActivatorProxy[] Targets;

        [Header("FX")]
        public Flame FlameFx;
        public Color ActiveCursorColor = Color.white;
        public Color ActivatedColor = Color.blue;
        public Color UnactivatedColor = Color.black;
        public Color NoteHitColorA = Color.red;
        public Color NoteHitColorB = Color.magenta;
        [Range(0, 1)] public float NoteImpact = 0.5f;
        [Range(0, 1)] public float SuccessImpact = 0.5f;
        public Sound SuccessPatternSound;

        private DrumArea _drumArea;
        private bool _isActivated;
        private Drum _drum;

        void Start ()
        {
            _drumArea = GetComponent<DrumArea>();
            _drumArea.CursorEnter += OnCursorEnter;
            _drumArea.CursorExit += OnCursorExit;

            if (FlameFx != null)
                FlameFx.BaseColor = UnactivatedColor;
        }

        private void DrumOnOnNotePlayed(Drum.Note note)
        {
            if (_drumArea.IsCursorInArea && !_isActivated)
            {
                ReactToNote(note);
                // If sequence match
                if (_drum.CommandSequenceAtTheEnd(Sequence) && _drum.Bpm > MinBpm)
                {
                    Debug.Log(string.Format("[DRUM DETECTOR] [{0}] Success!", gameObject.name));
                    ReactToSuccessfulCommand();

                    if(OneTime)
                        _isActivated = true;

                    if (Targets != null)
                        foreach (var target in Targets)
                            if (target != null)
                                target.Activate();
                }
            }
        }

        public void OnCursorEnter()
        {
            if (_drum == null)
            {
                _drum = FindObjectOfType<Drum>();
                if (_drum == null)
                    Debug.LogError("[DRUM DETECTOR] Can't find drum!");
            }
            
            _drum.OnNotePlayed += DrumOnOnNotePlayed;

            if (FlameFx != null && !_isActivated)
                FlameFx.BaseColor = ActiveCursorColor;
        }

        public void OnCursorExit()
        {
            _drum.OnNotePlayed -= DrumOnOnNotePlayed;

            if (FlameFx != null)
            {
                if (!_isActivated)
                    FlameFx.BaseColor = UnactivatedColor;
                else
                    FlameFx.BaseColor = ActivatedColor;
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

            SoundManager.Instance.Play(SuccessPatternSound, transform);
        }

        void OnDrawGizmosSelected()
        {
            // Activatable targets visualization
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
