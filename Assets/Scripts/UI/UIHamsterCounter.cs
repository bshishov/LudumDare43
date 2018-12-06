using Assets.Scripts.EnvironmentLogic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIHamsterCounter : MonoBehaviour
    {
        public Text CounterValueText;
        public UIShaker Shaker;

        private AscendAltar _altar;
        private int _lastAscended = 0;

        void Start ()
        {
            _altar = FindObjectOfType<AscendAltar>();

            // If no altar in scene
            if (_altar == null)
            {
                Debug.LogWarning("[UIHamsterCounter] Can't find AscendAltar in scene, UI counter will be disabled");
                this.gameObject.SetActive(false);
                return;
            }

            if (CounterValueText == null)
            {
                Debug.LogWarning("[UIHamsterCounter] No text component is set for value");
                this.gameObject.SetActive(false);
                return;
            }
            
            CounterValueText.text = string.Format("{0}/{1}", _altar.Ascended, _altar.Required);
        }
        
        void Update ()
        {
            // If value has changed
            if (_lastAscended != _altar.Ascended)
            {
                // Update internal value
                _lastAscended = _altar.Ascended;

                // Update text
                CounterValueText.text = string.Format("{0}/{1}", _altar.Ascended, _altar.Required);

                // Shake
                if(Shaker != null)
                    Shaker.Shake();
            }
        }
    }
}
