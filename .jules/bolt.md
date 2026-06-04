## 2024-05-24 - Optimize static regex patterns
**Learning:** Performance-critical static regex patterns should use `RegexOptions.Compiled` to avoid repeated compilation overhead. Dynamic regex patterns should use standard `Regex.Match` to prevent regex compilation memory leaks.
**Action:** Extract static regex patterns into `static readonly Regex` instances with `RegexOptions.Compiled`. Leave dynamic regex patterns as standard `Regex.Match` calls.
