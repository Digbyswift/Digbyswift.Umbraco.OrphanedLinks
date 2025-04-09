using Digbyswift.Umbraco.OrphanedLinks.Migration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Digbyswift.Umbraco.OrphanedLinks;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddOrphanedLinks(this IUmbracoBuilder builder)
    {
        if (!builder.Config.GetValue("Digbyswift:OrphanedLinks:Enabled", defaultValue: false))
            return builder;

        builder.Components().Append<MigrationComponent>();

        builder
            .AddNotificationHandler<ContentUnpublishingNotification, OrphanedContentHandler>()
            .AddNotificationHandler<ContentPublishingNotification, OrphanedContentHandler>()
            .AddNotificationHandler<ContentMovingToRecycleBinNotification, OrphanedContentHandler>()
            .AddNotificationHandler<ContentCacheRefresherNotification, OrphanedContentHandler>();

        builder.Services.AddSingleton<IOrphanedLinkRepository, OrphanedLinkRepository>();

        // Register a facade for including the outputting of product
        // redirect original URLs in the redirect management dashboard.
        builder.Services.AddUnique<IPublishedUrlProvider, CustomPublishedUrlProvider>();

        return builder;
    }
}
