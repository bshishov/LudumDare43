using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.EnvironmentLogic
{
    [RequireComponent(typeof(ActivatorProxy))]
    public class SacrificeAltar : MonoBehaviour
    {
        public float Decay = 0.1f;
        public float RotationSpeed = 1f;
        public float SacrificeRange = 2f;
        public float EnergyPerHamster = 10f;

        [Header("Visuals")]
        public Transform Sphere;
        public Transform Ring1;
        public Transform Ring2;
        
        private float _trauma = 0f;
        private List<Collider> _hamstersInRange = new List<Collider>();
        private ActivatorProxy _activator;

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

        void Update ()
        {
            _trauma = Mathf.Clamp01(_trauma - Decay * Time.deltaTime);
            var k1 = Mathf.Pow(_trauma, 2);
            var k2 = Mathf.Pow(_trauma, 3);

            var altarActive = _trauma > 0.2f;
            Sphere.localScale = Vector3.one * 2 * k1;
            Ring1.localScale = Vector3.one * k2;
            Ring2.localScale = Vector3.one * k2;
            Ring1.Rotate(Vector3.forward, k2 * RotationSpeed * Time.deltaTime);
            Ring2.Rotate(Vector3.down, k2 * RotationSpeed * Time.deltaTime);

            if (altarActive)
            {
                if (_hamstersInRange.Count > 0)
                {
                    var sacrificed = new List<Collider>();
                    foreach (var col in _hamstersInRange)
                    {
                        var hc = col.GetComponent<HamsterController>();
                        if (hc != null && Vector3.Distance(col.transform.position, transform.position) < SacrificeRange)
                        {
                            Debug.Log("[ALTAR] Hamster sacrifices");
                            StartCoroutine(SacrificeAnimation(hc));
                            sacrificed.Add(col);
                            DrumController.Instance.IncreaseEnergy(EnergyPerHamster);
                        }
                    }

                    foreach (var col in sacrificed)
                        _hamstersInRange.Remove(col);
                }
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("Hamster") && !_hamstersInRange.Contains(col))
            {
                _hamstersInRange.Add(col);
            }
        }

        void OnTriggerExit(Collider col)
        {
            if (col.CompareTag("Hamster") && _hamstersInRange.Contains(col))
            {
                _hamstersInRange.Remove(col);
            }
        }

        IEnumerator SacrificeAnimation(HamsterController hc)
        {
            hc.Die();
            yield return new WaitForSeconds(0.1f);
            var rc = hc.GetComponent<RagdollController>();
            rc.enabled = false;

            // TODO: WHOA SUCH A SPAGHETTI!!
            for (var i = 0; i < 200; i++)
            {
                hc.transform.localScale *= 0.99f;
                hc.transform.position = Vector3.Lerp(hc.transform.position, transform.position, 0.99f);
                yield return null;
            }

            Destroy(hc.gameObject);
            yield return null;
        }
    }
}
