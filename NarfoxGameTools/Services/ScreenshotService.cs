using FlatRedBall;
using FlatRedBall.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NarfoxGameTools.Services
{
    public class ScreenshotService
    {
        private static ScreenshotService instance;
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

        public string SavePath { get; set; } = "";

        private ScreenshotService() { }

        public void SaveScreenshotPng(string path = null, Camera camera = null)
        {
            if(!FlatRedBallServices.IsInitialized)
            {
                throw new Exception("FlatRedBall must be fully initialized before you can take a screenshot!");
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

            // try to get a default save path if one has not been set
            if(string.IsNullOrWhiteSpace(SavePath))
            {
                TryResolveDefaultSavePath();
            }
            path = Path.Combine(SavePath, GetFilename(path));

            if (File.Exists(path))
            {
                throw new Exception($"Existing pathname generated: {path}. This can happen if you take screenshots too quickly.");
            }

            using (Stream stream = System.IO.File.Create(path))
            {
                buffer.SaveAsPng(stream, width, height);
            }
        }

        private void TryResolveDefaultSavePath()
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Screenshots");
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    throw new Exception($"Cannot create default screenshot location at: {path} because {e.Message}.");
                }
            }
            SavePath = path;
        }

        private string GetFilename(string name = null)
        {
            name = name ?? DateTime.Now.ToString("G");
            var cleanName = Regex.Replace(name, "[^0-9A-Za-z]", "_");
            return Path.GetFileNameWithoutExtension(cleanName) + ".png";
        }
    }
}
