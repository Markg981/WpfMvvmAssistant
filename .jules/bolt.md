
## 2024-03-25 - Regex Compilation Overhead
**Learning:** In `RuleBasedNaturalLanguageTranslator`, performance-critical regex patterns (like 'top', 'first', and 'last X days') should be implemented as `static readonly Regex` instances with `RegexOptions.Compiled` to avoid repeated compilation overhead. Benchmarking showed this approach is approximately 90% faster than dynamic `Regex.Match` calls. However, compiling dynamic regex patterns inside a loop using `RegexOptions.Compiled` is a performance anti-pattern. Standard `Regex.Match` calls should be used for dynamic strings to avoid severe recompilation overhead.
**Action:** Always precompile static regexes in high-throughput paths, but avoid compiling dynamic regexes generated within loops.

## 2024-03-25 - Project File Dependencies
**Learning:** Adding new dependencies or bumping major versions of packages (like `Microsoft.Data.SqlClient`) in `.csproj` files to fix test execution environment issues should be avoided unless explicitly required by the primary task. This contaminates the domain logic with database-specific concerns and risks architectural breakage, as pointed out during code review.
**Action:** Do not alter `.csproj` dependencies simply to fix test compilation issues that result from missing target dependencies unless approved or necessary for the main PR objective. Ensure domain layers remain infrastructure-agnostic.
