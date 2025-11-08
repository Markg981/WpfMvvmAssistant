using Core.Domain.Models;

namespace Core.Nlp.Interfaces;

public interface INaturalLanguageQueryTranslator
{
    Task<NaturalLanguageQueryResult> TranslateAsync(
        NaturalLanguageQueryRequest request,
        CancellationToken cancellationToken = default);

    bool IsAvailable();
    
    string GetProviderName();
}
