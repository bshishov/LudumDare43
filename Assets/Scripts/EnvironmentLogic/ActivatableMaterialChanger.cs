using System;
using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    [RequireComponent(typeof(ActivatorProxy))]
    public class ActivatableMaterialChanger : MonoBehaviour, IActivatable
    {
        [Serializable]
        public class ColorPropertyState
        {
            public string Key = "_Color";
            public Color Color;
            public AnimationCurve ChangeCurve = AnimationCurve.Linear(0, 0, 1, 1);

            [NonSerialized]
            public Color InitialValue;
        }

        [Serializable]
        public class FloatPropertyState
        {
            public string Key = "_SomeProperty";
            public float Value;
            public AnimationCurve ChangeCurve = AnimationCurve.Linear(0, 0, 1, 1);

            [NonSerialized]
            public float InitialValue;
        }

        public Renderer TargetRenderer;
        public ColorPropertyState[] ActivatedColors;
        public FloatPropertyState[] ActivatedFloatValues;

        [Range(0, 10f)]
        public float TransitionTime = 1f;

        private Material _targetMaterial;
        private float _state;
        private float _targetState;
        private float _velocity;

        void Start ()
        {
            if (TargetRenderer == null)
                TargetRenderer = GetComponent<Renderer>();

            _targetMaterial = TargetRenderer.material;
            _state = 0f;
            _targetState = 0f;

            if(ActivatedColors != null)
                foreach (var colorPropertyState in ActivatedColors)
                    colorPropertyState.InitialValue = _targetMaterial.GetColor(colorPropertyState.Key);

            if (ActivatedFloatValues != null)
                foreach (var floatValue in ActivatedFloatValues)
                    floatValue.InitialValue = _targetMaterial.GetFloat(floatValue.Key);
        }
	
        void Update ()
        {
            _state = Mathf.SmoothDamp(_state, _targetState, ref _velocity, TransitionTime);

            if (ActivatedColors != null)
            {
                foreach (var prop in ActivatedColors)
                    _targetMaterial.SetColor(prop.Key, Color.Lerp(
                        prop.InitialValue, 
                        prop.Color, 
                        prop.ChangeCurve.Evaluate(_state)));
            }

            if (ActivatedFloatValues != null)
            {
                foreach (var prop in ActivatedFloatValues)
                    _targetMaterial.SetFloat(prop.Key, Mathf.Lerp(
                        prop.InitialValue,
                        prop.Value,
                        prop.ChangeCurve.Evaluate(_state)));
            }
        }

        public void OnProxyActivate()
        {
            _targetState = 1f;
        }

        public void OnProxyDeactivate()
        {
            _targetState = 0f;
        }
    }
}
