## 2024-06-12 - Regex Compilation Anti-Pattern in C#

**Learning:** Compiling dynamic regex patterns inside a loop using `RegexOptions.Compiled` (or calling `Regex.Match` repeatedly with a static string in a loop without compiling) is a severe performance anti-pattern. It destroys performance due to massive GC pressure and repeated IL generation overhead.
**Action:** Always extract static regex patterns into `static readonly Regex` instances with `RegexOptions.Compiled` at the class level. For truly dynamic patterns that change based on loop variables, DO NOT use `RegexOptions.Compiled`.
