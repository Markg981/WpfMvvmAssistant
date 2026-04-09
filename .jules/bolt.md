## 2024-05-24 - Pre-compiled Regex Options
**Learning:** Compiling regex patterns (`RegexOptions.Compiled`) is very effective for static strings instantiated once globally but dynamic strings matching exact inline patterns within loops are incredibly inefficient if instantiated every time. Pre-compiled regex instances save ~90% parsing overhead in such loops.
**Action:** Extract inline constant/static regex patterns inside classes that are frequently instantiated into `private static readonly Regex` class fields to reap the performance benefits without compilation overhead.
