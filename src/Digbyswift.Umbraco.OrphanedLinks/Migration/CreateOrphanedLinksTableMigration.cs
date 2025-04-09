using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Digbyswift.Umbraco.OrphanedLinks.Migration;

public sealed class CreateOrphanedLinksTableMigration : MigrationBase
{
    private readonly ILogger _logger;

    public CreateOrphanedLinksTableMigration(
        IMigrationContext context,
        ILogger<CreateOrphanedLinksTableMigration> logger) : base(context)
    {
        _logger = logger;
    }

    protected override void Migrate()
    {
        _logger.LogInformation("Starting {migrationPlan} #migration", nameof(CreateOrphanedLinksTableMigration));

        if (TableExists(OrphanedLink.TableName))
        {
            _logger.LogDebug("Table already exists for {TableName} in {migrationPlan} #migrations", nameof(CreateOrphanedLinksTableMigration), OrphanedLink.TableName);
            _logger.LogInformation("Concluding {migrationPlan} #migration", nameof(CreateOrphanedLinksTableMigration));
            return;
        }

        Create.Table<OrphanedLink>().Do();

        _logger.LogInformation("Concluding {migrationPlan} #migration", nameof(CreateOrphanedLinksTableMigration));
    }
}
