using System.Collections.Generic;

namespace Eshava.Core.Security.Cryptography.Models
{
	public class AesGcmSettings
	{
		public AesGcmSettings()
		{
			Keys = new List<AesGcmKey>();
		}

		/// <summary>
		/// The id of the key every new value is encrypted with.
		/// While it is empty, the engine decrypts but does not encrypt.
		/// </summary>
		public string ActiveKeyId { get; set; }

		/// <summary>
		/// Every key a stored value may name. A value is always decrypted with the key its id names.
		/// </summary>
		public List<AesGcmKey> Keys { get; set; }
	}
}