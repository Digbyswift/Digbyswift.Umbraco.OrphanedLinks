using System;
using System.Collections.Generic;
using System.Linq;
using LazyCache;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Digbyswift.Umbraco.OrphanedLinks;

internal class OrphanedLinkRepository : IOrphanedLinkRepository
{
    private const string OrphanedLinksCacheKey = "OrphanedLinks";

    private readonly IScopeProvider _scopeProvider;
    private readonly IAppCache _appCache;
    private readonly ILogger _logger;

    public OrphanedLinkRepository(IScopeProvider scopeProvider, IAppCache appCache, ILogger<OrphanedLinkRepository> logger)
    {
        _scopeProvider = scopeProvider;
        _appCache = appCache;
        _logger = logger;
    }

    public string? Get(Guid key)
    {
        return Get().GetValueOrDefault(key);
    }

    public void Add(Guid key, string value)
    {
        if (String.IsNullOrWhiteSpace(value))
            return;

        const string upsertSql = """
             INSERT INTO [dbo].[dsOrphanedLinks] ([NodeKey], [Value])
             SELECT @key, @value
             WHERE NOT EXISTS
             (
                 SELECT 1 FROM [dbo].[dsOrphanedLinks] WITH (UPDLOCK, SERIALIZABLE)
                 WHERE [NodeKey] = @key
             );
             """;

        using var scope = _scopeProvider.CreateScope();
        scope.Database.Execute(upsertSql, new { key, value });
        scope.Complete();
    }

    public void Delete(Guid key)
    {
        const string deleteSql = "DELETE [dbo].[dsOrphanedLinks] WHERE [NodeKey] = @key";

        using var scope = _scopeProvider.CreateScope();
        scope.Database.Execute(deleteSql, new { key });
        scope.Complete();
    }

    public void ClearCache()
    {
        _appCache.Remove(OrphanedLinksCacheKey);
    }

    private Dictionary<Guid, string> Get()
    {
        const string fetchSql = "SELECT * FROM [dbo].[dsOrphanedLinks]";

        try
        {
            return _appCache.GetOrAdd(OrphanedLinksCacheKey, () =>
            {
                using var scope = _scopeProvider.CreateScope();
                var results = scope.Database.Fetch<OrphanedLink>(fetchSql);
                scope.Complete();

                return results.Where(x => !String.IsNullOrWhiteSpace(x.Value)).ToDictionary(x => x.NodeKey, x => x.Value!);
            }) ?? new Dictionary<Guid, string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cache #orphaned-links");

            return [];
        }
    }
}
