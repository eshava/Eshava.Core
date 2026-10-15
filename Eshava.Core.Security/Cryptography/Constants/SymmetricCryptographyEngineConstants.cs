namespace Eshava.Core.Security.Cryptography.Constants
{
	/// <summary>
	/// The values of <see cref="Interfaces.ISymmetricCryptographyEngine.Type"/>, one per engine.
	/// Usable as service keys when several engines are registered side by side.
	/// </summary>
	public static class SymmetricCryptographyEngineConstants
	{
		public const string RIJNDAEL = "Rijndael";
		public const string AESGCM = "AesGcm";
		public const string MIGRATING = "Migrating";
	}
}