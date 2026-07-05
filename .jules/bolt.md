## 2024-05-24 - [C# Regex Compilation Performance]
**Learning:** Instantiating `Regex` in a local method continuously parses the pattern string, which causes unnecessary overhead.
**Action:** Use `RegexOptions.Compiled` statically to ensure the pattern is compiled to MSIL only once, improving throughput for repeated parsing operations. Avoid this inside loops.
