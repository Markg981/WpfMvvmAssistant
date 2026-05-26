## 2024-05-24 - Compiled Static Regex
**Learning:** In RuleBasedNaturalLanguageTranslator, dynamic string regex matching (Regex.Match with string literals) within frequently called methods (like QueryParser translation loops) creates severe compilation overhead. Benchmarks show this is a common anti-pattern, whereas using static readonly Regex instances with RegexOptions.Compiled avoids this overhead and is ~90% faster.
**Action:** Always extract static regular expressions to compiled class-level static fields when performance is a concern, especially inside loops or parsing logic.
