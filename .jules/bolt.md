## 2024-05-14 - Regex Optimization in NLP Query Parsing
**Learning:** Instantiating `Regex` dynamically via static `Regex.Match` calls in parsing logic (like `RuleBasedNaturalLanguageTranslator`) incurs a performance penalty due to repeated pattern parsing, even if internally cached.
**Action:** Always pre-compile static Regex patterns used in hot paths using `static readonly Regex` with `RegexOptions.Compiled` to optimize execution speed.
