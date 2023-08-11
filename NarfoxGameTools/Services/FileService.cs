using NarfoxGameTools.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
        string appVersionString = null;


        /// <summary>
        /// The application Version as a string with Major.Minor.Build
        /// and the specific platform build prepended. For example:
        /// PC 1.23.5
        /// </summary>
        public string AppVersionString
        {
            get
            {
                if (appVersionString == null)
                {
                    var versionString = AppVersion.ToString(3);
                    appVersionString = $"PC {versionString}";
                }
                return appVersionString;
            }
        }

        /// <summary>
        /// The application Version
        /// </summary>
        public Version AppVersion => Assembly.GetEntryAssembly().GetName().Version;

        /// <summary>
        /// Singleton instance of this service.
        /// </summary>
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
        
        /// <summary>
        /// The AppData directory as provided by the environment,
        /// returned as a path string for convenience.
        /// </summary>
        public string AppDataDirectory
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
        }

        public string AppName
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                string name = assembly == null ? "" : Assembly.GetEntryAssembly().FullName;
                if(!string.IsNullOrEmpty(name))
                {
                    name = name.Substring(0, name.IndexOf(','));
                }
                return name;
            }
        }

        /// <summary>
        /// The default place for your game to save user data. This is not your
        /// game's content folder but rather where you'd put a saved game or
        /// settings file. It will create a directory in AppData/Roaming
        /// that matches your assembly name. If your assembly name cannot
        /// be resolved, the directory will be called "narfox".
        /// </summary>
        public string DefaultSaveDirectory
        {
            get
            {
                var defaultSaveDirectory = AppName.ToLower();
                if (string.IsNullOrEmpty(defaultSaveDirectory))
                {
                    defaultSaveDirectory = "narfox";
                }

                var fullpath = Path.Combine(AppDataDirectory, defaultSaveDirectory);

                if(Directory.Exists(fullpath) == false)
                {
                    Directory.CreateDirectory(fullpath);
                }

                return fullpath;
            }
        }

        /// <summary>
        /// The extension to use when saving data. Defaults to ".sav"
        /// </summary>
        public string Extension { get; set; } = ".sav";

        /// <summary>
        /// The extension to use when saving a backup. Defaults to ".bak"
        /// </summary>
        public string BackupExtension { get; set; } = ".bak";
        
        /// <summary>
        /// The ILogger instance to use when logging
        /// </summary>
        public ILogger Logger { get; set; }




        /// <summary>
        /// Private constructor for Singleton pattern access
        /// </summary>
        private FileService() { }



        /// <summary>
        /// Converts the provided name into a lower case
        /// string and converts spaces to underscores and appends
        /// the save extension
        /// </summary>
        /// <param name="name">The save file name to convert.</param>
        /// <returns></returns>
        public string GetSaveFileName(string name)
        {
            var safeName = name.ToLower().Replace(" ", "_");
            return safeName + Extension;
        }

        /// <summary>
        /// Checks if the provided path is absolute by checking if
        /// it contains the default save directory. Returns a path
        /// that is reliably absolute.
        /// </summary>
        /// <param name="unknownPath">A path that may or may not be absolute</param>
        /// <returns>A path that is absolute</returns>
        public string GetAbsoluteSavePath(string unknownPath)
        {
            bool isAbsolute = unknownPath.Contains(DefaultSaveDirectory);
            return isAbsolute ? unknownPath : Path.Combine(DefaultSaveDirectory, unknownPath);
        }

        /// <summary>
        /// Returns a list of all files in a directory with the option to filter
        /// for a specific extension. Often used to find all player save files in
        /// the save directory.
        /// </summary>
        /// <param name="path">The directory path to search</param>
        /// <param name="extension">The specific extension to filter for</param>
        /// <returns>A list of file paths, limited to a specific extension if one was provided.</returns>
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

        /// <summary>
        /// Copies a file from the srcPath to the destPath and overwrites
        /// any existing file with the same name by default.
        /// 
        /// Noop if file doesn't exist
        /// </summary>
        /// <param name="srcPath">The path of the file to copy</param>
        /// <param name="destPath">The path where the file should be copied to</param>
        /// <param name="overwrite">Whether to overwrite existing file at destination path</param>
        public void CopyFile(string srcPath, string destPath, bool overwrite = true)
        {
            if (File.Exists(srcPath))
            {
                File.Copy(srcPath, destPath, overwrite);
            }
        }

        /// <summary>
        /// Loads text from the provided path, decrypts it if the decrypt argument
        /// is true, and deserializes the result into an instance of T.
        /// 
        /// Decryption requires the SimpleCryptoService to be initialized.
        /// </summary>
        /// <typeparam name="T">The expected type to deserialize</typeparam>
        /// <param name="path">The path to a JSON file</param>
        /// <param name="decrypt">Whether to decrypt the file contents, requires the SimpleCryptoService to be initialized.</param>
        /// <returns>An object of type T</returns>
        public T LoadFile<T>(string path, bool decrypt = false)
        {
            var filetext = LoadText(path);
            string json;

            if(decrypt)
            {
                try
                {
                    json = filetext.Decrypt();
                }
                catch(Exception e)
                {
                    LogService.Log.Error($"Problem encountered while trying to decrypt save: {path}. File may not be encrypted, attempting to load without decrypting.");
                    LogService.Log.Error($"Decryption error: {e.Message}");
                    json = filetext;
                }
            }
            else
            {
                json = filetext;
            }
            return Deserialize<T>(json);
        }

        /// <summary>
        /// Serializes and object and saves it to the provided path.
        /// 
        /// Can encrypt JSON but requires the SimpleCryptoService to be
        /// initialized.
        /// 
        /// Will overwrite if a file already exists at the provided path.
        /// </summary>
        /// <param name="model">The object to serialize</param>
        /// <param name="path">The file path to save the serialized object</param>
        /// <param name="encrypt">Whether to encrypt the JSON before saving</param>
        public void SaveFile(object model, string path, bool encrypt = false)
        {
            var prettyFormat = !encrypt;
            var data = Serialize(model, prettyFormat);
            SaveFile(data, path, encrypt);
        }
        
        /// <summary>
        /// Saves text to the provided path, optionally encrypting using
        /// the SimpleCryptoService
        /// </summary>
        /// <param name="text">The text to save</param>
        /// <param name="path">The path to save</param>
        /// <param name="encrypt">Whether to encrypt or not</param>
        public void SaveFile(string text, string path, bool encrypt = false)
        {
            var savetext = encrypt ? text.Encrypt() : text;
            SaveText(path, savetext);
        }

        /// <summary>
        /// Deletes the file at the provided path.
        /// 
        /// Just a wrapper for File.Delete
        /// </summary>
        /// <param name="path">The path to delete.</param>
        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        /// <summary>
        /// Loads, encrypts, and saves a file. Requires
        /// the SimpleCryptoService to be initialized.
        /// </summary>
        /// <param name="srcPath">The path to load and encrypt</param>
        /// <param name="destPath">The path to save</param>
        public void EncryptFile(string srcPath, string destPath)
        {
            var src = LoadText(srcPath);
            var encrypted = src.Encrypt();
            SaveText(destPath, encrypted);
        }

        /// <summary>
        /// Loads, decrypts, and saves a file. Requires the
        /// SimpleCryptoService to be initialized.
        /// </summary>
        /// <param name="srcPath">The path to load and decrypt</param>
        /// <param name="destPath">The path to save</param>
        public void DecryptFile(string srcPath, string destPath)
        {
            var src = LoadText(srcPath);
            var decrypted = src.Decrypt();
            SaveText(destPath, decrypted);
        }

        /// <summary>
        /// Does a deep clone by serializing the provided object
        /// to JSON and deserializing it back into a new object.
        /// </summary>
        /// <typeparam name="T">The type of object that will be cloned</typeparam>
        /// <param name="obj">The object to clone</param>
        /// <returns>A deep clone of the provided object</returns>
        public T Clone<T>(T obj)
        {
            var json = Serialize(obj);
            var cloned = Deserialize<T>(json);
            return cloned;
        }

        /// <summary>
        /// Serializes a provided object and returns a JSON string
        /// using JSON.Net.
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <param name="prettyFormat">Minifies if fales, prettifies if true</param>
        /// <returns>A JSON string representation of the provided object</returns>
        public string Serialize(Object obj, bool prettyFormat = false)
        {
            var formatting = prettyFormat ? Formatting.Indented : Formatting.None;
            var json = JsonConvert.SerializeObject(obj, formatting);
            return json;
        }

        /// <summary>
        /// Deserializes the provided string and returns an object using
        /// JSON.Net.
        /// </summary>
        /// <typeparam name="T">The destination type</typeparam>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>An object of type T</returns>
        public T Deserialize<T>(string json)
        {
            T runtime = JsonConvert.DeserializeObject<T>(json);
            return runtime;
        }



        /// <summary>
        /// Saves the provided text to the specified path.
        /// </summary>
        /// <param name="path">The path to save</param>
        /// <param name="text">The text to save</param>
        /// <returns>A boolean indicating success (true) or failure (false)</returns>
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

        /// <summary>
        /// Loads the text in a provided file path and returns
        /// it as a string.
        /// </summary>
        /// <param name="path">The path to load</param>
        /// <returns>The text contents of the file at the specified path</returns>
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
