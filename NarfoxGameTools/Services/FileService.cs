using NarfoxGameTools.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NarfoxGameTools.Services
{
    /// <summary>
    /// This class makes it easier to save and load data,
    /// either encrypted or unencrypted using the SimpleCryptoService,
    /// as JSON data using Newtonsoft.Json
    /// </summary>
    public class FileService
    {
        static FileService instance;

        public static FileService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FileService();
                }
                return instance;
            }
        }
        

        public string AppDataDirectory
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
        }
        public ILogger Logger { get; set; }


        private FileService() { }


        public List<string> GetDirectoryFiles(string path, string extension = "")
        {
            var files = new List<string>();
            if (Directory.Exists(path))
            {
                files = Directory.GetFiles(path, "*" + extension).ToList();
            }

            for (int i = 0; i < files.Count; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }

            return files;
        }

        public T LoadFile<T>(string path, bool decrypt = false)
        {
            var filetext = LoadText(path);
            var json = decrypt ? filetext.Decrypt() : filetext;
            return Deserialize<T>(json);
        }

        public void SaveFile(object model, string path, bool encrypt = false)
        {
            var data = Serialize(model, !encrypt);
            var savetext = encrypt ? data.Encrypt() : data;
            SaveText(path, savetext);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void EncryptFile(string srcPath, string destPath)
        {
            var src = LoadText(srcPath);
            var encrypted = src.Encrypt();
            SaveText(destPath, encrypted);
        }

        public T Clone<T>(T obj)
        {
            var json = Serialize(obj);
            var cloned = Deserialize<T>(json);
            return cloned;
        }


        string Serialize(Object obj, bool prettyFormat = false)
        {
            var formatting = prettyFormat ? Formatting.Indented : Formatting.None;
            var json = JsonConvert.SerializeObject(obj, formatting);
            return json;
        }

        T Deserialize<T>(string json)
        {
            T runtime = JsonConvert.DeserializeObject<T>(json);
            return runtime;
        }

        bool SaveText(string path, string text)
        {
            bool success = false;
            try
            {
                // We don't use the FRB FileManager here because it
                // attempts to force isolated storage
                File.WriteAllText(path, text);
                success = true;
            }
            catch (Exception e)
            {
                var msg = string.Format("Failed to save to {0}, error: {1}", path, e);
                LogService.Log.Error(msg);
            }

            return success;
        }

        string LoadText(string path)
        {
            string text;
            try
            {
                text = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                var msg = string.Format("Failed to load from {0}, error: {1}", path, e);
                LogService.Log.Error(msg);
                text = "";
            }
            return text;
        }

    }
}
