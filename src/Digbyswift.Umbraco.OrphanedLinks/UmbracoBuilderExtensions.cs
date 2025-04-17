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
        var configSection = builder.Config.GetSection(OrphanedLinksSettings.SectionName);
        var settings = configSection.Get<OrphanedLinksSettings>();
        if (settings is not { IsEnabled: true })
            return builder;

        builder.Services.Configure<OrphanedLinksSettings>(configSection);
        builder.Components().Append<MigrationComponent>();

        if (settings.Content.IsEnabled)
        {
            builder
                .AddNotificationHandler<ContentUnpublishingNotification, OrphanedContentHandler>()
                .AddNotificationHandler<ContentPublishingNotification, OrphanedContentHandler>()
                .AddNotificationHandler<ContentMovingToRecycleBinNotification, OrphanedContentHandler>()
                .AddNotificationHandler<ContentCacheRefresherNotification, OrphanedContentHandler>();
        }

        if (settings.Media.IsEnabled)
        {
            builder
                .AddNotificationHandler<MediaMovingToRecycleBinNotification, OrphanedMediaHandler>()
                .AddNotificationHandler<MediaMovingNotification, OrphanedMediaHandler>()
                .AddNotificationHandler<MediaCacheRefresherNotification, OrphanedMediaHandler>();
        }

        builder.Services.AddSingleton<IOrphanedLinkRepository, OrphanedLinkRepository>();

        // Register default implementation of LazyCache. This is safe
        // because it won't overwrite existing implementations.
        builder.Services.AddLazyCache();

        // Register a facade for including the outputting of product
        // redirect original URLs in the redirect management dashboard.
        builder.Services.AddUnique<IPublishedUrlProvider, CustomPublishedUrlProvider>();

        return builder;
    }
}
