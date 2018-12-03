﻿using Assets.Scripts.Utils;
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

        // Sound settings
       // private readonly float[] _pitchesA = new float[3] { 1f - PitchShift, 1f, 1 + PitchShift };
       // private readonly float[] _pitchesB = new float[4] { 1f - PitchShift, 1f, 1 + PitchShift, 1f };

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
        }

        void Update ()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _aHits = (_aHits + 1) % _pitchesA.Length;
                SoundManager.Instance.Play(SoundA, pitch: _pitchesA[_aHits]);
		// SoundManager.Instance.Play(SoundA, pitch: Random.Range(0.85f, 1.15f)); 
                _drum.PlayNote(Drum.NoteType.A);
            }

            if (Input.GetMouseButtonDown(1))
            {
                _bHits = (_bHits + 1) % _pitchesB.Length;
                SoundManager.Instance.Play(SoundB, pitch: _pitchesB[_bHits]);
		// SoundManager.Instance.Play(SoundB, pitch: Random.Range(0.85f, 1.15f));
                _drum.PlayNote(Drum.NoteType.B);
            }

            if (Input.GetMouseButtonDown(2))
                _drum.PlayNote(Drum.NoteType.SequenceEnd);
        }

        private void DrumOnOnCommandSequence(Drum.CommandSequence seq)
        {
            DecreaseEnergy(DrainEnergyPerСommand);
            if (!Cursor.Instance.IsHittingGround)
                return;

            if (Cursor.Instance.IsInDrumArea)
                return;

            if (seq.Name.Equals("Move"))
            {
                foreach (var hamster in Cursor.Instance.FindHamsters())
                {
                    hamster.SetDestination(Cursor.Instance.transform.position, _drum.Bpm);
                }
            }

            if (seq.Name.Equals("Sacrifice"))
            {
                var nearestHamster = Cursor.Instance.FindNearest();
                if (nearestHamster != null)
                {
                    nearestHamster.SetDestination(Cursor.Instance.transform.position, _drum.Bpm);
                }
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

                DecreaseEnergy(DrainEnergyPerNote);
            }
        }

        void OnDrawGizmos()
        {
            // Soul spawner point
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.TransformPoint(SoulSpawnerPoint), 0.1f);
        }

        void OnGUI()
        {
            GUI.Box(new Rect(180, 0, 200, 200), GUIContent.none);
            GUI.Label(new Rect(200, 10, 200, 50), string.Format("Current Energy: {0}", _currentEnergy));
            if (GUI.Button(new Rect(200, 30, 50, 50), "+"))
                IncreaseEnergy(100f);

            if (GUI.Button(new Rect(260, 30, 50, 50), "-"))
                DecreaseEnergy(100f);
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
