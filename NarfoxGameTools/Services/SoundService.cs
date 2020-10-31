using FlatRedBall;
using FlatRedBall.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using NarfoxGameTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
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
        private ContentManager contentManager;
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

        public float VolumeMaxDistance
        {
            get
            {
                // use twice the camera's longest dimension
                // as the maximum sound distance
                return Camera.Main.AbsoluteRightXEdgeAt(0) * 2;
            }
        }
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



        public bool IsMuted { get; set; } = false;

        protected SoundService() { }

        public void Initialize(string managerName = null)
        {
            ContentManagerName = managerName ?? FlatRedBallServices.GlobalContentManager;
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


        protected float GetVolumeForPosition(Vector3? nullablePosition)
        {
            // EARLY OUT: null position
            if (IsMuted || nullablePosition == null)
            {
                return 0f;
            }
            var position = nullablePosition.Value;
            var volume = 0f;
            var dist = Camera.Main.DistanceTo(position.X, position.Y);
            if (dist < VolumeMaxDistance)
            {
                volume = 1f - (dist / VolumeMaxDistance);
            }
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
            var barename = Path.GetFileNameWithoutExtension(request.Name);
            var path = Path.Combine(SoundFolder, barename);
            var effect = contentManager.Load<SoundEffect>(path);

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
    }
}
