using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(ParticleSystem))]
    public class Flame : MonoBehaviour
    {
        [Header("Dynamics")]
        [Range(0.001f, 0.2f)]
        public float TraumaDecay = 0.01f;
        public float ScaleMod = 1f;
        public float SpeedMod = 1f;
        public Color BaseColor = Color.black;
        public bool PlayOnStart = true;

        private float _trauma = 0f;
        private ParticleSystem _particleSystem;
        private ParticleSystem.MainModule _particles;
        private Color _targetColor;

        void Start ()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            if (_particleSystem != null)
                _particles = _particleSystem.main;
            
            _particles.startColor = BaseColor;
            _particles.playOnAwake = PlayOnStart;
            _targetColor = BaseColor;
        }

        void Update ()
        {
            // Impact Effects
            _trauma = Mathf.Clamp01(_trauma - TraumaDecay);
            var mod = Mathf.Pow(_trauma, 3);

            _particles.startSizeMultiplier = 1 + mod * ScaleMod;
            _particles.startSpeedMultiplier = 1 + mod * SpeedMod;
            _particles.startColor = Color.Lerp(BaseColor, _targetColor, mod);
        }

        public void AddTrauma(float amount)
        {
            _trauma += amount;
        }

        public void AddTrauma(float amount, Color targetColor)
        {
            _trauma += amount;
            _targetColor = targetColor;
        }

        public void StopEmission()
        {
            _particleSystem.Stop();
        }

        public void StartEmission()
        {
            _particleSystem.Play();
        }
    }
}
