## 2024-05-18 - Compiled Regex for Static Patterns
**Learning:** In highly trafficked parsing classes (like `RuleBasedNaturalLanguageTranslator`), instantiating `Regex` instances or calling `Regex.Match(string, string)` repeatedly within loops creates severe GC pressure and performance bottlenecks due to IL compilation overhead.
**Action:** Always extract performance-critical, static regex patterns into `private static readonly Regex` instances with `RegexOptions.Compiled`. Note: Avoid doing this for dynamic patterns to prevent uncontrolled IL generation and memory leaks.
