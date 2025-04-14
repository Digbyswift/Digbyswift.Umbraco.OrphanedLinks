using System;
using System.Collections.Generic;
using System.Linq;
using Digbyswift.Core.Constants;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Digbyswift.Umbraco.OrphanedLinks;

public class OrphanedContentHandler :
    INotificationHandler<ContentUnpublishingNotification>,
    INotificationHandler<ContentPublishingNotification>,
    INotificationHandler<ContentMovingToRecycleBinNotification>,
    INotificationHandler<ContentCacheRefresherNotification>
{
    private readonly IOrphanedLinkRepository _orphanedLinkRepository;
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly ILogger _logger;

    public OrphanedContentHandler(
        IOrphanedLinkRepository orphanedLinkRepository,
        IUmbracoContextFactory umbracoContextFactory,
        ILogger<OrphanedContentHandler> logger)
    {
        _orphanedLinkRepository = orphanedLinkRepository;
        _umbracoContextFactory = umbracoContextFactory;
        _logger = logger;
    }

    public void Handle(ContentUnpublishingNotification notification)
    {
        HandleImpl(notification.UnpublishedEntities.Select(x => x.Key));
    }

    public void Handle(ContentMovingToRecycleBinNotification notification)
    {
        HandleImpl(notification.MoveInfoCollection.Select(x => x.Entity.Key));
    }

    public void Handle(ContentPublishingNotification notification)
    {
        foreach (var key in notification.PublishedEntities.Select(x => x.Key))
        {
            try
            {
                _orphanedLinkRepository.Delete(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove {key} #orphaned-links", key);
            }
        }
    }

    public void Handle(ContentCacheRefresherNotification notification)
    {
        _orphanedLinkRepository.ClearCache();
    }

    private void HandleImpl(IEnumerable<Guid> keys)
    {
        var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();
        if (umbracoContextReference.UmbracoContext.Content == null)
        {
            _logger.LogWarning("Unable to ensure UmbracoContext #orphaned-links");
            return;
        }

        foreach (var key in keys)
        {
            var publishedContent = umbracoContextReference.UmbracoContext.Content.GetById(key);
            if (publishedContent == null)
            {
                _logger.LogWarning("Unable to locate content for {key} #orphaned-links", key);
                continue;
            }

            if (publishedContent.TemplateId is null or 0)
            {
                _logger.LogDebug("Skipping content {key}: No template #orphaned-links", key);
                continue;
            }

            var url = publishedContent.Url(mode: UrlMode.Relative);
            if (url == StringConstants.Hash)
            {
                _logger.LogDebug("Skipping content {key}: No URL #orphaned-links", key);
                continue;
            }

            try
            {
                _orphanedLinkRepository.Add(key, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add content {key} #orphaned-links", key);
            }
        }
    }
}
