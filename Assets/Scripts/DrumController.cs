using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Drum))]
    public class DrumController : MonoBehaviour
    {
        [Header("Drum")]
        public float MaxEnergy = 1000f;
        public float DrainEnergyPerNote = 1f;
        public float DrainEnergyPerСommand = 2f;

        [Header("Soul")]
        public GameObject SoulPrefab;
        public Vector3 SoulSpawnerPoint = Vector3.zero;

        [Header("Audio")]
        public AudioClipWithVolume SoundA;
        public AudioClipWithVolume SoundB;

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

        private const float PitchShift = 0.2f;
        private float _currentEnergy;
        private Drum _drum;
        private Camera _camera;

        // Sound settings
        private readonly float[] _pitchesA = new float[3] { 1f - PitchShift, 1f, 1 + PitchShift };
        private readonly float[] _pitchesB = new float[4] { 1f - PitchShift, 1f, 1 + PitchShift, 1f };
        private int _aHits = 0;
        private int _bHits = 0;

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
            if (Input.GetMouseButtonDown(0))
            {
                _aHits = (_aHits + 1) % _pitchesA.Length;
                SoundManager.Instance.Play(SoundA, pitch: _pitchesA[_aHits]);
                _drum.PlayNote(Drum.NoteType.A);
            }

            if (Input.GetMouseButtonDown(1))
            {
                _bHits = (_bHits + 1) % _pitchesB.Length;
                SoundManager.Instance.Play(SoundB, pitch: _pitchesB[_bHits]);
                _drum.PlayNote(Drum.NoteType.B);
            }

            if (Input.GetMouseButtonDown(2))
                _drum.PlayNote(Drum.NoteType.SequenceEnd);
        }

        private void DrumOnOnCommandSequence(Drum.CommandSequence seq)
        {
            _currentEnergy -= DrainEnergyPerСommand;
            if (!Cursor.Instance.IsHittingGround)
                return;
            
            foreach (var hamster in Cursor.Instance.FindHamsters())
            {
                hamster.SetDestination(Cursor.Instance.transform.position);
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
                    var go = Instantiate(SoulPrefab, transform.TransformPoint(SoulSpawnerPoint), Quaternion.identity);
                    var soul = go.GetComponent<Soul>();
                    soul.GoToTarget(Cursor.Instance.transform);
                }

                _currentEnergy -= DrainEnergyPerNote;
            }
        }

        void OnDrawGizmos()
        {
            // Soul spawner point
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.TransformPoint(SoulSpawnerPoint), 0.1f);
        }
    }
}
