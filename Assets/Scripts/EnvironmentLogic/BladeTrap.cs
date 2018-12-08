using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class BladeTrap : MonoBehaviour
    {
        public Transform TargetTransform;
        public float RotationSpeed;
        public Vector3 Axis = Vector3.up;
        public bool UsePhysics = true;

        private Rigidbody _rigidbody;

        void Start()
        {
            if (TargetTransform == null)
                TargetTransform = transform;

            if(UsePhysics)
                _rigidbody = TargetTransform.GetComponent<Rigidbody>();
        }
        
        void Update ()
        {
            if(UsePhysics)
                _rigidbody.AddTorque(Axis * RotationSpeed * Time.deltaTime);
            else
                transform.Rotate(Axis, RotationSpeed * Time.deltaTime);
        }
    }
}
