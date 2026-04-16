using System;
using System.Text;

namespace Quantum.Utilities
{
    /// <summary>
    /// Silent security manager for source-level obfuscation and protection.
    /// Renamed and simplified to avoid detection by casual reverse-engineers.
    /// </summary>
    public static class Security
    {
        // Secret flags for hidden state tracking
        // true = legit, false = security violation or outdated
        public static bool _network_v3_internal_state = true;
        
        // XOR Key for string encryption
        private static readonly byte[] _entropy = { 0x51, 0x75, 0x61, 0x6E, 0x74, 0x75, 0x6D, 0x21 }; // "Quantum!"

        /// <summary>
        /// Decrypts an obfuscated string using XOR logic.
        /// </summary>
        public static string Decrypt(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            try
            {
                byte[] data = Convert.FromBase64String(input);
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(data[i] ^ _entropy[i % _entropy.Length]);
                }
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                return "ERR_SEC_FAULT";
            }
        }

        /// <summary>
        /// Encrypts a string for initial storage in the source.
        /// Use this to generate the strings to be put into Decrypt calls.
        /// </summary>
        public static string Encrypt(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            
            byte[] data = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ _entropy[i % _entropy.Length]);
            }
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Misleading routine to hide logic checks.
        /// Always returns true unless tempered with.
        /// </summary>
        public static bool ValidateBufferIntegrity()
        {
            return _network_v3_internal_state;
        }
    }
}
