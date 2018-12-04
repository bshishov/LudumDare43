using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.EnvironmentLogic
{
    [RequireComponent(typeof(ActivatorProxy))]
    public class AscendAltar : MonoBehaviour
    {
        public int Required = 5;
        public float Decay = 0.1f;
        public float Range = 2f;
        public string NextLevelName;

        private ActivatorProxy _activator;
        private bool _altarActive;
        private float _trauma = 0f;
        private readonly List<Collider> _hamstersInRange = new List<Collider>();
        private int _ascended = 0;

        void Start ()
        {
            _activator = GetComponent<ActivatorProxy>();
            _activator.Activated += ActivatorOnActivated;
        }

        private void ActivatorOnActivated()
        {
            var nearestHamster = Cursor.Instance.FindNearest();
            if (nearestHamster != null)
                nearestHamster.SetDestination(transform.position);

            _trauma += 0.8f;
        }

        IEnumerator AscendAnimation(HamsterController hc)
        {
            yield return new WaitForSeconds(0.1f);
            var rc = hc.GetComponent<HamsterController>();
            rc.enabled = false;

            var ik = hc.GetComponent<IKController>();
            if (ik != null)
            {
                ik.IkActive = true;
                ik.LookObj = Camera.main.transform;
                ik.LookWeight = 1f;
            }
            

            var ascendTo = transform.position + new Vector3(0, 20, 0);

            const int frames = 200;
            // TODO: WHOA SUCH A SPAGHETTI!!
            for (var i = 0; i < frames; i++)
            {
                hc.transform.position = Vector3.Lerp(hc.transform.position, ascendTo, (float)i / frames);
                yield return null;
            }

            Destroy(hc.gameObject);
            yield return null;
        }

        void Update ()
        {
            _trauma = Mathf.Clamp01(_trauma - Decay * Time.deltaTime);
            var altarActive = _trauma > 0.2f;

            if (altarActive)
            {
                if (_hamstersInRange.Count > 0)
                {
                    var processed = new List<Collider>();
                    foreach (var col in _hamstersInRange)
                    {
                        var hc = col.GetComponent<HamsterController>();
                        if (hc != null && Vector3.Distance(col.transform.position, transform.position) < Range)
                        {
                            StartCoroutine(AscendAnimation(hc));
                            processed.Add(col);

                            Debug.Log("[ALTAR] Hamster ascend");
                            _ascended++;
                            if (_ascended >= Required)
                                LoadNextScene();
                        }
                    }

                    foreach (var col in processed)
                        _hamstersInRange.Remove(col);
                }
            }
        }


        void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("Hamster") && !_hamstersInRange.Contains(col))
                _hamstersInRange.Add(col);
        }

        void OnTriggerExit(Collider col)
        {
            if (col.CompareTag("Hamster") && _hamstersInRange.Contains(col))
                _hamstersInRange.Remove(col);
        }

        void LoadNextScene()
        {
            
            // TODO: Load scene properly
            if(!string.IsNullOrEmpty(NextLevelName))
                LevelManager.Instance.LoadLevel(NextLevelName);
            else
                Debug.LogWarning("No next level provided");
        }
    }
}
