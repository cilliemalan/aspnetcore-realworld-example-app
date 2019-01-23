using ConduitApi.Models;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConduitApi.Infrastructure
{
    public class Authentication
    {
        private Func<byte[], byte[]> _signer;
        private Func<byte[], byte[], bool> _verifier;

        /// <summary>
        /// Creates an authentication provider using HMAC as signing algorithm.
        /// </summary>
        /// <param name="key">The key for the HMAC algorithm</param>
        /// <remarks>
        /// The constructor takes a signer and verifier, so the keys are stored in
        /// a closure, rather than having them lie around in memory somewhere more
        /// easily accessible. The algorithm used is HMACSHA256 and the key is
        /// hashed with SHA512 before handing it to the HMAC (because HMAC block size = 64).
        /// </remarks>
        public static Authentication CreateHmac(string key)
        {
            byte[] hashedKey;
            using (var hash = SHA512.Create())
            {
                hashedKey = hash.ComputeHash(Encoding.UTF8.GetBytes(key));
            }

            var hmac = new HMACSHA256(hashedKey);
            return new Authentication(
                d => hmac.ComputeHash(d),
                (d, s) => BytesEqual(s, hmac.ComputeHash(d)));
        }

        /// <summary>
        /// Create an authentication provider, using the given signer and verifier.
        /// </summary>
        /// <param name="signer">A function that will sign the input.</param>
        /// <param name="verifier">A function that will verify a given blob and signature.</param>
        public Authentication(Func<byte[], byte[]> signer, Func<byte[], byte[], bool> verifier)
        {
            _signer = signer;
            _verifier = verifier;
        }

        private string Encode(byte[] b) => Convert.ToBase64String(b)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        private byte[] Decode(string s)
        {
            try
            {
                string incoming = s.Replace('_', '/').Replace('-', '+');
                switch (s.Length % 4)
                {
                    case 2: incoming += "=="; break;
                    case 3: incoming += "="; break;
                }
                return Convert.FromBase64String(incoming);
            }
            catch (FormatException) { return null; }
        }

        public string GenerateToken(MinimalUser user) =>
            GenerateToken(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user)));

        public string GenerateToken(byte[] contents)
        {
            var signature = _signer(contents);
            return $"{Encode(contents)}.{Encode(signature)}";
        }

        public bool VerifyToken(string token)
        {
            var match = Regex.Match(token, @"^([0-9a-zA-Z_-]+)\.([0-9a-zA-Z_-]+)$");
            if (!match.Success)
            {
                return false;
            }
            else
            {
                var blob = Decode(match.Groups[1].Value);
                var sig = Decode(match.Groups[2].Value);

                return blob != null && sig != null && _verifier(blob, sig);
            }
        }

        public MinimalUser DeserializeToken(string token)
        {
            var match = Regex.Match(token, @"^([0-9a-zA-Z_-]+)\.([0-9a-zA-Z_-]+)$");
            if (match.Success)
            {
                var blob = Decode(match.Groups[1].Value);
                var sig = Decode(match.Groups[2].Value);

                if(blob != null)
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<MinimalUser>(Encoding.UTF8.GetString(blob));
                    }
                    catch { }
                }
            }

            return default;
        }

        public string HashPassword(string password)
        {
            byte[] salt = new byte[32];
            byte[] encodedPassword = Encoding.UTF8.GetBytes(password);
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = HashPasswordInternal(salt, encodedPassword);
            return $"{Encode(salt)}.{Encode(hash)}";
        }

        private byte[] HashPasswordInternal(byte[] salt, byte[] encodedPassword)
        {
            using (var sha = SHA256.Create())
            {
                byte[] tohash = new byte[salt.Length + encodedPassword.Length];
                Array.Copy(salt, 0, tohash, 0, salt.Length);
                Array.Copy(encodedPassword, 0, tohash, salt.Length, encodedPassword.Length);
                return sha.ComputeHash(tohash);
            }
        }

        public bool VerifyPassword(string password, string hashed)
        {
            byte[] encodedPassword = Encoding.UTF8.GetBytes(password);
            var match = Regex.Match(hashed, @"^([0-9a-zA-Z_-]+)\.([0-9a-zA-Z_-]+)$");
            if (!match.Success)
            {
                return false;
            }
            else
            {
                var salt = Decode(match.Groups[1].Value);
                var hash = Decode(match.Groups[2].Value);
                if (salt == null || hash == null)
                {
                    return false;
                }
                else
                {
                    var hashedAgain = HashPasswordInternal(salt, encodedPassword);
                    return BytesEqual(hash, hashedAgain);
                }
            }
        }

        private static bool BytesEqual(byte[] hash, byte[] hashedAgain)
        {
            if(hash == null || hashedAgain == null || hash.Length != hashedAgain.Length || hash.Length == 0)
            {
                return false;
            }

            for(int i=0;i<hash.Length;i++)
            {
                if (hash[i] != hashedAgain[i]) return false;
            }

            return true;
        }
    }
}
