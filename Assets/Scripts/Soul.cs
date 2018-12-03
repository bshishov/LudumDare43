﻿using UnityEngine;

namespace Assets.Scripts
{
    public class Soul : MonoBehaviour
    {
        public AnimationCurve MovementCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve HeightCurve = AnimationCurve.Linear(0, 0, 1, 0);
        [Range(0.05f, 2f)]
        public float FlyTime = 1f;
        public float TimeBeforeDestroy = 1f;

        private float _progress = 0f;
        private Transform _target;
        private Vector3 _targetPos;
        private Vector3 _sourcePoint;
        

        void Start ()
        {
            _sourcePoint = transform.position;
            _targetPos = _sourcePoint;
        }
        
        void Update ()
        {
            _progress += Time.deltaTime / FlyTime;
            if (_progress > 1f)
            {
                _progress = 1f;
                Destroy(gameObject, TimeBeforeDestroy);
            }

            if (_target != null)
                _targetPos = _target.position;

            var mt = MovementCurve.Evaluate(_progress);
            var ht = HeightCurve.Evaluate(_progress);

            transform.position = Vector3.Lerp(_sourcePoint, _targetPos, mt) + Vector3.up * ht;
            transform.LookAt(_targetPos);
        }

        public void GoToTarget(Transform target)
        {
            _target = target;
        }

        public void GoToPoint(Vector3 target)
        {
            _targetPos = target;
        }
    }
}