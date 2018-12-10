using Assets.Scripts.EnvironmentLogic;
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
        public UICanvasGroupFader Fader;
        public Color FadeColor = Color.white;
        public float FadeInTime = 0.5f;
        public float FadeOutTime = 1f;

        [Header("Sounds")]
        public Sound SoundA;
        public Sound SoundB;
        public Sound ActivatedSound;
        
        [Header("Objects control")]
        public GameObject NewPlayer;
        public GameObject[] ObjectsToDestroy;
        public GameObject[] ObjectsToSpawn;
        public GameObject[] ObjectsToDisable;
        public GameObject[] ObjectsToEnable;
        public ActivatorProxy[] ActivatableTargets;

        private GameObject _player;
        private SphereCollider _collider;
        private readonly int[] _melody = new int[4] {0, 0, 0, 1};
        private int _step;
        private float _t;
        private State _state;
        private Image _faderImage;
        private CameraController _camera;
        private Color _defaultImageColor;
        private float _defaultFadeTime;
        private float _defaultMusicVolume;

        void Start ()
        {
            _collider = GetComponent<SphereCollider>();
            _state = State.NotActivatedYet;

            _camera = FindObjectOfType<CameraController>();
            if (Fader == null)
            {
                Fader = FindObjectOfType<UICanvasGroupFader>();
                if (Fader == null)
                    Debug.LogError("NO FADER IN SCENE");
            }

            _defaultFadeTime = Fader.FadeTime;
            _faderImage = Fader.GetComponent<Image>();
            if(_faderImage != null)
                _defaultImageColor = Color.black;

            Fader.StateChanged += StateChanged;
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

                if (distanceToPlayer < 2.5f)
                {
                    _state = State.Activating;
                    Activate();
                }

                var music = SoundManager.Instance.MusicHandler;
                if (music != null && music.IsActive)
                    music.Volume = _defaultMusicVolume * (1f - d);
            }
        }

        void OnTriggerEnter(Collider col)
        {
            if (_player == null && col.CompareTag("Player"))
                _player = col.gameObject;

            var music = SoundManager.Instance.MusicHandler;
            if (music != null)
                _defaultMusicVolume = music.Volume;
        }

        private void StateChanged()
        {
            if (_state == State.Activating && Fader.State == UICanvasGroupFader.FaderState.FadedIn)
            {
                SoundManager.Instance.Play(ActivatedSound);

                if (_camera != null)
                {
                    _camera.TraumaDecayStep *= 0.05f;
                    _camera.Shake(1f);
                }

                if(ObjectsToDisable != null)
                    foreach (var o in ObjectsToDisable)
                        o.SetActive(false);

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

                if (ObjectsToEnable != null)
                    foreach (var o in ObjectsToEnable)
                        o.SetActive(true);

                if(ActivatableTargets != null)
                    foreach (var a in ActivatableTargets)
                        a.Activate();
                    
                var music = SoundManager.Instance.MusicHandler;
                if(music != null && music.IsActive)
                    music.Stop();

                Debug.Log("!!!!!!!!!!! STUFF IS HAPPENING !!!!!!!!!!!!");
                _state = State.Activated;
                Fader.FadeTime = FadeOutTime;
                Fader.FadeOut();
            }

            if (_state == State.Activated && Fader.State == UICanvasGroupFader.FaderState.FadedOut)
            {
                if (_camera != null)
                {
                    _camera.TraumaDecayStep *= 20f;
                }

                if (_faderImage != null)
                    _faderImage.color = _defaultImageColor;

                Fader.FadeTime = _defaultFadeTime;
            }
        }

        void Activate()
        {
            if (Fader != null)
            {
                if(_faderImage != null)
                    _faderImage.color = FadeColor;
                Fader.FadeTime = FadeInTime;
                Fader.FadeIn();
            }
        }
    }
}
