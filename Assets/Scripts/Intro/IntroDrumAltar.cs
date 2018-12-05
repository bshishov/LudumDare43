using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Intro
{
    [RequireComponent(typeof(SphereCollider))]
    public class IntroDrumAltar : MonoBehaviour
    {
        public enum State
        {
            NotActivatedYet,
            Activating,
            Activated
        }

        [Header("Detection")]
        public AnimationCurve DrumSpeedByInvDistance = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve VolumeByInvDistance = AnimationCurve.Linear(0, 0, 1, 1);

        [Header("Fading")]
        public Color FadeColor = Color.white;
        public float FadeInTime = 0.5f;
        public float FadeOutTime = 1f;

        [Header("Sounds")]
        public Sound SoundA;
        public Sound SoundB;
        public Sound ActivatedSound;
        
        
        [Header("Spawning")]
        public GameObject[] ObjectsToDestroy;
        public GameObject NewPlayer;
        public GameObject[] ObjectsToSpawn;

        private GameObject _player;
        private SphereCollider _collider;
        private readonly int[] _melody = new int[4] {0, 0, 0, 1};
        private int _step;
        private float _t;
        private State _state;
        private UICanvasGroupFader _fader;
        private Image _faderImage;
        private CameraController _camera;
        private Color _defaultImageColor;
        private float _defaultFadeTime;

        void Start ()
        {
            _collider = GetComponent<SphereCollider>();
            _state = State.NotActivatedYet;

            _camera = FindObjectOfType<CameraController>();
            _fader = FindObjectOfType<UICanvasGroupFader>();
            if (_fader == null)
                Debug.LogError("NO FADER IN SCENE");

            _defaultFadeTime = _fader.FadeTime;
            _faderImage = _fader.GetComponent<Image>();
            if(_faderImage != null)
                _defaultImageColor = Color.black;

            _fader.StateChanged += StateChanged;
        }

        void Update ()
        {
            if(_player == null)
                return;

            if (_state == State.NotActivatedYet)
            {
                var distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);
                var d = 1f - Mathf.Clamp01(distanceToPlayer / _collider.radius);

                _t += Time.deltaTime * DrumSpeedByInvDistance.Evaluate(d);
                if (_t > 1.0f)
                {
                    var note = _melody[_step];

                    var noteSound = SoundA;
                    if (note == 1)
                        noteSound = SoundB;

                    var soundHandler = SoundManager.Instance.Play(noteSound);
                    if (soundHandler != null)
                    {
                        soundHandler.AttachToObject(transform);
                        soundHandler.Volume = VolumeByInvDistance.Evaluate(d);
                    }

                    _t -= 1.0f;
                    _step = (_step + 1) % _melody.Length;
                }

                if (d >= 0.9f)
                {
                    _state = State.Activating;
                    Activate();
                }
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (_player == null && col.CompareTag("Player"))
                _player = col.gameObject;
        }

        private void StateChanged()
        {
            if (_state == State.Activating && _fader.State == UICanvasGroupFader.FaderState.FadedIn)
            {
                SoundManager.Instance.Play(ActivatedSound);

                if (_camera != null)
                {
                    _camera.TraumaDecayStep *= 0.05f;
                    _camera.Shake(1f);
                }

                if(ObjectsToDestroy != null)
                    foreach (var o in ObjectsToDestroy)
                        Destroy(o);

                if (NewPlayer != null)
                {
                    GameObject.Instantiate(NewPlayer, _player.transform.position, _player.transform.rotation);
                    GameObject.Destroy(_player);
                }

                if (ObjectsToSpawn != null)
                    foreach (var o in ObjectsToSpawn)
                        GameObject.Instantiate(o, transform.position, Quaternion.identity);

                Debug.Log("!!!!!!!!!!! STUFF IS HAPPENING !!!!!!!!!!!!");
                _state = State.Activated;
                _fader.FadeTime = FadeOutTime;
                _fader.FadeOut();
            }

            if (_state == State.Activated && _fader.State == UICanvasGroupFader.FaderState.FadedOut)
            {
                if (_camera != null)
                {
                    _camera.TraumaDecayStep *= 20f;
                }

                if (_faderImage != null)
                    _faderImage.color = _defaultImageColor;

                _fader.FadeTime = _defaultFadeTime;
            }
        }

        void Activate()
        {
            if (_fader != null)
            {
                if(_faderImage != null)
                    _faderImage.color = FadeColor;
                _fader.FadeTime = FadeInTime;
                _fader.FadeIn();
            }
        }
    }
}
