using System.Collections.Generic;
using System.Linq;
using UnityEngine;


    public class SoundManager : MonoBehaviour
    {
        public class SoundHandler
        {
            public AudioSource Source { get; private set; }

            public bool IsLooped
            {
                get { return Source.loop; }
                set { if (IsActive) Source.loop = value; }
            }

            public float Pitch
            {
                get { return Source.pitch; }
                set { if (IsActive) Source.pitch = value; }
            }

            public float Volume
            {
                get { return Source.volume; }
                set { if (IsActive) Source.volume = value; }
            }

            public bool IgnoreListenerPause
            {
                get { return Source.ignoreListenerPause; }
                set { if (IsActive) Source.ignoreListenerPause = value; }
            }

            public bool IsActive
            {
                get
                {
                    if (Source.clip != null)
                    {
                        if (Source.clip.loadState == AudioDataLoadState.Loading)
                            return true;
                    }
                    return Source.isPlaying;
                }
            }

            public SoundHandler(AudioSource source)
            {
                Source = source;
            }

            public void Stop()
            {
                if (Source != null)
                    Source.Stop();
            }

            public void AttachToObject(Transform transform1)
            {
            }
        }

        public const int MaxSounds = 24;


        private readonly List<SoundHandler> _handlers = new List<SoundHandler>();
        public SoundHandler MusicHandler;
        private static SoundManager _instance;

        public static SoundManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                var existing = FindObjectOfType<SoundManager>();
                if (existing != null)
                {
                    Debug.Log("[SoundManager] Reusing");
                    _instance = existing;
                    return existing;
                }

                Debug.Log("[SoundManager] Instantiating");
                var go = new GameObject("[SoundManager]");
                var manager = go.AddComponent<SoundManager>();
                _instance = manager;
                return manager;
            }
        }

        void Awake()
        {
            if (!Instance.Equals(this))
            {
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            var inactiveHandlers = _handlers.Where(h => !h.IsActive).ToList();
            foreach (var handler in inactiveHandlers)
            {
                Destroy(handler.Source.gameObject);
                _handlers.Remove(handler);
            }
        }

        public SoundHandler Play(AudioClip clip, float volume = 1f, bool loop = false, float pitch = 1f,
            bool ignoreListenerPause = false, float delay = 0f)
        {
            if (clip == null)
                return null;

            if (_handlers.Count >= MaxSounds)
            {
                Debug.Log("[SoundManager] Too many sounds");
                return null;
            }

            var go = new GameObject(string.Format("Sound: {0}", clip.name));
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();

            source.clip = clip;
            source.priority = 128;
            source.playOnAwake = false;
            source.volume = volume;
            source.loop = loop;
            source.pitch = pitch;
            source.ignoreListenerPause = ignoreListenerPause;

            var sound = new SoundHandler(source);
            _handlers.Add(sound);

            if (delay > 0)
                source.PlayDelayed(delay);
            else
                source.Play();

            return sound;
        }
         
        public SoundHandler Play(AudioClipWithVolume clip, bool loop = false, float pitch = 1f,
            bool ignoreListenerPause = false, float delay = 0f)
        {
            if (clip == null)
                return null;
            return Play(clip.Clip, clip.VolumeModifier, loop, pitch, ignoreListenerPause, delay);
        }

        public SoundHandler PlayMusic(AudioClipWithVolume clip, bool loop = true, float pitch = 1f,
            bool ignoreListenerPause = false, float delay = 0f)
        {
            if (clip == null)
                return null;

            if (clip.Clip == null)
                return null;


            if (MusicHandler != null)
            {
                Debug.Log(clip.VolumeModifier);
                MusicHandler.Source.clip = clip.Clip;
                //MusicHandler.Volume = clip.VolumeModifier;
                MusicHandler.Source.volume = clip.VolumeModifier;
                MusicHandler.IsLooped = loop;
                MusicHandler.Pitch = pitch;
                MusicHandler.Source.Play();
                return MusicHandler;
            }

            var handler = Play(clip, loop, pitch, true, delay);
            DontDestroyOnLoad(handler.Source.gameObject);
            MusicHandler = handler;
            return handler;
        }
    }