namespace Eshava.Core.Security.Cryptography.Models
{
	public class AesGcmKey
	{
		/// <summary>
		/// Lower-case letters and digits, 1 to 16 characters, for example "k202610".
		/// Stored in every value encrypted with this key.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// A 256-bit key, base64 encoded.
		/// </summary>
		public string Key { get; set; }
	}
}