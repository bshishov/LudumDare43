using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class ActivatableSound : MonoBehaviour
    {
        public bool AttachToObject = true;
        public AudioClipWithVolume ActivateSound;
        public AudioClipWithVolume DeactivateSound;

        void OnProxyActivate()
        {
            var sh = SoundManager.Instance.Play(ActivateSound);
            if(AttachToObject)
                sh.AttachToObject(transform);
        }

        void OnProxyDeactivate()
        {
            var sh = SoundManager.Instance.Play(DeactivateSound);
            if (AttachToObject)
                sh.AttachToObject(transform);
        }
    }
}
