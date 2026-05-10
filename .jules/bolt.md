
## 2024-05-24 - Optimization Reversion and Regex Compilation
**Learning:** Replacing `string.Contains` with a pre-tokenized `HashSet` for O(1) lookups broke NLP semantics by preventing matches on partial words or phrases containing spaces. However, extracting dynamic regex instantiations into `static readonly Regex` fields with `RegexOptions.Compiled` remains a safe and effective optimization.
**Action:** Always ensure that O(1) lookups using HashSets do not break required partial matching or substring matching behaviors. Proceed with pre-compiled regex fields for safe performance wins.
