## 2024-05-17 - Avoid repeated regex compilation
**Learning:** Compiling dynamic regex patterns inside a loop using `RegexOptions.Compiled` is a severe performance anti-pattern that destroys performance due to massive GC pressure and repeated IL generation overhead.
**Action:** Use `static readonly Regex` instances for fixed patterns (like 'top', 'first', and 'last X days') to eliminate recompilation overhead during parsing. Use standard `Regex.Match` for dynamic strings to avoid `RegexOptions.Compiled` overhead.
