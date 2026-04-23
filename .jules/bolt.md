## 2024-05-24 - Compile Regex patterns for NLP parsing
**Learning:** Using `RegexOptions.Compiled` on dynamic regex patterns inside a loop creates massive GC pressure and repeated IL generation overhead, destroying performance.
**Action:** Extract these performance-critical regex strings to `private static readonly Regex` fields initialized with `RegexOptions.Compiled` to avoid repeated compilation overhead.
