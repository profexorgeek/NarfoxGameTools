using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace NarfoxGameTools.Services
{
    /// <summary>
    /// This class SHOULD NOT be used for sensitive data. This provides
    /// really easy encryption and decryption of strings as an extension
    /// method. This is useful for light encryption/decryption of game
    /// data.
    /// </summary>
    public static class SimpleCryptoService
    {
        private static byte[] key;
        private static byte[] iv;
        private static bool initialized;
        private static SymmetricAlgorithm algo;

        /// <summary>
        /// The key used to encrypt and decrypt messages. The
        /// key is considered secret in that it is used to
        /// decrypt data
        /// </summary>
        public static string KeyBase64String
        {
            get
            {
                if (key != null)
                {
                    return Convert.ToBase64String(key);
                }
                return "";
            }
            set
            {
                key = Convert.FromBase64String(value);
            }
        }

        /// <summary>
        /// The Initialization Vector, a random string that is used
        /// to "salt" a message before encryption. The Initialization
        /// Vector is typically considered non-secret.
        /// </summary>
        public static string IvBase64String
        {
            get
            {
                if (iv != null)
                {
                    return Convert.ToBase64String(iv);
                }
                return "";
            }
            set
            {
                iv = Convert.FromBase64String(value);
            }
        }

        public static void Initialize(string keyString, string keyVector)
        {
            algo = DES.Create();
            KeyBase64String = GetValidBase64StringOrError(keyString);
            IvBase64String = GetValidBase64StringOrError(keyVector);
            initialized = true;
        }

        /// <summary>
        /// An extension method on the string object that allows strings
        /// to be encrypted on the fly by calling myString.Encrypt().
        /// Note that the SimpleCryptoService must be initialized with a
        /// key and initialization vector or an exception will be thrown.
        /// </summary>
        /// <param name="str">The unencrypted string to encrypt</param>
        /// <returns>An encrypted string</returns>
        public static string Encrypt(this string str)
        {
            if (!initialized)
            {
                var msg = "Crypto service has not been initialized.";
                LogService.Log.Error(msg);
                throw new Exception(msg);
            }

            ICryptoTransform transform = algo.CreateEncryptor(key, iv);
            byte[] inputBuffer = Encoding.UTF8.GetBytes(str);
            byte[] outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            return Convert.ToBase64String(outputBuffer);
        }

        /// <summary>
        /// An extension method on the string object that allows strings
        /// to be decrypted on the fly by calling myString.Decrypt().
        /// Note that the SimpleCryptoService must be initialized with a
        /// key and initialization vector or an exception will be thrown.
        /// </summary>
        /// <param name="str">The encrypted string to decrypt</param>
        /// <returns>A decrypted string</returns>
        public static string Decrypt(this string str)
        {
            if (!initialized)
            {
                var msg = "Crypto system has not been initialized!";
                LogService.Log.Error(msg);
                throw new Exception(msg);
            }

            ICryptoTransform transform = algo.CreateDecryptor(key, iv);
            byte[] inputBuffer = Convert.FromBase64String(str);
            byte[] outputBuffer = transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
            return Encoding.UTF8.GetString(outputBuffer);
        }

        static string GetValidBase64StringOrError(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            var bits = bytes.Length * 8;
            if (bits != 64)
            {
                throw new Exception($"Got {bits}bits but this encryption algorithm requires a 64bit key (8 UTF characters)!");
            }
            return Convert.ToBase64String(bytes);

        }
    }
}
