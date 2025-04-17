#pragma warning disable SA1402
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Digbyswift.Umbraco.OrphanedLinks;

public class OrphanedLinksSettings
{
    public const string SectionName = "Digbyswift:OrphanedLinks";

    [ConfigurationKeyName("Enabled")]
    public bool IsEnabled { get; set; } = true;

    public OrphanedContentLinksSettings Content { get; set; } = new();
    public OrphanedMediaLinksSettings Media { get; set; } = new();
}

public class OrphanedContentLinksSettings
{
    [ConfigurationKeyName("Enabled")]
    public bool IsEnabled { get; set; } = true;

    public IEnumerable<string> IncludedDocTypes { get; set; } = [];
    public IEnumerable<string> ExcludedDocTypes { get; set; } = [];
}

public class OrphanedMediaLinksSettings
{
    [ConfigurationKeyName("Enabled")]
    public bool IsEnabled { get; set; } = true;

    public IEnumerable<string> IncludedMediaTypes { get; set; } = [];
    public IEnumerable<string> ExcludedMediaTypes { get; set; } = [];
}
