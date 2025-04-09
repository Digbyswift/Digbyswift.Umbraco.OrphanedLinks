using System;
using System.Collections.Generic;
using Digbyswift.Core.Constants;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;

namespace Digbyswift.Umbraco.OrphanedLinks;

/// <summary>
/// Provides URLs.
/// </summary>
public class CustomPublishedUrlProvider : IPublishedUrlProvider
{
    private readonly UrlProvider _urlProvider;
    private readonly IOrphanedLinkRepository _orphanedLinkRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomPublishedUrlProvider"/> class with an Umbraco context and a list of URL providers.
    /// </summary>
    public CustomPublishedUrlProvider(
        IUmbracoContextAccessor umbracoContextAccessor,
        IOptions<WebRoutingSettings> routingSettings,
        UrlProviderCollection urlProviders,
        MediaUrlProviderCollection mediaUrlProviders,
        IVariationContextAccessor variationContextAccessor,
        IOrphanedLinkRepository orphanedLinkRepository)
    {
        _orphanedLinkRepository = orphanedLinkRepository;
        _urlProvider = new UrlProvider(umbracoContextAccessor, routingSettings, urlProviders, mediaUrlProviders, variationContextAccessor);
    }

    /// <summary>
    /// Gets or sets the provider URL mode.
    /// </summary>
    public UrlMode Mode { get; set; }

    #region GetUrl

    public string GetUrl(Guid id, UrlMode mode = UrlMode.Default, string? culture = null, Uri? current = null)
    {
        var workingUrl = _urlProvider.GetUrl(id, mode, culture, current);

        if (!String.IsNullOrWhiteSpace(workingUrl) && workingUrl != StringConstants.Hash)
            return workingUrl;

        var url = _orphanedLinkRepository.Get(id);
        return String.IsNullOrWhiteSpace(url) ? workingUrl : url;
    }

    public string GetUrl(int id, UrlMode mode = UrlMode.Default, string? culture = null, Uri? current = null) => _urlProvider.GetUrl(id, mode, culture, current);
    public string GetUrl(IPublishedContent? content, UrlMode mode = UrlMode.Default, string? culture = null, Uri? current = null) => _urlProvider.GetUrl(content, mode, culture, current);

    public string GetUrlFromRoute(int id, string? route, string? culture)
    {
        var url = _urlProvider.GetUrlFromRoute(id, route, culture);
        if (url == StringConstants.Hash)
        {
            return !String.IsNullOrWhiteSpace(route)
                ? $"{route.TrimEnd(CharConstants.ForwardSlash)}/"
                : url;
        }

        return url;
    }

    #endregion

    #region GetOtherUrls

    public IEnumerable<UrlInfo> GetOtherUrls(int id) => _urlProvider.GetOtherUrls(id);
    public IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current) => _urlProvider.GetOtherUrls(id, current);

    #endregion

    #region GetMediaUrl

    public string GetMediaUrl(Guid id, UrlMode mode = UrlMode.Default, string? culture = null, string propertyAlias = global::Umbraco.Cms.Core.Constants.Conventions.Media.File, Uri? current = null) => _urlProvider.GetMediaUrl(id, mode, culture, propertyAlias, current);
    public string GetMediaUrl(IPublishedContent? content, UrlMode mode = UrlMode.Default, string? culture = null, string propertyAlias = global::Umbraco.Cms.Core.Constants.Conventions.Media.File, Uri? current = null) => _urlProvider.GetMediaUrl(content, mode, culture, propertyAlias, current);

    #endregion
}
