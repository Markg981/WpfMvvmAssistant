## 2024-07-07 - [Regex Compilation Strategy]
**Learning:** Compiling dynamic regex patterns inside a loop using RegexOptions.Compiled is a severe performance anti-pattern and memory leak risk. Only static regex patterns should be compiled, while dynamic strings should use standard Regex.Match to avoid recompilation overhead.
**Action:** Extract static regex patterns (like 'top', 'first', 'last X days') to static readonly Regex instances with RegexOptions.Compiled, but deliberately leave dynamic regex patterns (like those injecting variables) uncompiled.
