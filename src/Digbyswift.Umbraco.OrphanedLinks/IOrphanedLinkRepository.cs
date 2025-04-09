using System;

namespace Digbyswift.Umbraco.OrphanedLinks;

public interface IOrphanedLinkRepository
{
    string? Get(Guid key);
    void Add(Guid key, string value);
    void Delete(Guid key);
    void ClearCache();
}
