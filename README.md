`VersionedFileProvider` is a simple and lightweight `IFileProvider` wrapper for ASP.NET Core that allows you to remove or transform certain part of the file path.
The package also expose a more generalized `IPathTransformFileProvider` and `BasePathTransformFileProvider` if you want a different path transformation.

![screenshot](https://i.imgur.com/H2RO7Po.png)

# Why?

ASP.NET Core provides `asp-append-version` attribute so you can "drop" file cache (either browser cache or server cache or even services like Cloudflare cache) of CSS and Javascript files. However, with ES6 module `import`, this does not work:

```
// Content of /js/app.js

import { Common } from "./../common.js"; // Cannot use asp-append-version here
```

In development machine, it is not a problem to disable cache. However, for production environment, there is no simple way to clear it without modifying every Javascript file. That's why I think of appending a manual version on the top file path only:

```
<script type="module" src="~/js/v1/app.js" asp-append-version="true"></script>
```

This way, when you increase the version in the path, all the modules' paths are updated accordingly. However you need a way to serve such files when in reality, the server file system only contains `/js` folder.

# Installation

[Nuget package](https://www.nuget.org/packages/BibliTech.VersionedFileProvider/):

```
Install-Package BibliTech.VersionedFileProvider
```

Note: the library itself is very simple. If you don't want to include external reference, simply copy the content of [VersionedFileProvider.cs](https://github.com/BibliTech/VersionedFileProvider/blob/master/BibliTech.VersionedFileProvider/VersionedFileProvider.cs) and [BasePathTransformFileProvider.cs](https://github.com/BibliTech/VersionedFileProvider/blob/master/BibliTech.VersionedFileProvider/BasePathTransformFileProvider.cs).

# Usage

You can check the [Demo project](https://github.com/BibliTech/VersionedFileProvider/tree/master/BibliTech.VersionedFileProvider.Demo) to see how it is used.

## Set as File Provider

`VersionedFileProvider` accepts an underlying `IFileProvider`. You can also use a folder path string, it will automatically create a `PhysicalFileProvider` for that folder. In usual case, you will want to use `WebRootFileProvider` for that.

```
var versionedFileProvider = new VersionedFileProvider(env.WebRootFileProvider);
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = versionedFileProvider,
});
```

## Set the WebRootFileProvider

If you want `asp-append-version` to work with this custom FileProvider, you will need to set it to `IWebHostEnvironment.WebRootFileProvider`. [It is confirmed by the ASP.NET Core team](https://github.com/aspnet/AspNetCore/issues/17409).

```
env.WebRootFileProvider = versionedFileProvider;
```

# Custom Version Expression or Transformation

By default, `VersionedFileProvider` uses a `RegEx` and simply remove every path segment that matches with it:

```
public Regex VersionRegex { get; set; } = new Regex(@"^v[0-9\.]+");
```

The default version match anything that starts with v and follows by 0-9 and dot (.) character, so your path is safe if you happen to have something like `version` or `variant`. You can change the `VersionRegex` property, or implement a `FileProvider` yourself by extending `BasePathTransformFileProvider` and override `TransformSubpath` method.
