using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Digbyswift.Umbraco.OrphanedLinks;

public static class UmbracoBuilderExtensions
{
    public static IUmbracoBuilder AddOrphanedLinks(this IUmbracoBuilder builder)
    {
        if (!builder.Config.GetValue("Umbraco:CMS:Integrations:Digbyswift:OrphanedLinks:Enabled", defaultValue: false))
            return builder;

        builder
            .AddNotificationHandler<ContentUnpublishingNotification, OrphanedContentHandler>()
            .AddNotificationHandler<ContentPublishingNotification, OrphanedContentHandler>()
            .AddNotificationHandler<ContentMovingToRecycleBinNotification, OrphanedContentHandler>()
            .AddNotificationHandler<ContentCacheRefresherNotification, OrphanedContentHandler>();

        // Register a facade for including the outputting of product
        // redirect original URLs in the redirect management dashboard.
        builder.Services.AddUnique<IPublishedUrlProvider, CustomPublishedUrlProvider>();

        return builder;
    }
}
