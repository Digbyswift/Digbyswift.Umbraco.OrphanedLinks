using System;
using Umbraco.Cms.Infrastructure.Migrations;

namespace Digbyswift.Umbraco.OrphanedLinks.Migration;

public class OrphanedLinksMigrationPlan : MigrationPlan
{
    public OrphanedLinksMigrationPlan() : base("Digbyswift.Umbraco.OrphanedLinks")
    {
        From(String.Empty)
            .To<CreateOrphanedLinksTableMigration>("3f273f2d-5023-492c-91e0-9ae43f0c6205");
    }
}
