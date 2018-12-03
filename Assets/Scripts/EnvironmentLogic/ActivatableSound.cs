using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    public class ActivatableSound : MonoBehaviour
    {
        public AudioClipWithVolume ActivateSound;
        public AudioClipWithVolume DeactivateSound;

        void OnProxyActivate()
        {
            SoundManager.Instance.Play(ActivateSound);
        }

        void OnProxyDeactivate()
        {
            SoundManager.Instance.Play(DeactivateSound);
        }
    }
}
