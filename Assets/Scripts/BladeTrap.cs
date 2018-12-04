using UnityEngine;

namespace Assets.Scripts
{
    public class BladeTrap : MonoBehaviour
    {
        public float RotationSpeed;

        private Rigidbody _rigidbody;

        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
	
        // Update is called once per frame
        void Update ()
        {
            _rigidbody.AddTorque(Vector3.up * RotationSpeed * Time.deltaTime);
            //transform.Rotate(Vector3.up, );
        }
    }
}
