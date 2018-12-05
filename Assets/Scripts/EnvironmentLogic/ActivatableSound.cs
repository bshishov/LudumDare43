using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class ActivatableSound : MonoBehaviour
    {
        public bool AttachToObject = true;
        public Sound ActivateSound;
        public Sound DeactivateSound;

        void OnProxyActivate()
        {
            var sh = SoundManager.Instance.Play(ActivateSound);
            if(AttachToObject && sh != null)
                sh.AttachToObject(transform);
        }

        void OnProxyDeactivate()
        {
            var sh = SoundManager.Instance.Play(DeactivateSound);
            if (AttachToObject && sh != null)
                sh.AttachToObject(transform);
        }
    }
}
