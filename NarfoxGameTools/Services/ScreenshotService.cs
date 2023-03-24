using FlatRedBall;
using FlatRedBall.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NarfoxGameTools.Services
{
    public class ScreenshotService
    {
        static ScreenshotService instance;
        string savePath = "";
        bool initialized = false;

        public static ScreenshotService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ScreenshotService();
                }

                return instance;
            }
        }

        public string SaveDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(savePath))
                {
                    savePath = DefaultSaveDirectory;
                }
                return savePath;
            }
            set
            {
                savePath = value;
            }
        }

        public string PicturesDirectory
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
        }

        public string DefaultSaveDirectory
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                string defaultSaveDirectory = assembly == null ? "" : Assembly.GetEntryAssembly().FullName;
                if (string.IsNullOrEmpty(defaultSaveDirectory))
                {
                    defaultSaveDirectory = "narfox";
                }
                else
                {
                    defaultSaveDirectory = defaultSaveDirectory.Substring(0, defaultSaveDirectory.IndexOf(','));
                }

                var fullpath = Path.Combine(PicturesDirectory, defaultSaveDirectory);

                if (Directory.Exists(fullpath) == false)
                {
                    Directory.CreateDirectory(fullpath);
                }

                return fullpath;
            }
        }

        private ScreenshotService() { }

        public void Initialize(string saveLocation = null)
        {
            if (!FlatRedBallServices.IsInitialized)
            {
                throw new Exception("FlatRedBall must be fully initialized you can initialize this service!");
            }

            if(saveLocation != null)
            {
                savePath = saveLocation;
            }

            initialized = true;
        }

        public void SaveScreenshotPng(string filename = null, Camera camera = null)
        {
            if(!initialized)
            {
                throw new Exception("Tried to take screenshot before service was initialized. You must initialize the ScreenshotService to use it!");
            }


            camera = camera ?? Camera.Main;
            var gfx = FlatRedBallServices.GraphicsDevice;
            var width = FlatRedBallServices.GraphicsOptions.ResolutionWidth;
            var height = FlatRedBallServices.GraphicsOptions.ResolutionHeight;
            var buffer = new RenderTarget2D(
                FlatRedBallServices.GraphicsDevice,
                width,
                height);

            // get existing render target
            var existingTargets = gfx.GetRenderTargets();

            // render
            gfx.SetRenderTarget(buffer);
            Renderer.DrawCamera(camera, null);

            // restore render targets
            gfx.SetRenderTargets(existingTargets);

            var path = Path.Combine(SaveDirectory, GetFilename(filename));

            if (File.Exists(path))
            {
                throw new Exception($"Existing pathname generated: {path}. This can happen if you take screenshots too quickly.");
            }
            using (Stream stream = System.IO.File.Create(path))
            {
                buffer.SaveAsPng(stream, width, height);
            }
        }

        private string GetFilename(string name = null)
        {
            // Old, but we're going to match the windows recording naming convention
            //name = name ?? DateTime.Now.ToString("G");
            //var cleanName = Regex.Replace(name, "[^0-9A-Za-z]", "_");
            //return Path.GetFileNameWithoutExtension(cleanName) + ".png";
            if(name != null)
            {
                var cleanName = Regex.Replace(name, "[^0-9A-Za-z]", "_");
                return Path.GetFileNameWithoutExtension(cleanName) + ".png";
            }
            else
            {
                var now = DateTime.Now;
                return now.ToString("yyyy-mm-dd HH-mm-ss") + ".png";
            }
        }
    }
}
