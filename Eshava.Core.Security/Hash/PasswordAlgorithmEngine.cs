using System;
using System.Security.Cryptography;
using Eshava.Core.Security.Hash.Interfaces;

namespace Eshava.Core.Security.Hash
{
	public sealed class PasswordAlgorithmEngine : IPasswordAlgorithmEngine
	{
		private const int SALT_SIZE = 256;
		private const int HASH_SIZE = 256;
		private const int HASH_ITERATIONS = 1000;
		
		public (string HashBase64, string SaltBase64) Hash(string password)
		{
			var salt = new byte[SALT_SIZE];
			using (var randomNumberGenerator = RandomNumberGenerator.Create())
			{
				randomNumberGenerator.GetBytes(salt);
			}

			var hash = DeriveHash(password, salt);

			return (Convert.ToBase64String((byte[])hash.Clone()), Convert.ToBase64String((byte[])salt.Clone()));
		}

		public bool Verify(string password, string hashBase64, string saltBase64)
		{
			var salt = new byte[SALT_SIZE];
			var hash = new byte[SALT_SIZE];
			
			Array.Copy(Convert.FromBase64String(saltBase64), 0, salt, 0, SALT_SIZE);
			Array.Copy(Convert.FromBase64String(hashBase64), 0, hash, 0, HASH_SIZE);

			var test = DeriveHash(password, salt);
			for (var index = 0; index < HASH_SIZE; index++)
			{
				if (test[index] != hash[index])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// PBKDF2 with HMAC-SHA1 - **named explicitly, not changed**: it is what the parameterless constructors have
		/// always used, so every hash stored before still verifies.
		/// </summary>
		private static byte[] DeriveHash(string password, byte[] salt)
		{
#if NET8_0_OR_GREATER
			return Rfc2898DeriveBytes.Pbkdf2(password, salt, HASH_ITERATIONS, HashAlgorithmName.SHA1, HASH_SIZE);
#else
			using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, HASH_ITERATIONS))
			{
				return deriveBytes.GetBytes(HASH_SIZE);
			}
#endif
		}
	}
}