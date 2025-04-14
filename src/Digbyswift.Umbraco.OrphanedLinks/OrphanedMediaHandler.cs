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

public class OrphanedMediaHandler :
    INotificationHandler<MediaMovingToRecycleBinNotification>,
    INotificationHandler<MediaCacheRefresherNotification>
{
    private readonly IOrphanedLinkRepository _orphanedLinkRepository;
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly ILogger _logger;

    public OrphanedMediaHandler(
        IOrphanedLinkRepository orphanedLinkRepository,
        IUmbracoContextFactory umbracoContextFactory,
        ILogger<OrphanedMediaHandler> logger)
    {
        _orphanedLinkRepository = orphanedLinkRepository;
        _umbracoContextFactory = umbracoContextFactory;
        _logger = logger;
    }

    public void Handle(MediaMovingToRecycleBinNotification notification)
    {
        HandleImpl(notification.MoveInfoCollection.Select(x => x.Entity.Key));
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
}
