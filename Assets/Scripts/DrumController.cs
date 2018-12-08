using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Drum))]
    public class DrumController : Singleton<DrumController>
    {
        [Header("Drum")]
        public float MaxEnergy = 1000f;
        public float DrainEnergyPerNote = 1f;
        public float DrainEnergyPerСommand = 2f;
        public Transform DrumMeshTransform;

        public Vector3 DrumHitCenter = Vector3.zero;
        public Vector3 LeftHandSourcePosition = Vector3.zero;
        public Vector3 RightHandSourcePosition = Vector3.zero;

        [Header("Soul")]
        public GameObject SoulPrefab;

        [Header("Audio")]
        public Sound SoundA;
        public Sound SoundB;

        [Header("Camera")]
        [Range(0f, 1f)]
        public float CameraShakePerHitA = 0.2f;
        [Range(0f, 1f)]
        public float CameraShakePerHitB = 0.2f;

        public float CurrentEnergy
        {
            get { return _currentEnergy; }
        }

        public float EnergyFraction
        {
            get { return _currentEnergy / MaxEnergy; }
        }

        public bool EnoughEnergy
        {
            get { return _currentEnergy > 10f; }
        }

        /// <summary>
        /// Returns a coefficient for controlling movement and other things in range [0, 1]
        /// Linear!
        /// </summary>
        public float BpmEnergyModifier
        {
            get
            {
                if(EnoughEnergy)
                    return (_drum.Bpm - Drum.MinBpm) / (Drum.MaxBpm - Drum.MinBpm);

                return 0f;
            }
        }

        private const float PitchShift = 0.2f;
        private float _currentEnergy;
        private Drum _drum;

        // Sound settings (melody)
	    private readonly float[] _pitchesA = new float[8] { 1f, 
							    1f - PitchShift, 1f + PitchShift,  
							    1f - PitchShift, 1f + PitchShift,
							    1f - PitchShift, 1f + PitchShift, 
							    1f };

        private readonly float[] _pitchesB = new float[8] { 1f ,
							    1f, 1f + 2*PitchShift, 
							    1f, 1f + 2*PitchShift, 
							    1f, 1f + 2*PitchShift,
							    1f };
        private int _aHits = 0;
        private int _bHits = 0;

        void Start ()
        {
            _drum = GetComponent<Drum>();
            _drum.OnCommandSequence += DrumOnOnCommandSequence;
            _drum.OnNotePlayed += DrumOnOnNotePlayed;

            _currentEnergy = MaxEnergy;

            if (DrumMeshTransform == null)
                DrumMeshTransform = transform;
        }

        void Update ()
        {
            if (Input.GetMouseButtonDown(Common.Controls.LeftMouseButton))
            {
                _aHits = (_aHits + 1) % _pitchesA.Length;
                var s = SoundManager.Instance.Play(SoundA);
                if (s != null)
                {
                    s.Pitch = _pitchesA[_aHits];
                    s.AttachToObject(transform);
                }

                _drum.PlayNote(Drum.NoteType.A);
            }

            if (Input.GetMouseButtonDown(Common.Controls.RightMouseButton))
            {
                _bHits = (_bHits + 1) % _pitchesB.Length;

                var s = SoundManager.Instance.Play(SoundB);
                if (s != null)
                {
                    s.Pitch = _pitchesB[_bHits];
                    s.AttachToObject(transform);
                }

                _drum.PlayNote(Drum.NoteType.B);
            }

            if (Input.GetMouseButtonDown(Common.Controls.MiddleMouseButton))
                _drum.PlayNote(Drum.NoteType.SequenceEnd);
        }

        private void DrumOnOnCommandSequence(Drum.CommandSequence seq)
        {
            DecreaseEnergy(DrainEnergyPerСommand);
            if (!Cursor.Instance.IsHittingGround)
                return;

            if (Cursor.Instance.IsInDrumArea)
                return;

            if (seq.Name.Equals(Common.Patterns.Move))
            {
                foreach (var hamster in Cursor.Instance.FindHamsters())
                {
                    hamster.SetDestination(Cursor.Instance.transform.position, _drum.Bpm);
                }
            }

            if (seq.Name.Equals(Common.Patterns.MoveNearest))
            {
                var nearestHamster = Cursor.Instance.FindNearestHamster();
                if (nearestHamster != null)
                {
                    nearestHamster.SetDestination(Cursor.Instance.transform.position, _drum.Bpm);
                }
            }

            if (seq.Name.Equals(Common.Patterns.Restart))
            {
                DrummerController.Instance.Die();
            }
        }

        private void DrumOnOnNotePlayed(Drum.Note note)
        {
            if (note.Type != Drum.NoteType.SequenceEnd)
            {
                if(note.Type == Drum.NoteType.A)
                    CameraController.Instance.Shake(CameraShakePerHitA);

                if (note.Type == Drum.NoteType.B)
                    CameraController.Instance.Shake(CameraShakePerHitB);

                if (SoulPrefab != null)
                {
                    var go = Instantiate(SoulPrefab, DrumMeshTransform.TransformPoint(DrumHitCenter), Quaternion.identity);
                    var soul = go.GetComponent<Soul>();
                    soul.GoToTarget(Cursor.Instance.transform, note.Type);
                }

                DecreaseEnergy(DrainEnergyPerNote);
            }
        }

        void OnDrawGizmos()
        {
            // Soul spawner point
            Gizmos.color = Color.blue;

            var t = DrumMeshTransform;
            if (t == null)
                t = transform;

            var c = t.TransformPoint(DrumHitCenter);
            var lh = t.TransformPoint(LeftHandSourcePosition);
            var rh = t.TransformPoint(RightHandSourcePosition);

            Gizmos.DrawSphere(c, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lh, 0.1f);
            Gizmos.DrawSphere(rh, 0.1f);

            Gizmos.DrawLine(c, lh);
            Gizmos.DrawLine(c, rh);
        }

        void OnGUI()
        {
            /*
            GUI.Box(new Rect(180, 0, 200, 200), GUIContent.none);
            GUI.Label(new Rect(200, 10, 200, 50), string.Format("Current Energy: {0}", _currentEnergy));
            if (GUI.Button(new Rect(200, 30, 50, 50), "+"))
                IncreaseEnergy(100f);

            if (GUI.Button(new Rect(260, 30, 50, 50), "-"))
                DecreaseEnergy(100f);*/
        }

        public void DecreaseEnergy(float amount)
        {
            _currentEnergy = Mathf.Clamp(_currentEnergy - amount, 0, MaxEnergy);
        }

        public void IncreaseEnergy(float amount)
        {
            _currentEnergy = Mathf.Clamp(_currentEnergy + amount, 0, MaxEnergy);
        }
    }
}
