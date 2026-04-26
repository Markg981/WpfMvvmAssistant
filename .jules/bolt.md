## 2026-04-26 - Static vs Dynamic Regex Compilation
**Learning:** Compiling dynamic regex patterns inside a loop or parser using `RegexOptions.Compiled` without caching the regex object is a performance anti-pattern due to massive GC pressure and repeated IL generation overhead. Caching them as `static readonly Regex` instances completely eliminates this overhead.
**Action:** Always identify performance-critical regex patterns in parsing or loops and move them to `static readonly Regex` instances with `RegexOptions.Compiled`.
