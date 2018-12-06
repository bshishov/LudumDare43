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
        private DrumController _drumController;

        void Awake()
        {
            _initialSize = FillTransform.sizeDelta;
            Value = Initial;
            _localPos = transform.localPosition;

            _drumController = FindObjectOfType<DrumController>();
            if (_drumController == null)
            {
                Debug.LogWarning("[UIProgressBar] No drum controller found in the scene. Disabling progress bar object");
                this.gameObject.SetActive(false);
                return;
            }
        }

        void Update()
        {
            if (_drumController != null)
                _target = _drumController.EnergyFraction;

            Value = Mathf.SmoothDamp(_value, _target, ref _velocity, ChangeTime);

            transform.localPosition =
                _localPos + new Vector3(_velocity * Random.Range(-Amplitude, Amplitude), _velocity * Random.Range(-Amplitude, Amplitude), 0);
        }
    }
}
