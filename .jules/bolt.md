## 2024-05-24 - Compiling dynamic regex patterns inside loops

**Learning:** Compiling dynamic regex patterns inside a loop using `RegexOptions.Compiled` is a severe performance anti-pattern. It destroys performance due to massive GC pressure and repeated IL generation overhead. Dynamic `Regex.Match` calls should be used for dynamic strings to avoid recompilation overhead, OR static strings should be compiled once using static readonly fields.

**Action:** Extract frequently used static regex patterns into `static readonly Regex` fields with `RegexOptions.Compiled` to avoid repeated compilation overhead, especially when parsing natural language or executing inside loops.
