FlashWWWCached
==============

A solution to use LSO's to CacheOrDownload for Unity's Flash Exporter


While Flash doesn't expose the same caching mechanism as for example the Unity Web Player, it is possible to use Flash Local Shared Objects for Asset (bundle) caching. Doing so does require extra code and a UI dialog for the user to confirm the usage of this Flash feature.

The code model for Flash LSO's doesn't fit our WWW.LoadFromCacheOrDownload api perfectly; to do so is possible without a new build of Unity that supports it; to show how and give somewhat of a "reference" implementation for Flash LSO's, I've placed an example on github that shows how to use them to achieve an API like LoadFromCacheOrDownload.

The entire implementation requires you to copy all folders in your project, while the example is only there to show you how to use it.
https://github.com/RalphHauwertUT/FlashWWWCached

Some notes; 

*FlashCachedWWWInitializer
The initial call / yield return on the FlashCachedWWWInitializer is needed. But it's only needed once per application run. If the user hasn't permitted your swf to use local storage yet, this will yield until that dialog is closed. The status might still be that the user denied access, in which case caching will not be used and they will be confronted with the same dialog the next time they run your game. By default it will request 100mb of storage which will automatically be interpreted as "unlimited" by the Flash Player.

*FlashCachedWWW.cs
Emulates most of the functionality of WWW.LoadFromCacheOrDownload; including crc checks, which are heavy. One big change is that it exposes bytes after it's completed, as well as that it actually creates the assetbundle on load. This can be disabled by setting dontAutoCreateAssetbundle on it, in which case you can defer that by using AssetBundle.CreateFromMemory yourself on the exposed bytes property. This will cache any data you load using it, so unlike WWW.LoadFromCacheOrDownload it doesn't only cache assetbundles.

*LSOCache.cs
This does all the interop between ActionScript Shared objects and C# code; you can use this class to implement your own local storage strategy; the classes above are using this as means for storage into the Local Shared Object.

Code mentioned above is provided under MIT licence; is provided as is, works on the current release 4.0 versions of Flash, but this might change over future versions of the Unity Flash export deploy. This is not code part of the Unity 4.0 Flash release and isn't supported as such. However it should be fully usable with the current version of Flash export (4.0.x).

* CRC32.as
Is provided as a means to do CRC32 checking on the downloaded bytes directly in ActionScript; this is needed to mimick the WWW.LoadFromCacheOrDownload. Original C implementation of that by Gary S. Brown.

The ActionScript folder and UnityEngine folder are needed in your project for this to work, while the example is only there as a potential use-case example.
RalphH, Jan 29, 2013 #1
 yesbaba@qq.com
yesbaba@qq.com
Member
Messages:4
the flash version of WWW seams to cache everything it downloads in browser, i test it in chrome and ie8.
so the caching is done automatically, no need to do other work?


In response to WWW method I also found this:


WWW.LoadFromCacheOrDownload is not supported for Flash export. Try downloading your asset bundle using WWW(bundlePath) instead.

If that still doesn't work, make sure you're building your asset bundles for the correct platform (BuildTarget.FlashPlayer).
