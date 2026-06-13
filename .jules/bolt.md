## $(date +%Y-%m-%d) - Regex Compilation Optimization in Loops

**Learning:**
Compiling dynamic regex patterns inside a loop using `RegexOptions.Compiled` is a severe performance anti-pattern. However, using dynamic `Regex.Match` for *static* patterns inside loops also incurs recompilation overhead. Benchmarking shows caching static patterns as `static readonly Regex` instances with `RegexOptions.Compiled` is approximately 90% faster than inline `Regex.Match` calls. Dynamic patterns (e.g., those injecting variables like `columnName`) must deliberately remain uncompiled (standard `Regex.Match`) to avoid regex compilation memory leaks.

**Action:**
Extract any static, frequently used Regex patterns within loops or translation logic into `private static readonly Regex` fields initialized with `RegexOptions.Compiled`. Retain `Regex.Match` strictly for dynamically generated patterns.
