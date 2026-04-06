## 2024-05-24 - Pre-Compiled Regex Performance
**Learning:** In the `RuleBasedNaturalLanguageTranslator`, performance-critical regex patterns (like 'top', 'first', and 'last X days') were implemented as dynamic `Regex.Match` calls. Benchmarking showed that using `static readonly Regex` instances with `RegexOptions.Compiled` is approximately 90% faster than dynamic calls.
**Action:** When working with frequently executed Regex patterns in loops or critical paths, use `static readonly Regex` with `RegexOptions.Compiled` to avoid repeated compilation overhead.
