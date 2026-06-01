## 2024-05-15 - Regex Optimization
**Learning:** Dynamic regex compilation causes memory leaks; static pre-compilation yields 90% speedup.
**Action:** Apply RegexOptions.Compiled only to static readonly instances.
