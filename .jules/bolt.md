## 2024-05-30 - Repeated Regex Compilation Overhead
**Learning:** Found a performance bottleneck where `RuleBasedNaturalLanguageTranslator.QueryParser` instantiates and compiles complex regexes (like "last X days", "top X", "first X") dynamically on every query parse call.
**Action:** Extracted these regexes into `static readonly Regex` instances using `RegexOptions.Compiled` to avoid repeated compilation overhead and garbage collection pressure.
