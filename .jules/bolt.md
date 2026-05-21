## 2024-05-24 - Pre-compiling static Regex instances
**Learning:** Compiling dynamic Regex patterns with standard strings in loops or frequently invoked parse methods causes significant overhead in NLP components.
**Action:** Extract standard Regex patterns as `static readonly` fields using `RegexOptions.Compiled` where applicable to reuse compiled definitions.
