## 2024-05-24 - [Compiled Regex Memory Leaks]
**Learning:** Dynamic regex patterns inside loops must not use RegexOptions.Compiled due to memory leaks, while static performance-critical regexes should be compiled.
**Action:** Always use Regex.Match for dynamic strings inside loops, and use static readonly Regex with RegexOptions.Compiled for static patterns.
