# core
Collection of methods, classes and helpers to simplify recurring standard actions

## core.security
Extension to secure data worthy of protection

* Checksum
	* Fletcher's checksum
* Cryptography
	* AES-GCM engine (symmetric, authenticated, with a key ring) — .NET 8 and above
	* Rijndael engine (symmetric)
	* Migrating engine, reading both and writing AES-GCM
* Hash
	* Password engine

### AES-GCM engine

`AesGcmEngine` implements `ISymmetricCryptographyEngine` with AES-GCM: a 256-bit key, a random 96-bit
nonce for every value and a 128-bit authentication tag. A value that was changed, truncated or
written with another key throws a `CryptographicException` instead of decrypting to garbage.

Every value starts with a header that is authenticated together with it:

```
offset  length  content
0       3       magic 0x11 0x21 0xB5 — the base64 of every value starts with "ESG1"
3       1       length L of the key id
4       L       key id, ASCII, [a-z0-9], 1 to 16 characters
4+L     12      nonce
16+L    n       ciphertext (the plaintext as UTF-16LE)
16+L+n  16      tag
```

The fixed prefix makes values in this format easy to find in a database — `LIKE 'ESG1%'` on a base64
column — and `AesGcmEngine.IsAesGcmFormat` does the same for a byte array.

The settings are a key ring:

```json
"CryptographySettings": {
	"ActiveKeyId": "k202610",
	"Keys": [ { "Id": "k202610", "Key": "<base64 of 32 random bytes>" } ]
}
```

```csharp
var engine = new AesGcmEngine(configuration.GetSection("CryptographySettings").Get<AesGcmSettings>());
var stored = Convert.ToBase64String(engine.Encrypt("secret"));
var secret = engine.Decrypt(Convert.FromBase64String(stored));
```

* **New values are written with `ActiveKeyId`; a value is read with the key its id names.** To rotate,
  add a key under a new id and switch `ActiveKeyId` — older values stay readable for as long as their
  key is in the ring. There is no fallback to another key.
* **Without an active key id the engine only decrypts**; `CanEncrypt` tells which.
* **Invalid settings throw in the constructor** — an id outside `[a-z0-9]{1,16}`, an id given twice,
  a key that is not 256 bits, an active key id that names no key.
* `GenerateKeys` returns a fresh 256-bit key and an empty IV, and never changes the settings.
* A random 96-bit nonce is safe for about 2^32 encryptions per key.

### Registering more than one engine

The engines are told apart by `ISymmetricCryptographyEngine.Type`, whose values are the constants in
`SymmetricCryptographyEngineConstants`. With the keyed services of
`Microsoft.Extensions.DependencyInjection`, the engine a caller gets is chosen at registration:

```csharp
services
	.AddKeyedSingleton<ISymmetricCryptographyEngine>(SymmetricCryptographyEngineConstants.RIJNDAEL, (_, _) => new RijndaelEngine(rijndaelSettings))
	.AddKeyedSingleton<ISymmetricCryptographyEngine>(SymmetricCryptographyEngineConstants.AESGCM, (_, _) => new AesGcmEngine(aesGcmSettings))
	.AddSingleton<ISymmetricCryptographyEngine>(provider => new MigratingCryptographyEngine(
		provider.GetRequiredKeyedService<ISymmetricCryptographyEngine>(SymmetricCryptographyEngineConstants.AESGCM),
		provider.GetRequiredKeyedService<ISymmetricCryptographyEngine>(SymmetricCryptographyEngineConstants.RIJNDAEL)
	));
```

### Moving off the Rijndael engine

`RijndaelEngine` uses a single IV from its settings for every value and no authentication, so values
sharing a prefix share their leading ciphertext blocks, and a changed block is not detected.
`MigratingCryptographyEngine` moves stored values to AES-GCM without a moment in which a reader cannot
read what a writer wrote:

1. **Readers first**: register the migrating engine with an empty `ActiveKeyId`. It reads both
   formats and still writes the old one.
2. **Then the key**: set `ActiveKeyId`. New values are written as AES-GCM.
3. **Then the rewrite**: decrypt every stored value that does not start with the header and encrypt
   it again.
4. **Finally** register `AesGcmEngine` alone and remove the old key.

The migrating engine checks the engines it is given and throws if they are swapped.