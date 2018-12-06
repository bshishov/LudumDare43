using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


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

        public AudioMixerGroup MixerGroup
        {
            get { return Source.outputAudioMixerGroup; }
            set { if (IsActive) Source.outputAudioMixerGroup = value; }
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

        public Transform AttachedTo { get; private set; }

        public SoundHandler(AudioSource source)
        {
            Source = source;
        }

        public void Stop()
        {
            if (Source != null)
                Source.Stop();
        }

        public void AttachToObject(Transform transform1, 
            float spatialBlend = 0.7f, 
            float minDistance = 15f,
            float maxDistance = 40f)
        {
            AttachedTo = transform1;
            Source.spatialBlend = spatialBlend;
            Source.rolloffMode = AudioRolloffMode.Linear;
            Source.minDistance = minDistance;
            Source.maxDistance = maxDistance;
            Source.dopplerLevel = 0f;
        }
    }

    public const int MaxSounds = 32;
    
    public SoundHandler MusicHandler;
    public MixerGroupLimitSettings LimitSettings;

    private static SoundManager _instance;

    private readonly List<SoundHandler> _handlers = new List<SoundHandler>();
    private readonly Dictionary<AudioMixerGroup, int> _groupCounter = new Dictionary<AudioMixerGroup, int>();
    private readonly List<SoundHandler> _inactiveHandlers = new List<SoundHandler>();

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
            if (manager.LimitSettings == null)
            {
                Debug.Log("[SoundManager] Loading Limit Settings from resources");
                manager.LimitSettings = Resources.Load<MixerGroupLimitSettings>("SoundLimitSettings");
            }

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
        foreach (var handler in _handlers)
        {
            if (!handler.IsActive)
            {
                _inactiveHandlers.Add(handler);
            }
            else
            {
                if (handler.AttachedTo != null)
                    handler.Source.transform.position = handler.AttachedTo.transform.position;
            }
        }
        
        foreach (var handler in _inactiveHandlers)
        {
            var sourceGroup = handler.Source.outputAudioMixerGroup;
            if (sourceGroup != null && _groupCounter.ContainsKey(sourceGroup))
                _groupCounter[sourceGroup] -= 1;
            Destroy(handler.Source.gameObject);
            _handlers.Remove(handler);
        }

        _inactiveHandlers.Clear();
    }

    public SoundHandler Play(AudioClip clip, float volume = 1f, bool loop = false, float pitch = 1f,
        bool ignoreListenerPause = false, float delay = 0f, AudioMixerGroup mixerGroup=null)
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
        source.outputAudioMixerGroup = mixerGroup;
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
        return Play(clip.Clip, clip.VolumeModifier, loop, clip.Pitch * pitch, ignoreListenerPause, delay);
    }

    public SoundHandler Play(Sound sound)
    {
        if (sound == null)
            return null;
        
        if (sound.MixerGroup != null && LimitSettings != null)
        {
            if (_groupCounter.ContainsKey(sound.MixerGroup))
            {
                var soundsOfSameGroup = _groupCounter[sound.MixerGroup];
                if (soundsOfSameGroup >= LimitSettings.GetLimit(sound.MixerGroup))
                {
                    //Debug.Log(string.Format("[SoundManager] Too many sounds for group {0}", sound.MixerGroup));
                    return null;
                }

                // There are sounds in group so increment by one
                _groupCounter[sound.MixerGroup] = soundsOfSameGroup + 1;
            }
            else
            {
                // First sound in group
                _groupCounter.Add(sound.MixerGroup, 1);
            }
        }

        var pitch = sound.Pitch;
        if (sound.RandomizePitch)
            pitch = Random.Range(pitch - sound.MaxPitchShift, pitch + sound.MaxPitchShift);

        var delay = sound.Delay;
        if (sound.RandomizeDelay)
            delay += Random.value * sound.MaxAdditionalDelay;

        var handler = Play(sound.Clip, sound.VolumeModifier, sound.Loop, pitch, sound.IgnoreListenerPause, delay, sound.MixerGroup);
        return handler;
    }

    public SoundHandler Play(Sound sound, Transform attachTo)
    {
        var s = Play(sound);
        if (s != null)
            s.AttachToObject(attachTo);

        return s;
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