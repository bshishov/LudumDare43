using UnityEngine;

namespace Assets.Scripts
{
    public class RagdollController : MonoBehaviour
    {
        [Header("IK")]
        public bool IkActive = false;
        public Transform LookObj = null;
        [Range(0f, 1f)]
        public float LookWeight = 0.5f;

        private Animator _animator;

        void Start ()
        {
            _animator = GetComponent<Animator>();
        }
        
        void OnAnimatorIK()
        {
            if (_animator)
            {

                //if the IK is active, set the position and rotation directly to the goal. 
                if (IkActive)
                {

                    // Set the look target position, if one has been assigned
                    if (LookObj != null)
                    {
                        _animator.SetLookAtWeight(LookWeight);
                        _animator.SetLookAtPosition(LookObj.position);
                    }

                    
                    // Set the right hand target position and rotation, if one has been assigned
                    if (LookObj != null)
                    {
                        _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, LookWeight);
                        _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, LookWeight);
                        _animator.SetIKPosition(AvatarIKGoal.RightHand, LookObj.position);
                        _animator.SetIKRotation(AvatarIKGoal.RightHand, LookObj.rotation);
                    }
                    

                }

                //if the IK is not active, set the position and rotation of the hand and head back to the original position
                else
                {
                    _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    _animator.SetLookAtWeight(0);
                }
            }
        }
    }
}
