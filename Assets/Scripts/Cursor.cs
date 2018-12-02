using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cursor : Singleton<Cursor>
    {
        public bool IsHittingGround
        {
            get { return _cursorIsHittingGround; }
        }

        [Header("Mechanics")]
        [Range(0.5f, 20f)]
        public float GatherRadius = 10f;

        [Header("Visuals")]
        public MeshRenderer Renderer;
        public ParticleSystem Particles;

        [Range(0.001f, 0.2f)] public float TraumaDecay = 0.01f;
        public float ScaleMod = 1f;
        public float SpeedMod = 1f;
        public Color BaseColor = Color.black;
        public Color ColorA = Color.red;
        public Color ColorB = Color.magenta;
        [Range(0f, 1f)] public float ImpactA = 0.3f;
        [Range(0f, 1f)] public float ImpactB = 0.1f;

        private Drum _drum;
        private float _noteActivity = 0f;
        private Color _targetColor;
        private float _trauma;
        private Vector3 _initialScale;
        ParticleSystem.MainModule _particles;
        private bool _cursorIsHittingGround;
        private Vector3 _mouseWorldPosition;
        private Camera _camera;


        void Start()
        {
            _drum = FindObjectOfType<Drum>();
            _drum.OnNotePlayed += DrumOnOnNotePlayed;
            _initialScale = transform.localScale;

            if (Particles != null)
                _particles = Particles.main;

            _camera = GameObject.FindObjectOfType<Camera>();
        }

        void Update()
        {
            RaycastHit hit;
            _cursorIsHittingGround = Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out hit, 100f,
                LayerMask.GetMask("Environment"));

            if (_cursorIsHittingGround)
            {
                _mouseWorldPosition = hit.point;
                SetDesiredPosition(_mouseWorldPosition);
            }

            // Impact Effects
            _trauma = Mathf.Clamp01(_trauma - TraumaDecay);
            _noteActivity = Mathf.Pow(_trauma, 3);

            if (Renderer != null)
                Renderer.material.color = Color.Lerp(Color.white, _targetColor, _noteActivity);

            transform.localScale = _initialScale + Vector3.one * _noteActivity;

            if (Particles != null)
            {
                _particles.startSizeMultiplier = 1 + _noteActivity * ScaleMod;
                _particles.startSpeedMultiplier = 1 + _noteActivity * SpeedMod;
                _particles.startColor = Color.Lerp(BaseColor, _targetColor, _noteActivity);
            }
        }

        private void DrumOnOnNotePlayed(Drum.Note note)
        {
            if (note.Type == Drum.NoteType.A)
            {
                AddTrauma(ImpactA);
                _targetColor = ColorA;
            }

            if (note.Type == Drum.NoteType.B)
            {
                AddTrauma(ImpactB);
                _targetColor = ColorB;
            }
        }

        private void AddTrauma(float trauma)
        {
            _trauma += trauma;
        }

        public void SetDesiredPosition(Vector3 targetPosition)
        {
            transform.position = targetPosition;
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_mouseWorldPosition, 0.5f);
            Gizmos.DrawWireSphere(transform.position, GatherRadius);
        }

        public IEnumerable<Collider> FindNearObjectsWithTag(string requestTag, float radiusMul = 1f)
        {
            return Physics.OverlapSphere(_mouseWorldPosition, GatherRadius * radiusMul)
                .Where(c => c.CompareTag(requestTag));
        }

        public IEnumerable<HamsterController> FindHamsters(float radiusMul = 1f)
        {
            return FindNearObjectsWithTag("Hamster", radiusMul)
                .Select(c => c.GetComponent<HamsterController>());
        }
    }
}
