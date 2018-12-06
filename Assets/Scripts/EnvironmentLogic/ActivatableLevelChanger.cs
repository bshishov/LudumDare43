using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.EnvironmentLogic
{
    [RequireComponent(typeof(ActivatorProxy))]
    public class ActivatableLevelChanger : MonoBehaviour
    {
        public string LevelName;

        private bool _levelLoadRequested = false;

        void OnProxyActivate()
        {
            if(_levelLoadRequested)
                return;

            if (string.IsNullOrEmpty(LevelName))
            {
                Debug.LogError("[ActivatableLevelChanger] LevelName is not set");
                return;
            }

            var lvlManager = LevelManager.Instance;
            if (lvlManager != null)
            {
                LevelManager.Instance.LoadLevel(LevelName);
            }
            else
            {
                SceneManager.LoadScene(LevelName);
            }

            _levelLoadRequested = true;
        }
    }
}
