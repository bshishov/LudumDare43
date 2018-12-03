using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class DrumDetector : MonoBehaviour
    {
        public Drum.CommandSequence Sequence;
        [Range(0f, 5f)]
        public float Radius = 3f;
        public bool Listen = false;

        [Header("Activation")]
        public ActivatorProxy[] Targets;

        [Header("FX")]
        public GameObject Particles;
        public MeshRenderer Renderer;
        public Flame FlameFx;
        public Color UnactivatedColor = Color.black;
        public Color NoteHitColorA = Color.red;
        public Color NoteHitColorB = Color.magenta;

        private bool _isActivated;
        private Drum _drum;

        void Start ()
        {
            _drum = FindObjectOfType<Drum>();
            if(_drum == null)
                Debug.LogError("[DRUM DETECTOR] Can't find drum!");
            else
                _drum.OnNotePlayed += DrumOnOnNotePlayed;
        }

        private void DrumOnOnNotePlayed(Drum.Note note)
        {
            if (!_isActivated && Listen)
            {
                ReactToNote();
                // If sequence match
                if (_drum.CommandSequenceAtTheEnd(Sequence))
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
        void ReactToNote()
        {
            if (FlameFx != null)
            {

            }
        }

        /// <summary>
        /// FX
        /// </summary>
        void ReactToSuccessfulCommand()
        {
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
