## 2024-05-24 - Regex Options Compiled pattern
**Learning:** In the `RuleBasedNaturalLanguageTranslator`, there are dynamically compiled patterns using interpolation (like `columnName`) mixed with static expressions. Compiling dynamic regex patterns inside a loop using `RegexOptions.Compiled` is a performance anti-pattern due to recompilation overhead.
**Action:** Always use `static readonly Regex` instances with `RegexOptions.Compiled` for static patterns (like 'last X days', 'top', 'first'), but fall back to standard `Regex.Match` calls for purely dynamic strings to avoid overhead.
