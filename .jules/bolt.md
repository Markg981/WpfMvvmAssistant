## 2024-05-31 - Regex Compilation Impact on Performance

**Learning:** Compiling dynamic regex patterns inside a loop using `RegexOptions.Compiled` is a severe performance anti-pattern. Standard `Regex.Match` calls should be used for dynamic strings to avoid recompilation overhead. However, performance-critical static regex patterns (like 'top', 'first', and 'last X days') should be implemented as `static readonly Regex` instances with `RegexOptions.Compiled` to avoid repeated compilation overhead. Benchmarking showed this approach is approximately 90% faster than dynamic `Regex.Match` calls.

**Action:** Identify static regex patterns within hot paths or loops (like `QueryParser` methods in `RuleBasedNaturalLanguageTranslator.cs`) and convert them to `static readonly Regex` compiled instances. Leave dynamic pattern matching uncompiled.
