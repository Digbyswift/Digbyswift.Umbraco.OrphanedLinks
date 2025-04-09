using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Digbyswift.Umbraco.OrphanedLinks.Migration;

public sealed class MigrationComponent : IComponent
{
    private readonly IMigrationPlanExecutor _migrationPlanExecutor;
    private readonly IScopeProvider _scopeProvider;
    private readonly IKeyValueService _keyValueService;

    public MigrationComponent(
        IMigrationPlanExecutor migrationPlanExecutor,
        IScopeProvider scopeProvider,
        IKeyValueService keyValueService)
    {
        _migrationPlanExecutor = migrationPlanExecutor;
        _scopeProvider = scopeProvider;
        _keyValueService = keyValueService;
    }

    public void Initialize()
    {
        new Upgrader(new OrphanedLinksMigrationPlan()).Execute(_migrationPlanExecutor, _scopeProvider, _keyValueService);
    }

    public void Terminate()
    {
    }
}
