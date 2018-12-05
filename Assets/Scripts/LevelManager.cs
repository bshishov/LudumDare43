using Assets.Scripts.UI;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class LevelManager : Singleton<LevelManager>
    {
        public UICanvasGroupFader ScreenFader;
        public AudioClipWithVolume Music;
        private string _nextLevelRequest;

        void Start ()
        {
            if (ScreenFader == null)
                ScreenFader = GetComponent<UICanvasGroupFader>();

            ScreenFader.StateChanged += StateChanged;
            ScreenFader.FadeOut();

            SoundManager.Instance.PlayMusic(Music);
        }

        private void StateChanged()
        {
            if (ScreenFader.State == UICanvasGroupFader.FaderState.FadedIn)
            {
                if (!string.IsNullOrEmpty(_nextLevelRequest))
                {
                    SceneManager.LoadScene(_nextLevelRequest);
                    _nextLevelRequest = null;
                }
            }
        }
        
        public void LoadLevel(string levelName)
        {
            _nextLevelRequest = levelName;
            ScreenFader.FadeIn();
        }

        public void Restart()
        {
            LoadLevel(SceneManager.GetActiveScene().name);
        }
    }
}
