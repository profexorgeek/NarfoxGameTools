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

namespace NarfoxGameTools.Services
{

    /// <summary>
    /// This class wraps FlatRedBall and MonoGame audio utilities
    /// to provide more advanced functionality like positioned audio
    /// </summary>
    public class SoundService
    {
        /// <summary>
        /// A struct that represents a queued request
        /// to play a sound.
        /// </summary>
        protected struct SoundRequest
        {
            public string Name;
            public float Volume;
            public float Pitch;
            public float Pan;
            public double TimeRequested;
            public double Duration;
        }

        static SoundService instance;
        List<SoundRequest> soundQueue = new List<SoundRequest>();
        Dictionary<SoundEffectInstance, PositionedObject> ownedInstances = new Dictionary<SoundEffectInstance, PositionedObject>();
        ContentManager contentManager;
        float musicVolume = 1;
        bool initialized = false;
        PositionedObject target;

        /// <summary>
        /// Property exposing this class as a singleton
        /// </summary>
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

        /// <summary>
        /// The maximum distance a sound can be heard,
        /// usually in pixels for 2D pixelart games.
        /// Positioned sound will attenuate linearly
        /// over this distance.
        /// </summary>
        public float VolumeMaxDistance { get; set; } = 800;

        /// <summary>
        /// The target "listener" to use when determining audio
        /// position. This is usually the camera but may need
        /// to be a different target if the camera interpolates
        /// to a position over time.
        /// </summary>
        public PositionedObject Target
        {
            get
            {
                if (target == null)
                {
                    target = Camera.Main;
                }
                return target;
            }
            set
            {
                target = value;

                if (target == null)
                {
                    target = Camera.Main;
                }
            }
        }

        /// <summary>
        /// The maximum number of sounds to play at the same time. This may need to be 
        /// set differently for mobile or lowspec devices that have low limits on
        /// simultaneous sounds
        /// </summary>
        public float MaxConcurrentSounds { get; set; } = 32f;

        /// <summary>
        /// The number of sounds in the queue
        /// </summary>
        public float CurrentlyPlayingSounds
        {
            get
            {
                return soundQueue.Count;
            }
        }

        /// <summary>
        /// The name of the content manager responsible for loading and caching sounds
        /// </summary>
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

        /// <summary>
        /// The folder where sounds are located.
        /// </summary>
        public string SoundFolder { get; set; } = @"Content/GlobalContent/Sounds";

        /// <summary>
        /// The folder where music is located.
        /// </summary>
        public string MusicFolder { get; set; } = @"Content/GlobalContent/Music";

        /// <summary>
        /// The mix volume of the music: how loud it should be in the mix, from 0 - 1
        /// </summary>
        public float MusicMixVolume { get; set; } = 1f;

        /// <summary>
        /// The mix volume of the sound: how loud it should be in the mix, from 0 - 1
        /// </summary>
        public float SoundMixVolume { get; set; } = 1f;

        /// <summary>
        /// The volume of music in the game, this is usually set by the user in some type
        /// of settings menu. Values range between 0 to 1, inclusive;
        /// </summary>
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

        /// <summary>
        /// The volume of sound in the game, this is usually set by the user in some type
        /// of settings menu.  Values range between 0 to 1, inclusive;
        /// </summary>
        public float SoundVolume { get; set; } = 1f;

        /// <summary>
        /// The calculated total volume of music, a product of the mix volume and user volume
        /// </summary>
        float CalcMusicVolume => MusicVolume * MusicMixVolume;

        /// <summary>
        /// The calculated total volume of sound, a product of the mix volume and user volume
        /// </summary>
        float CalcSoundVolume => SoundVolume * SoundMixVolume;

        /// <summary>
        /// Whether or not sound will play. If true, no sound will play at all.
        /// 
        /// NOTE: does not affect music. Music should be managed with volume or by
        /// stopping/playing songs.
        /// </summary>
        public bool IsMuted { get; set; } = false;

        /// <summary>
        /// How much the pitch can vary (up or down so total range is double the setting)
        /// when playing sounds with pitch variance
        /// </summary>
        public float PitchVariance { get; set; } = 0.25f;

        /// <summary>
        /// The name of the song that is currently playing
        /// </summary>
        public string CurrentSongName => AudioManager.CurrentlyPlayingSong?.Name;


        /// <summary>
        /// Protected constructor to enforce singleton pattern
        /// </summary>
        protected SoundService() { }

        /// <summary>
        /// Initializes the sound service. This must be called before attempting
        /// to play sounds or otherwise use the SoundService. This should be called
        /// in the Game1 class in FlatRedBall during Initialization
        /// </summary>
        /// <param name="managerName">The name of the manager to use, defaults to the GlobalContentManager</param>
        public void Initialize(string managerName = null)
        {
            ContentManagerName = managerName ?? FlatRedBallServices.GlobalContentManager;

            // use camera for default volume distance and target
            VolumeMaxDistance = Camera.Main.AbsoluteRightXEdgeAt(0);

            initialized = true;
        }

        /// <summary>
        /// This must be called in the game loop. This processes the queue of requested
        /// sounds every frame
        /// </summary>
        public void Update()
        {
            if (!initialized)
            {
                throw new Exception("Attempted to update SoundService before calling Initialize()!");
            }

            // play sounds
            for (int i = soundQueue.Count - 1; i > -1; i--)
            {
                var req = soundQueue[i];
                if (TimeManager.CurrentTime - req.TimeRequested > req.Duration)
                {
                    soundQueue.Remove(req);
                }
            }

            // manage named instances
            foreach (var kvp in ownedInstances)
            {
                if (!kvp.Key.IsDisposed && kvp.Value != null)
                {
                    kvp.Key.Pan = GetPanForPosition(kvp.Value.Position);
                    kvp.Key.Volume = IsMuted ? 0 : GetVolumeForPosition(kvp.Value.Position);
                }
            }
        }

        /// <summary>
        /// Fire-and-forget method to play a single sound effect. Passing the Position will pan
        /// and attenuate the sound based on the Target position. This method doesn't
        /// keep a handle to the sound effect and so effects played this way can not be
        /// stopped or altered once started.
        /// </summary>
        /// <param name="effectName">The name of the effect, which will be resolved from the base directories</param>
        /// <param name="position">The position of the effect, used to attenuate and pan the sound</param>
        /// <param name="randomizePitch">Whether or not to randomize pitch, defaults to true</param>
        public void RequestPlayEffect(string effectName, Vector3? position = null, bool randomizePitch = true)
        {
            if (!initialized)
            {
                throw new Exception("Attempted to play effect before initializing the SoundService.");
            }

            // EARLY OUT: null name
            if (string.IsNullOrWhiteSpace(effectName))
            {
                LogService.Log.Debug($"Empty sound name requested!");
                return;
            }

            var pitch = randomizePitch ? RandomService.Random.InRange(-PitchVariance, PitchVariance) : 0f;
            var request = new SoundRequest
            {
                Name = effectName,
                Volume = GetVolumeForPosition(position),
                Pan = GetPanForPosition(position),
                Pitch = pitch,
                TimeRequested = TimeManager.CurrentScreenTime,
            };
            PlaySound(request);
        }

        /// <summary>
        /// Fire-and-forget method to play a single sound effect with specific volume, pan, and pitch
        /// </summary>
        /// <param name="effectName">The name of the effect, which will be resolved from the base directories</param>
        /// <param name="volume">The volume of the effect from 0 to 1</param>
        /// <param name="pan">The pan of the effect from -1 to 1</param>
        /// <param name="pitch">The pitch of the effect</param>
        public void RequestPlayEffect(string effectName, float volume, float pan, float pitch)
        {
            if (!initialized)
            {
                throw new Exception("Attempted to play effect before initializing the SoundService.");
            }

            // EARLY OUT: null name
            if (string.IsNullOrWhiteSpace(effectName))
            {
                LogService.Log.Debug($"Empty sound name requested!");
                return;
            }

            var request = new SoundRequest
            {
                Name = effectName,
                Volume = volume,
                Pan = pan,
                Pitch = pitch,
                TimeRequested = TimeManager.CurrentScreenTime,
            };
            PlaySound(request);
        }

        /// <summary>
        /// Plays a song and can force a restart or loop
        /// </summary>
        /// <param name="song">The song instance to play</param>
        /// <param name="loop">Whether or not to loop the song</param>
        /// <param name="forceRestart">Whether or not to force a restart if a song is already playing</param>
        public void RequestPlaySong(Song song, bool loop = true, bool forceRestart = false)
        {
            var current = AudioManager.CurrentlyPlayingSong;

            // if we have no song, or our current song has a different name, or we're forcing
            // a song restart - stop other music and start playing the new song
            if (CurrentSongName == null || CurrentSongName != song.Name || forceRestart)
            {
                AudioManager.StopSong();
                MediaPlayer.Volume = CalcMusicVolume;
                MediaPlayer.IsRepeating = loop;
                AudioManager.PlaySong(song, true, true);
            }
        }

        /// <summary>
        /// Resumes playing the current song if there is one.
        /// 
        /// No operation if there is no current song.
        /// </summary>
        public void ResumeSong()
        {
            if(AudioManager.CurrentSong != null)
            {
                AudioManager.PlaySong();
            }
        }

        /// <summary>
        /// Stops playing the current song
        /// </summary>
        public void StopSong()
        {
            AudioManager.StopSong();
        }

        /// <summary>
        /// Gets a sound effect instance with a handle. This is used for sounds that need to be
        /// updated over time to stop and start or play at a specific pitch. Any sound requested
        /// this way should be released when the requestor is done with the sound using the
        /// UnloadOwnedInstance method
        /// </summary>
        /// <param name="effectName"></param>
        /// <param name="requestor"></param>
        /// <param name="playImmediately"></param>
        /// <param name="isLooped"></param>
        /// <returns>A SoundEffectInstance</returns>
        public SoundEffectInstance GetOwnedInstance(string effectName, PositionedObject requestor = null, bool playImmediately = false, bool isLooped = true)
        {
            SoundEffect effect = GetEffect(effectName);
            SoundEffectInstance instance = null;

            if (effect != null)
            {
                instance = effect.CreateInstance();

                // volume defaults to zero because the position may not be taken
                // into account until next frame and we don't want audio to
                // pop in and go quiet. This allows requestors to immediately
                // play
                instance.Volume = 0;
                instance.Pan = 0;
                instance.IsLooped = isLooped;

                if (playImmediately)
                {
                    instance.Play();
                }

                if (requestor != null)
                {
                    ownedInstances.Add(instance, requestor);
                }
            }

            return instance;
        }

        /// <summary>
        /// Unloads an owned SoundEffectInstance obtained using GetOwnedInstance
        /// </summary>
        /// <param name="instance">The instance to unload</param>
        public void UnloadOwnedInstance(SoundEffectInstance instance)
        {
            if (instance != null && ownedInstances.ContainsKey(instance))
            {
                instance.Stop();
                ownedInstances.Remove(instance);
            }
        }

        /// <summary>
        /// Unloads all owned instances. This is usually called when unloading
        /// a screen to make sure all owned instances are released.
        /// </summary>
        public void UnloadAllOwnedInstances()
        {
            ownedInstances.Clear();
        }

        /// <summary>
        /// Calculates the volume of a sound based on its distance
        /// from the Target listener. Returns 1 if provided with a
        /// bad position
        /// </summary>
        /// <param name="nullablePosition">The position to use for calculation</param>
        /// <returns>A float representing the volume from 0 to 1</returns>
        public float GetVolumeForPosition(Vector3? nullablePosition)
        {
            // EARLY OUT: null position
            if (IsMuted)
            {
                return 0f;
            }

            // assume max volume to start, if no position was
            // passed, this will be the default
            var volume = 1f;

            // if we got a position, calculate the sound based
            // on max distance
            if (nullablePosition != null)
            {
                var position = nullablePosition.Value;
                var dist = Target.DistanceTo(position.X, position.Y);
                if (dist < VolumeMaxDistance)
                {
                    volume = 1f - (dist / VolumeMaxDistance);
                }
                else
                {
                    volume = 0f;
                }
            }

            volume *= CalcSoundVolume;

            return volume;
        }

        /// <summary>
        /// Calculates the pan for a sound based on its position to
        /// left or right of the Target listener
        /// </summary>
        /// <param name="nullablePosition">The position to use for calculation</param>
        /// <returns>A float from -1 to 1</returns>
        public float GetPanForPosition(Vector3? nullablePosition)
        {
            // EARLY OUT: null position
            if (nullablePosition == null)
            {
                return 0f;
            }
            var position = nullablePosition.Value;
            var deltaX = position.X - Target.X;
            var percent = deltaX / VolumeMaxDistance;
            return percent.Clamp(-1f, 1f);
        }

        /// <summary>
        /// Plays a sound request object, usually called during Update when
        /// processing the sound queue
        /// </summary>
        /// <param name="request">The sound request object to play</param>
        protected void PlaySound(SoundRequest request)
        {
            var effect = GetEffect(request.Name);
            //var effectLeft = GetEffect(request.Name);
            //var effectRight = GetEffect(request.Name);

            if (effect == null)
            {
                LogService.Log.Debug($"Bad effect requested: {request.Name}!");
            }
            else if (IsMuted == false && CurrentlyPlayingSounds < MaxConcurrentSounds)
            {
                request.Duration = effect.Duration.TotalMilliseconds / 1000f;
                request.TimeRequested = TimeManager.CurrentTime;
                soundQueue.Add(request);

                try
                {
                    // NOTE: pan does not seem to be working right. Did thorough
                    // investigation and it appears to be an issue with the
                    // underlying default audio engine in monogame
                    // see:
                    // https://github.com/MonoGame/MonoGame/issues/6876
                    // https://github.com/MonoGame/MonoGame/issues/6543
                    // https://github.com/MonoGame/MonoGame/issues/5739

                    // One potential hack is to play two sounds at once and fake panning:
                    //float panVolumeCompensation = // figure out the calc for this;
                    //var leftVolume = (1 - request.Pan).Clamp(0, 1) * request.Volume * panVolumeCompensation;
                    //var rightVolume = (1 + request.Pan).Clamp(0, 1) * request.Volume * panVolumeCompensation;
                    //effectLeft.Play(leftVolume, request.Pitch, -1);
                    //effectRight.Play(rightVolume, request.Pitch, 1);

                    // This is how the sound should actually be played
                    effect.Play(request.Volume, request.Pitch, request.Pan);
                }
                catch (Exception e)
                {
                    LogService.Log.Error(e.Message);
                }
            }
            else
            {
                LogService.Log.Debug($"Too many sounds requested {CurrentlyPlayingSounds}/{MaxConcurrentSounds}");
            }
        }

        /// <summary>
        /// Loads a sound effect by name from the content manager
        /// </summary>
        /// <param name="name">The filename of the effect</param>
        /// <returns>A SoundEffect instance</returns>
        protected SoundEffect GetEffect(string name)
        {
            var barename = Path.GetFileNameWithoutExtension(name);
            var path = Path.Combine(SoundFolder, barename);
            var effect = contentManager.Load<SoundEffect>(path);
            return effect;
        }
    }
}
