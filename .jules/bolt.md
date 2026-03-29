
## 2024-05-18 - Compiled Regex Patterns for Better Performance
**Learning:** In C#, using `Regex.Match` with inline string patterns within loops inside the `RuleBasedNaturalLanguageTranslator` is a performance anti-pattern due to recompilation overhead for every execution of the loop.
**Action:** Extract frequently used static patterns into `static readonly Regex` instances with `RegexOptions.Compiled` at the class level to parse them once, avoiding repeated recompilation during query parsing.
