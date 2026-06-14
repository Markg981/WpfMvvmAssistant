## 2024-05-18 - Regex Compilation in NLP Translator
**Learning:** Performance-critical regex patterns that are static should be compiled using RegexOptions.Compiled, whereas dynamic patterns should remain uncompiled to avoid memory leaks.
**Action:** Extract static patterns like "top", "first", and "last X days" into static readonly Regex fields with RegexOptions.Compiled.
