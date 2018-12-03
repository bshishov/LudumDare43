using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts
{
    public class RagdollController : MonoBehaviour
    {
        [Header("General")]
        public bool LookForChildRagdollComponentsOnStart = true;
        public bool RagdollEnabledOnStart = false;
        public Collider DefaultCollider;
        public Rigidbody DefaultBody;
        public float YFix = 0.5f;

        [Header("Cheats")]
        public bool EnableCheats = true;

        private Animator _animator;
        private NavMeshAgent _agent;
        private HamsterController _hamsterController;
        private CharacterJoint[] _joints;
        private Rigidbody[] _bodies;
        private Collider[] _colliders;

        private Vector3 _velocityHack = Vector3.zero;

        void Start ()
        {
            _animator = GetComponent<Animator>();
            _agent = GetComponent<NavMeshAgent>();
            _hamsterController = GetComponent<HamsterController>();

            if (LookForChildRagdollComponentsOnStart)
            {
                _bodies = GetComponentsInChildren<Rigidbody>().Where(c => !c.gameObject.Equals(gameObject)).ToArray();
                _colliders = GetComponentsInChildren<Collider>().Where(c => !c.gameObject.Equals(gameObject)).ToArray();
            }

            if(RagdollEnabledOnStart)
                EnableRagdoll();
            else
                DisableRagdoll();
        }

        void Update()
        {
            if (EnableCheats)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    //transform.position += new Vector3(0, 2, 0);

                    var rigidBodies = GetComponentsInChildren<Rigidbody>();
                    var cPos = Cursor.Instance.transform.position;

                    foreach (var body in rigidBodies)
                    {
                        body.AddExplosionForce(1000f, cPos + Vector3.down * 3f, 10f);
                    }
                }

                if (Input.GetKeyDown(KeyCode.I))
                    EnableRagdoll();

                if (Input.GetKeyDown(KeyCode.O))
                    DisableRagdoll();
            }
        }

        public void EnableRagdoll()
        {
            if (_hamsterController != null)
                _hamsterController.enabled = false;

            if (_agent != null)
            {
                _velocityHack = _agent.velocity;
                _agent.enabled = false;
            }

            if (_animator != null)
                _animator.enabled = false;

            transform.position += new Vector3(0, YFix, 0);

            foreach (var col in _colliders)
                col.enabled = true;

            if (DefaultCollider != null)
                DefaultCollider.enabled = false;

            foreach (var body in _bodies)
            {
                body.isKinematic = false;
                body.velocity = _velocityHack;


                // TODO: Figure out why adding force is not working :(
                //body.velocity = _agent.velocity;
                //OR
                //body.AddForce(_agent.velocity * 1000f, ForceMode.Impulse);
            }
        }

        public void DisableRagdoll()
        {
            foreach (var col in _colliders)
                col.enabled = false;

            foreach (var body in _bodies)
                body.isKinematic = true;

            if (_animator != null)
                _animator.enabled = true;

            if (_agent != null)
                _agent.enabled = true;

            if (DefaultCollider != null)
                DefaultCollider.enabled = true;

            if (_hamsterController != null)
                _hamsterController.enabled = true;
        }
    }
}
