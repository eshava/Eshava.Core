# Changelog

Notable changes per released version of `Eshava.Core.Validation`, newest first. Versions before
1.0.20 are not documented here — the Git history is the source for those.

## 1.0.20

### Fixed

* **A `Range` bound at or beyond the limits of `long` or `int` no longer throws.** The
  `RangeAttribute(double, double)` constructor stores its bounds as `double`, so
  `[Range(1, Int64.MaxValue)]` carries 2^63 as its maximum — `Int64.MaxValue` rounded up on the way.
  Converting that back with `Convert.ToInt64` threw an `OverflowException` out of
  `ValidationEngine.Validate` instead of producing a validation result. The same happened on an `int`
  property whose bounds do not fit into `int`, for example `[Range(1, Int64.MaxValue)]`.

  Bounds given as `double` or `float` are now clamped to the limits of `long`, and for `int` and enum
  properties additionally to the limits of `int`. The typed form,
  `[Range(typeof(long), "1", "9223372036854775807")]`, is converted as before and remains valid.
