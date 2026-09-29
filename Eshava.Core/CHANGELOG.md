# Changelog

Notable changes per released version of `Eshava.Core`, newest first. Versions before 1.0.10 are not
documented here — the Git history is the source for those.

## 1.0.10

### Fixed

* **`StringFactory.GenerateRandomString` draws from a cryptographic random number generator.** It
  used `System.Random`, which is not designed for passwords, secrets or tokens. Every character now
  comes from `RandomNumberGenerator`, mapped onto the alphabet by rejection sampling, so no character
  is more likely than another.
* **The last character of each range is produced again.** The range arithmetic excluded the upper
  bound of the combined ranges, so the default alphanumeric output never contained `z`, numbers only
  never contained `9`, uppercase only never `Z`, and special characters never `~`. The alphanumeric
  alphabet had 61 characters instead of 62.

The signatures, the options and the exceptions are unchanged.
