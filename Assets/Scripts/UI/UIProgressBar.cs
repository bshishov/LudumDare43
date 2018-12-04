using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIProgressBar : MonoBehaviour
    {
        public RectTransform FillTransform;

        public float Value
        {
            get { return _value; }
            set
            {
                if (FillTransform != null)
                {
                    _value = Mathf.Clamp01(value);
                    FillTransform.sizeDelta = new Vector2(_initialSize.x * _value, _initialSize.y);
                }
            }
        }
        public float Initial = 1f;
        public float ChangeTime = 0.5f;
        public float Amplitude = 2f;


        private float _value;
        private float _target;
        private Vector2 _initialSize;
        private float _velocity;
        private Vector3 _localPos;

        void Awake()
        {
            _initialSize = FillTransform.sizeDelta;
            Value = Initial;
            _localPos = transform.localPosition;
        }

        void Update()
        {
            var dc = DrumController.Instance;
            if (dc != null)
                _target = dc.EnergyFraction;

            Value = Mathf.SmoothDamp(_value, _target, ref _velocity, ChangeTime);

            transform.localPosition =
                _localPos + new Vector3(_velocity * Random.Range(-Amplitude, Amplitude), _velocity * Random.Range(-Amplitude, Amplitude), 0);
        }
    }
}
