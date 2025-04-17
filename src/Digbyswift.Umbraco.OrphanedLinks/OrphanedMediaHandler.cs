using System;
using System.Collections.Generic;
using System.Linq;
using Digbyswift.Core.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Digbyswift.Umbraco.OrphanedLinks;

public class OrphanedMediaHandler :
    INotificationHandler<MediaMovingToRecycleBinNotification>,
    INotificationHandler<MediaMovingNotification>,
    INotificationHandler<MediaCacheRefresherNotification>
{
    private readonly IOrphanedLinkRepository _orphanedLinkRepository;
    private readonly OrphanedLinksSettings _orphanedLinksSettings;
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly ILogger _logger;

    private readonly bool _allowMediaTypeInclusion;
    private readonly bool _allowMediaTypeExclusion;

    public OrphanedMediaHandler(
        IOrphanedLinkRepository orphanedLinkRepository,
        IOptions<OrphanedLinksSettings> orphanedLinksSettings,
        IUmbracoContextFactory umbracoContextFactory,
        ILogger<OrphanedMediaHandler> logger)
    {
        _orphanedLinkRepository = orphanedLinkRepository;
        _orphanedLinksSettings = orphanedLinksSettings.Value;
        _umbracoContextFactory = umbracoContextFactory;
        _logger = logger;

        // Including doc types takes precedence.
        _allowMediaTypeInclusion = _orphanedLinksSettings.Media.IncludedMediaTypes.Any();

        // Only allow exclusion of doc types if inclusion of doc types is not allowed.
        _allowMediaTypeExclusion = !_allowMediaTypeInclusion && _orphanedLinksSettings.Media.ExcludedMediaTypes.Any();
    }

    public void Handle(MediaMovingToRecycleBinNotification notification)
    {
        var filteredKeys = GetFilteredKeys(notification.MoveInfoCollection.Select(x => x.Entity));
        HandleImpl(filteredKeys);
    }

    public void Handle(MediaMovingNotification notification)
    {
        var filteredKeys = GetFilteredKeys(notification.MoveInfoCollection.Where(x => x.Entity.Trashed).Select(x => x.Entity));
        foreach (var key in filteredKeys)
        {
            try
            {
                _orphanedLinkRepository.Delete(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove media {key} #orphaned-links", key);
            }
        }
    }

    public void Handle(MediaCacheRefresherNotification notification)
    {
        _orphanedLinkRepository.ClearCache();
    }

    private void HandleImpl(IEnumerable<Guid> keys)
    {
        var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();
        if (umbracoContextReference.UmbracoContext.Media == null)
        {
            _logger.LogWarning("Unable to ensure UmbracoContext #orphaned-links");
            return;
        }

        foreach (var key in keys)
        {
            var media = umbracoContextReference.UmbracoContext.Media.GetById(key);
            if (media == null)
            {
                _logger.LogWarning("Unable to locate media for {key} #orphaned-links", key);
                continue;
            }

            var url = media.MediaUrl(mode: UrlMode.Relative);
            if (url == StringConstants.Hash)
            {
                _logger.LogDebug("Skipping media {key}: No URL #orphaned-links", key);
                continue;
            }

            try
            {
                _orphanedLinkRepository.Add(key, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add media {key} #orphaned-links", key);
            }
        }
    }

    private IEnumerable<Guid> GetFilteredKeys(IEnumerable<IMedia> contents)
    {
        return contents
            .Where(x =>
                (!_allowMediaTypeInclusion && !_allowMediaTypeExclusion) ||
                (_allowMediaTypeInclusion && _orphanedLinksSettings.Media.IncludedMediaTypes.Contains(x.ContentType.Alias)) ||
                (_allowMediaTypeExclusion && !_orphanedLinksSettings.Media.ExcludedMediaTypes.Contains(x.ContentType.Alias))
            )
            .Select(x => x.Key);
    }
}
