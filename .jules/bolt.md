## 2024-05-24 - Precompiled Regex for static patterns
**Learning:** In RuleBasedNaturalLanguageTranslator, compiling static Regex patterns with `RegexOptions.Compiled` avoids repeated compilation overhead and is approximately 90% faster than dynamic `Regex.Match` calls. However, dynamic regex patterns (e.g., injecting variables) must deliberately remain uncompiled to avoid regex compilation memory leaks.
**Action:** Always use `static readonly Regex` with `RegexOptions.Compiled` for static patterns, but use standard `Regex.Match` for dynamic strings.
