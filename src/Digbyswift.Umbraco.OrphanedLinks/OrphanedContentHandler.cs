using System;
using System.Collections.Generic;
using System.Linq;
using Digbyswift.Core.Constants;
using Digbyswift.Core.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
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
    private readonly OrphanedLinksSettings _orphanedLinksSettings;
    private readonly ILogger _logger;

    private readonly bool _allowDocTypeInclusion;
    private readonly bool _allowDocTypeExclusion;

    public OrphanedContentHandler(
        IOrphanedLinkRepository orphanedLinkRepository,
        IOptions<OrphanedLinksSettings> orphanedLinksSettings,
        IUmbracoContextFactory umbracoContextFactory,
        ILogger<OrphanedContentHandler> logger)
    {
        _orphanedLinkRepository = orphanedLinkRepository;
        _orphanedLinksSettings = orphanedLinksSettings.Value;
        _umbracoContextFactory = umbracoContextFactory;
        _logger = logger;

        // Including doc types takes precedence.
        _allowDocTypeInclusion = _orphanedLinksSettings.Content.IncludedDocTypes.Any();

        // Only allow exclusion of doc types if inclusion of doc types is not allowed.
        _allowDocTypeExclusion = !_allowDocTypeInclusion && _orphanedLinksSettings.Content.ExcludedDocTypes.Any();
    }

    public void Handle(ContentUnpublishingNotification notification)
    {
        var filteredKeys = GetFilteredKeys(notification.UnpublishedEntities);
        HandleImpl(filteredKeys);
    }

    public void Handle(ContentMovingToRecycleBinNotification notification)
    {
        var filteredKeys = GetFilteredKeys(notification.MoveInfoCollection.Select(x => x.Entity));
        HandleImpl(filteredKeys);
    }

    public void Handle(ContentPublishingNotification notification)
    {
        var filteredKeys = GetFilteredKeys(notification.PublishedEntities);
        foreach (var key in filteredKeys)
        {
            try
            {
                _orphanedLinkRepository.Delete(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove content {key} #orphaned-links", key);
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

    private IEnumerable<Guid> GetFilteredKeys(IEnumerable<IContent> contents)
    {
        return contents
            .Where(x =>
                (!_allowDocTypeInclusion && !_allowDocTypeExclusion) ||
                (_allowDocTypeInclusion && _orphanedLinksSettings.Content.IncludedDocTypes.Contains(x.ContentType.Alias)) ||
                (_allowDocTypeExclusion && !_orphanedLinksSettings.Content.ExcludedDocTypes.Contains(x.ContentType.Alias))
            )
            .Select(x => x.Key);
    }
}
