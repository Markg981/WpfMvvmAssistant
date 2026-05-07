## 2024-05-24 - Pre-compiled Regexes in QueryParser

**Learning:** Re-compiling regular expressions inside iterative logic or query parser components (`Regex.Match(input, pattern)`) adds unnecessary CPU usage and GC pressure due to IL compilation.

**Action:** Always pre-compile frequently used static regular expression patterns as `static readonly Regex` fields using `RegexOptions.Compiled` to avoid repeated compilation and improve performance.
