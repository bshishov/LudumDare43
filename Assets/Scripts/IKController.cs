using UnityEngine;

namespace Assets.Scripts
{
    public class IKController : MonoBehaviour
    {
        [Header("IK")]
        public bool IkActive = false;
        public Transform LookObj = null;
        [Range(0f, 1f)] public float LookWeight = 0.5f;
        [Range(0f, 1f)] public float LeftHandWeight = 0.5f;
        [Range(0f, 1f)] public float RightHandWeight = 0.5f;
        public Vector3 LeftHandTargetPosition;
        public Vector3 RightHandTargetPosition;

        private Animator _animator;

        void Start()
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


                    // Right hand
                    _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandWeight);
                    _animator.SetIKPosition(AvatarIKGoal.RightHand, RightHandTargetPosition);

                    // Left hand
                    _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, LeftHandWeight);
                    _animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandTargetPosition);
                }

                //if the IK is not active, set the position and rotation of the hand and head back to the original position
                else
                {
                    _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                    _animator.SetLookAtWeight(0);
                }
            }
        }
    }
}
