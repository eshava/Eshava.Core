# Changelog

Notable changes per released version of `Eshava.Core.Security`, newest first. Versions before 1.0.1
are not documented here — the Git history is the source for those.

## 1.0.1

### Added

* **`AesGcmEngine`, an authenticated symmetric engine.** AES-GCM with a 256-bit key, a random 96-bit
  nonce per value and a 128-bit tag. Every value carries a header — a three-byte magic whose base64 is
  `ESG1`, then the id of the key it was written with — and the header is authenticated together with
  the value. A changed bit, a swapped key id or a truncated value throws instead of decrypting to
  something else. The plaintext is encoded as UTF-16LE, as in `RijndaelEngine`.
* **A key ring, `AesGcmSettings`.** `ActiveKeyId` names the key new values are written with; `Keys`
  holds every key a stored value may name, each a base64 encoded 256-bit key under an id of 1 to 16
  lower-case letters and digits. Rotating a key means adding one and switching `ActiveKeyId`; the
  older values stay readable. Without an active key id the engine decrypts but refuses to encrypt.
* **`MigratingCryptographyEngine`** for moving stored values off `RijndaelEngine`: it decrypts a value
  carrying the header through AES-GCM and anything else through the legacy engine, and encrypts
  through AES-GCM once an active key id is set, through the legacy engine until then.
* **`SymmetricCryptographyEngineConstants`**, the values of `ISymmetricCryptographyEngine.Type` —
  `RIJNDAEL`, `AESGCM`, `MIGRATING` — usable as keys when several engines are registered side by side.
* **Targets `net8.0`, `net9.0` and `net10.0`** beside `netstandard2.0`. The AES-GCM types exist only
  in the `net8.0` builds and above, since `System.Security.Cryptography.AesGcm` is not part of
  .NET Standard 2.0.

### Changed

* **`RijndaelEngine` uses `Aes.Create()` instead of `RijndaelManaged`**, which is obsolete since
  .NET 6. With the 128-bit block size `RijndaelManaged` always used, both are the same algorithm, so
  values written by either are read by the other; the settings and the output format are unchanged.
* **`RijndaelEngine.Type` returns `SymmetricCryptographyEngineConstants.RIJNDAEL`**, which is the
  same string, `"Rijndael"`, as before.

`ISymmetricCryptographyEngine` is unchanged.
