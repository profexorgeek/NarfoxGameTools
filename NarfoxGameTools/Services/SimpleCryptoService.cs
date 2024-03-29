﻿using System;
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

        /// <summary>
        /// Initialize the crypto service with a raw string and vector.
        /// </summary>
        /// <param name="keyString">A UTF8 string that's exactly 8 characters (64bit)</param>
        /// <param name="keyVector">A UTF8 string with exactly 8 characters (64bit)</param>
        /// <param name="convert">Whether the provided strings are base64 already or need to be converted.
        /// If false is passed and the strings are not valid 64bit strings, errors will be thrown when
        /// trying to decrypt!</param>
        public static void Initialize(string keyString, string keyVector, bool convert = true)
        {
            algo = DES.Create();

            if (convert)
            {
                KeyBase64String = GetValidBase64StringOrError(keyString);
                IvBase64String = GetValidBase64StringOrError(keyVector);
            }
            else
            {
                KeyBase64String = keyString;
                IvBase64String = keyVector;
            }
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

        /// <summary>
        /// Gets a base64 string from the provided UTF8 string that
        /// is exactly 64bits in length, otherwise throws an error
        /// </summary>
        /// <param name="str">A UTF8 string that should be 64bits or 8 characters long</param>
        /// <returns>A valid 64bit base64 string</returns>
        /// <exception cref="Exception">Throws exception when string is incorrect number of bits</exception>
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
