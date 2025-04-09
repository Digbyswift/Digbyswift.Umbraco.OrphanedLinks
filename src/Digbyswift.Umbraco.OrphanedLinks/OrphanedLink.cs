using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Digbyswift.Umbraco.OrphanedLinks;

[TableName(TableName)]
[PrimaryKey([nameof(NodeKey)], AutoIncrement = false)]
public record OrphanedLink
{
    public const string TableName = "dsOrphanedLinks";

    [PrimaryKeyColumn(AutoIncrement = false)]
    public Guid NodeKey { get; set; }

    [Length(1000)]
    public string? Value { get; set; }

    [Constraint(Name = "DF_dsOrphanedLinks_DateAdded", Default = "GETDATE()")]
    public DateTime DateAdded { get; set; }
}
