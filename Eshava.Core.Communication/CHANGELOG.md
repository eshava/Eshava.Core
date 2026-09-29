# Changelog

Notable changes per released version of `Eshava.Core.Communication`, newest first. Versions before
1.0.10 are not documented here — the Git history is the source for those.

## 1.0.10

### Changed

* **Depends on `Eshava.Core` 1.0.10**, up from 1.0.9. That version draws the characters of
  `StringFactory.GenerateRandomString` from a cryptographic random number generator and produces the
  last character of each range again — see its changelog. Nothing in this package changed; the
  release carries the new minimum version to consumers that reference `Eshava.Core` only through
  this package.
