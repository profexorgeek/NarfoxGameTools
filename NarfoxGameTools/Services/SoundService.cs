using FlatRedBall;
using FlatRedBall.Audio;
using FlatRedBall.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using NarfoxGameTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NarfoxGameTools.Services
{
    public class SoundService
    {
        protected struct SoundRequest
        {
            public string Name;
            public float Volume;
            public float Pitch;
            public float Pan;
            public double TimeRequested;
            public double Duration;
        }

        const float PitchVariance = 0.5f;

        private static SoundService instance;
        private List<SoundRequest> soundQueue = new List<SoundRequest>();
        private Dictionary<SoundEffectInstance, PositionedObject> ownedInstances = new Dictionary<SoundEffectInstance, PositionedObject>();
        private ContentManager contentManager;
        float musicVolume = 1;
        bool initialized = false;

        public static SoundService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SoundService();
                }
                return instance;
            }
        }

        public float VolumeMaxDistance { get; set; }
        
        public float MaxConcurrentSounds { get; set; } = 32;
        public float CurrentlyPlayingSounds
        {
            get
            {
                return soundQueue.Count;
            }
        }
        public string ContentManagerName
        {
            get
            {
                return contentManager.Name;
            }
            set
            {
                var name = value;
                contentManager = FlatRedBallServices.GetContentManagerByName(name);
                if (contentManager == null)
                {
                    throw new Exception($"Bad content manager name: {name}");
                }
            }
        }
        public string SoundFolder { get; set; } = @"Content/GlobalContent/Sounds";
        public string MusicFolder { get; set; } = @"Content/GlobalContent/Music";
        public float MusicMixVolume { get; set; } = 1f;
        public float SoundMixVolume { get; set; } = 1f;
        public float MusicVolume
        {
            get
            {
                return musicVolume;
            }
            set
            {
                musicVolume = value;
                MediaPlayer.Volume = CalcMusicVolume;
            }
        }
        public float SoundVolume { get; set; } = 1f;

        float CalcMusicVolume => MusicVolume * MusicMixVolume;
        float CalcSoundVolume => SoundVolume * SoundMixVolume;



        public bool IsMuted { get; set; } = false;

        protected SoundService() { }

        public void Initialize(string managerName = null)
        {
            ContentManagerName = managerName ?? FlatRedBallServices.GlobalContentManager;
            
            // user camera for rough max distance
            VolumeMaxDistance = Camera.Main.AbsoluteRightXEdgeAt(0);

            initialized = true;
        }

        public void Update()
        {
            if(!initialized)
            {
                throw new Exception("Attempted to update SoundService before calling Initialize()!");
            }

            // play sounds
            for (int i = soundQueue.Count - 1; i > -1; i--)
            {
                var req = soundQueue[i];
                if(TimeManager.CurrentTime - req.TimeRequested > req.Duration)
                {
                    soundQueue.Remove(req);
                }
            }

            // manage named instances
            foreach (var kvp in ownedInstances)
            {
                if(!kvp.Key.IsDisposed && kvp.Value != null)
                {
                    kvp.Key.Pan = GetPanForPosition(kvp.Value.Position);
                    kvp.Key.Volume = GetVolumeForPosition(kvp.Value.Position);
                }
            }
        }

        public void RequestPlayEffect(string effectName, Vector3? position = null, bool randomizePitch = true)
        {
            if(!initialized)
            {
                throw new Exception("Attempted to play effect before initializing the SoundService.");
            }

            // EARLY OUT: null name
            if(string.IsNullOrWhiteSpace(effectName))
            {
                LogService.Log.Warn($"Empty sound name requested!");
                return;
            }

            var pitch = randomizePitch ? RandomService.Random.InRange(-PitchVariance, PitchVariance) : 0f;
            var request = new SoundRequest
            {
                Name = effectName,
                Pitch = pitch,
                Volume = GetVolumeForPosition(position),
                Pan = GetPanForPosition(position),
                TimeRequested = TimeManager.CurrentScreenTime,
            };
            PlaySound(request);
        }

        public void RequestPlaySong(Song song, bool loop = true, bool forceRestart = false)
        {
            var current = AudioManager.CurrentlyPlayingSong;

            // if we have no song, or our current song has a different name, or we're forcing
            // a song restart - stop other music and start playing the new song
            if (current == null || current.Name != song.Name || forceRestart)
            {
                AudioManager.StopSong();
                MediaPlayer.Volume = CalcMusicVolume;
                MediaPlayer.IsRepeating = loop;
                AudioManager.PlaySong(song, true, true);
            }
        }

        public SoundEffectInstance GetOwnedInstance(string effectName, PositionedObject requestor = null, bool isLooped = true)
        {
            SoundEffect effect = GetEffect(effectName);
            SoundEffectInstance instance = null;

            if (effect != null)
            {
                instance = effect.CreateInstance();
                instance.IsLooped = isLooped;

                if(requestor != null)
                {
                    ownedInstances.Add(instance, requestor);
                }
            }

            return instance;
        }

        public void UnloadOwnedInstance(SoundEffectInstance instance)
        {
            if(ownedInstances.ContainsKey(instance))
            {
                instance.Stop();
                ownedInstances.Remove(instance);
            }
        }

        public void UnloadAllOwnedInstances()
        {
            ownedInstances.Clear();
        }


        protected float GetVolumeForPosition(Vector3? nullablePosition)
        {
            // EARLY OUT: null position
            if (IsMuted)
            {
                return 0f;
            }

            // assume max volume to start, if no position was
            // passed, this will be the default
            float volume = 1f;

            // if we got a position, calculate the sound based
            // on max distance
            if(nullablePosition != null)
            {
                var position = nullablePosition.Value;
                var dist = Camera.Main.DistanceTo(position.X, position.Y);
                if (dist < VolumeMaxDistance)
                {
                    volume = 1f - (dist / VolumeMaxDistance);
                }
            }

            volume *= CalcSoundVolume;

            return volume;
        }

        protected float GetPanForPosition(Vector3? nullablePosition)
        {
            // EARLY OUT: null position
            if (nullablePosition == null)
            {
                return 0f;
            }

            var position = nullablePosition.Value;
            return ((position.X - Camera.Main.X) / VolumeMaxDistance).Clamp(-1f, 1f);
        }

        protected void PlaySound(SoundRequest request)
        {
            var effect = GetEffect(request.Name);

            if (effect == null)
            {
                LogService.Log.Warn($"Bad effect requested: {request.Name}!");
            }
            else if (IsMuted == false && CurrentlyPlayingSounds < MaxConcurrentSounds)
            {
                request.Duration = effect.Duration.TotalMilliseconds / 1000f;
                request.TimeRequested = TimeManager.CurrentTime;
                soundQueue.Add(request);

                try
                {
                    effect.Play(request.Volume, request.Pitch, request.Pan);
                }
                catch (Exception e)
                {
                    LogService.Log.Error(e.Message);
                }

            }
            else
            {
                LogService.Log.Warn($"Too many sounds requested {CurrentlyPlayingSounds}/{MaxConcurrentSounds}");
            }
        }

        protected SoundEffect GetEffect(string name)
        {
            var barename = Path.GetFileNameWithoutExtension(name);
            var path = Path.Combine(SoundFolder, barename);
            var effect = contentManager.Load<SoundEffect>(path);
            return effect;
        }

    }
}
