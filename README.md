# Digbyswift.Umbraco.OrphanedLinks

[![NuGet version (Digbyswift.Umbraco.OrphanedLinks)](https://img.shields.io/nuget/v/Digbyswift.Umbraco.OrphanedLinks.svg)](https://www.nuget.org/packages/Digbyswift.Umbraco.OrphanedLinks/)
[![Build Status](https://dev.azure.com/digbyswift/Digbyswift%20-%20NuGet%20Packages/_apis/build/status%2FDigbyswift.Digbyswift.Umbraco.OrphanedLinks?branchName=master)](https://dev.azure.com/digbyswift/Digbyswift%20-%20NuGet%20Packages/_build/latest?definitionId=57&branchName=master)

Prevents broken links in Umbraco CMS rich text editors when linked content is unpublished.

## Why?

Links to Umbraco based content in a rich text editor (RTE) are saved in a format using the content's unique
key, e.g. `<a href="/{localLink:umb://document/13ce96ec83fe47fbb82e5dcca071cf2d}">`. This allows the CMS to
maintain references to the linked content regardless of where it is moved to.

However, if a linked page is unpublished in the Umbraco CMS, the link in the RTE is not preserved by Umbraco.
In a standard install, in the published RTE content you will see the link becoming a `href="#"`.

This package will persist the URLs so that links are preserved. This has the following benefits:

 - Redirects can be implemented when the link is broken but not when the link is a `#`.
 - Broken links are easier to find, either by search engines or by automated scraping tools;

## How?

It does this by saving content URLs to a database table upon the following Umbraco events:

 - `ContentUnpublishingNotification` and
 - `ContentMovingToRecycleBinNotification`

It will remove URLs from the DB upon `ContentPublishingNotification`

In-memory cache is used courtesy of [LazyCache](https://github.com/alastairtree/LazyCache) and is updated upon `ContentCacheRefresherNotification` and so will work in a both a single instance and a load-balanced setup.


## Setup

Include the `AddOrphanedLinks()` extension method in the startup of the project:

```csharp
var umbracoBuilder = builder
    .CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .AddOrphanedLinks()
```


## Config

By default the package is enabled and assumes the following optional configuration:

```json
{
  "Digbyswift": {
    "OrphanedLinks": {
      "Enabled": true,
      "Content": {
        "Enabled": true,
        "IncludedDocTypes": [],
        "ExcludedDocTypes": []
      },
      "Media": {
        "Enabled": true,
        "IncludedMediaTypes": [],
        "ExcludedMediaTypes": []
      }
    }
  }
}
```

### Included/excluded types

The following rules apply:

 - Setting one or more included types for media or content will result in any excluded types being ignored.
 - Setting `[]` for included types (or not providing this setting) is the equivalent of including all types.
 - Setting `[]` for excluded types (or not providing this setting) is the equivalent of excluding all types.
