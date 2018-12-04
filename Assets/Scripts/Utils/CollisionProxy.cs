using UnityEngine;

namespace Assets.Scripts.Utils
{
    [RequireComponent(typeof(Rigidbody))]
    public class CollisionProxy : MonoBehaviour
    {
        public GameObject Target;

        void OnTriggerEnter(Collider col)
        {
            if(Target != null)
                Target.SendMessage("OnTriggerEnter", col);
        }
    }
}
