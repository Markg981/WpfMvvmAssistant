## 2024-05-24 - Dynamic Regex Compilation in Loops
**Learning:** Compiling dynamic regex patterns inside a loop using `RegexOptions.Compiled` (or repeatedly running string matchers instead of static compiled patterns) is a severe performance anti-pattern. This destroys performance due to massive GC pressure and repeated IL generation overhead.
**Action:** Extract standard `Regex.Match` calls to `private static readonly Regex` fields with `RegexOptions.Compiled` at the class level to avoid recompilation overhead.
