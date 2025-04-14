using Digbyswift.Umbraco.OrphanedLinks.Migration;
using LazyCache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Digbyswift.Umbraco.OrphanedLinks;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddOrphanedLinks(this IUmbracoBuilder builder)
    {
        if (!builder.Config.GetValue("Digbyswift:OrphanedLinks:Enabled", defaultValue: true))
            return builder;

        builder.Components().Append<MigrationComponent>();

        builder

            // Content notifications
            .AddNotificationHandler<ContentUnpublishingNotification, OrphanedContentHandler>()
            .AddNotificationHandler<ContentPublishingNotification, OrphanedContentHandler>()
            .AddNotificationHandler<ContentMovingToRecycleBinNotification, OrphanedContentHandler>()
            .AddNotificationHandler<ContentCacheRefresherNotification, OrphanedContentHandler>()

            // Media notifications
            .AddNotificationHandler<MediaMovingToRecycleBinNotification, OrphanedMediaHandler>()
            .AddNotificationHandler<MediaCacheRefresherNotification, OrphanedMediaHandler>();

        builder.Services.AddSingleton<IOrphanedLinkRepository, OrphanedLinkRepository>();

        // Register default implementation of LazyCache if it
        // hasn't already been registered.
        builder.Services.TryAddSingleton<IAppCache, CachingService>();

        // Register a facade for including the outputting of product
        // redirect original URLs in the redirect management dashboard.
        builder.Services.AddUnique<IPublishedUrlProvider, CustomPublishedUrlProvider>();

        return builder;
    }
}
