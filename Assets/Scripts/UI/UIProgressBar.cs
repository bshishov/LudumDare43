using UnityEngine;

namespace Assets.Scripts.UI
{
    public class UIProgressBar : MonoBehaviour
    {
        public RectTransform FillTransform;
        public UIShaker Shaker;

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

        private float _value;
        private float _target;
        private Vector2 _initialSize;
        private float _velocity;
        private DrumController _drumController;

        void Start()
        {
            _initialSize = FillTransform.sizeDelta;
            Value = Initial;

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

            if(Shaker != null)
                Shaker.Shake(Mathf.Abs(_velocity));
        }
    }
}
