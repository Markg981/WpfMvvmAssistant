## 2024-05-18 - Avoid dynamic Regex recompilation inside loops

**Learning:** Recompiling constant Regex strings in loops causes massive GC pressure and repeated IL generation overhead, destroying performance. Using `RegexOptions.Compiled` makes it fast, but should only be done for *static* patterns that don't depend on loop variables.
**Action:** Extract constant regex patterns like `"last\\s+(\\d+)\\s+days"` or `"top\\s+(\\d+)"` into `static readonly Regex` fields with `RegexOptions.Compiled`. Continue using regular `Regex.Match` (without `RegexOptions.Compiled`) for dynamic strings (e.g., those containing variable table or column names) to avoid bloating the memory with compiled regex delegates.
