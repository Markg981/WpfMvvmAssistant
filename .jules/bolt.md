## 2024-05-24 - Static Regex Compilation
**Learning:** Compiling static regex patterns like 'top', 'first', and 'last X days' using `RegexOptions.Compiled` avoids repeated compilation overhead and is ~90% faster than dynamic `Regex.Match` calls. However, dynamic patterns that inject variables must remain uncompiled to prevent memory leaks.
**Action:** Always extract static regex patterns into `static readonly Regex` instances with `RegexOptions.Compiled`.
