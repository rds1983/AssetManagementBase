### AssetManagementBase
[![NuGet](https://img.shields.io/nuget/v/AssetManagementBase.svg)](https://www.nuget.org/packages/AssetManagementBase/) [![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

AssetManagementBase is a basic C# asset management library that isn't tied to any particular game engine. It supports loading custom asset types with automatic caching and **recursive asset loading** — allowing assets to reference and load other assets seamlessly.

### Adding Reference
https://www.nuget.org/packages/AssetManagementBase
    
### Creating AssetManager
Creating AssetManager that loads files:
```c#
AssetManager assetManager = AssetManager.CreateFileAssetManager(@"c:\MyGame\Assets");
```

Creating AssetManager that loads resources:
```c#
AssetManager assetManager = AssetManager.CreateResourceAssetManager(_assembly, "Resources");
```
If _assembly's name is "Assembly.Name" then the above code will create AssetManager that loads resources with prefix "Assembly.Name.Prefix.".

If you don't want the assembly's name prepended to the prefix, pass 'false' as the third parameter:
```c#
AssetManager assetManager = AssetManager.CreateResourceAssetManager(_assembly, "Full.Path.Resources", false);
```

### Loading Assets
After AssetManager is created, it can be used as follows:
```c#
string data = assetManager.LoadString("data/mydata.txt");
```

### Asset Path Resolution
AMB supports flexible path resolution to make asset references easy and relative-path friendly:

1. **Relative Paths** (e.g., `"config/settings.xml"`): Resolved relative to the current asset's folder context. This enables recursive loading where nested assets can reference sibling assets.

2. **Rooted Paths from Base** (e.g., `"/config/settings.xml"`): Always resolved from the base asset folder, regardless of current context.

3. **Explicit File System Paths** (e.g., `"@C:\Assets\config.xml"`): Start with `@` to use absolute file system paths.

4. **Path Normalization**: 
   - Backslashes are normalized to forward slashes
   - `..` sequences are resolved to navigate to parent folders
   - All paths are processed to their canonical form

**Example path resolutions:**
- `assetManager.LoadAsset("data/profile.xml")` — loads relative to current folder
- `assetManager.LoadAsset("/shared/defaults.xml")` — loads from base folder
- `assetManager.LoadAsset("../common/util.xml")` — navigates up one level
- `assetManager.LoadAsset("@C:\Assets\external.xml")` — loads from absolute path

### Custom Asset Types

#### Basic Example
This guide demonstrates how to expand AssetManager with custom loader methods. Let's define a `UserProfile` class:

```c#
public class UserProfile
{
    public string Name;
    public int Score;
}
```

Stored in XML format:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<UserProfile>
  <Name>AssetManagementBase</Name>
  <Score>10000</Score>
</UserProfile>
```

Create an extension method with a loader delegate:
```c#
using System.Xml.Linq;

namespace AssetManagementBase.Tests
{
	public static class AssetManagerExtensions
	{
		private static AssetLoader<UserProfile> _userProfileLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.ReadAsString(assetName);
			var xDoc = XDocument.Parse(data);

			var result = new UserProfile
			{
				Name = xDoc.Root.Element("Name").Value,
				Score = int.Parse(xDoc.Root.Element("Score").Value)
			};

			return result;
		};

		public static UserProfile LoadUserProfile(this AssetManager assetManager, string assetName) => 
			assetManager.UseLoader(_userProfileLoader, assetName);
	}
}
```

Now you can load the profile:
```c#
UserProfile userProfile = assetManager.LoadUserProfile("profile.xml");
```

#### Advanced Example: Recursive Asset Loading

AMB enables recursive loading where assets can reference and load other assets. Consider an `Employee` class that references a `Job` class:

**Job.cs** — The referenced type:
```c#
public class Job
{
    public string Title;
    public decimal Salary;
}
```

**Employee.cs** — The referencing type:
```c#
public class Employee
{
    public string Name;
    public string JobPath;  // Path to the Job XML file
    public Job Job { get; set; }  // Loaded recursively
}
```

**job.xml** — Job asset file:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Job>
  <Title>Senior Developer</Title>
  <Salary>120000</Salary>
</Job>
```

**employee.xml** — Employee asset file that references job.xml:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Employee>
  <Name>John Smith</Name>
  <JobPath>job.xml</JobPath>
</Employee>
```

**Asset loader extensions with recursive loading:**
```c#
using System.Xml.Linq;

namespace AssetManagementBase.Tests
{
	public static class AssetManagerExtensions
	{
		private static AssetLoader<Job> _jobLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.ReadAsString(assetName);
			var xDoc = XDocument.Parse(data);

			var result = new Job
			{
				Title = xDoc.Root.Element("Title").Value,
				Salary = decimal.Parse(xDoc.Root.Element("Salary").Value)
			};

			return result;
		};

		private static AssetLoader<Employee> _employeeLoader = (manager, assetName, settings, tag) =>
		{
			var data = manager.ReadAsString(assetName);
			var xDoc = XDocument.Parse(data);

			var jobPath = xDoc.Root.Element("JobPath").Value;

			var result = new Employee
			{
				Name = xDoc.Root.Element("Name").Value,
				JobPath = jobPath,
				// Recursive load: the 'manager' parameter provides context-aware asset loading
				Job = manager.LoadJob(jobPath)
			};

			return result;
		};

		public static Job LoadJob(this AssetManager assetManager, string assetName) => 
			assetManager.UseLoader(_jobLoader, assetName);

		public static Employee LoadEmployee(this AssetManager assetManager, string assetName) => 
			assetManager.UseLoader(_employeeLoader, assetName);
	}
}
```

**Usage:**
```c#
// Load employee - this automatically loads the referenced Job asset
Employee employee = assetManager.LoadEmployee("employee.xml");

Console.WriteLine($"Employee: {employee.Name}");
Console.WriteLine($"Job Title: {employee.Job.Title}");
Console.WriteLine($"Salary: ${employee.Job.Salary}");

// Both assets are cached, so subsequent loads are instant
Employee sameEmployee = assetManager.LoadEmployee("employee.xml");
```

**Key Points on Recursive Loading:**
- The `manager` parameter passed to the loader provides a context-aware instance. When loading nested assets, relative paths (like `"job.xml"`) are resolved relative to the containing asset's folder.
- Caching is automatic and shared across all nested loads, preventing duplicate asset loading.
- This pattern scales to arbitrary nesting levels — jobs can reference departments, employees can reference projects, etc.

### Settings-Aware Asset Loading

IAssetSettings enables loading the same asset file in different ways, with each variant cached separately. This is useful when you need to load an asset with different processing rules, transformations, or parameters while maintaining independent cache entries.

#### Practical Example: Image Loading with Optional Color Replacement

Consider a scenario where you load an image asset but sometimes need to replace a specific color (e.g., magenta) with transparency for UI elements. The same image file can be loaded two ways: with the color replacement applied or without it. Each variant is cached separately.

**Concept:**
- You have an image file (e.g., `icon.png`) with magenta (`#FF00FF`) as a placeholder color
- **Without settings**: The image loads as-is with magenta intact
- **With ColorReplacementSettings**: The loader detects magenta pixels and replaces them with transparency (alpha = 0)
- Both variants are cached independently under different cache keys

This allows you to:
1. Load UI icons where magenta represents "transparent" in the source file
2. Load game sprites where a specific color indicates "ignore this"
3. Reuse the same image asset for different visual purposes without storing multiple files

**Image.cs** — The image data type:
```c#
public class Image
{
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[] PixelData { get; set; }  // RGBA format
}
```

**ColorReplacementSettings.cs** — Settings for color-to-transparent conversion:
```c#
public class ColorReplacementSettings : IAssetSettings
{
    /// <summary>
    /// RGBA color to replace. Null means no replacement (loads image as-is).
    /// Format: 0xAA BB GG RR (little-endian)
    /// </summary>
    public uint? ColorToReplace { get; }

    public static ColorReplacementSettings None { get; } = new ColorReplacementSettings(null);

    public ColorReplacementSettings(uint? colorToReplace)
    {
        ColorToReplace = colorToReplace;
    }

    public string BuildKey()
    {
        if (ColorToReplace == null)
            return "color_replacement=none";
        return $"color_replacement={ColorToReplace:X8}";
    }
}
```

**Asset loader with color replacement:**
```c#
using StbImageSharp;

namespace AssetManagementBase.Sample
{
    public static class AssetManagerExtensions
    {
        private static AssetLoader<Image> _imageLoader = (manager, assetName, settings, tag) =>
        {
            using (var stream = manager.Open(assetName))
            {
                // Load image using StbImageSharp
                var stbImage = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                var pixelData = new byte[stbImage.Data.Length];
                Array.Copy(stbImage.Data, pixelData, stbImage.Data.Length);

                var image = new Image { Width = stbImage.Width, Height = stbImage.Height, PixelData = pixelData };

                // Apply color replacement if color is specified in settings
                if (settings is ColorReplacementSettings colorSettings && colorSettings.ColorToReplace.HasValue)
                {
					// Color replacement code
					...
                }

                return image;
            }
        };

        public static Image LoadImage(this AssetManager assetManager, string assetName, ColorReplacementSettings settings = null) =>
            assetManager.UseLoader(_imageLoader, assetName, settings ?? ColorReplacementSettings.None);
    }
}
```

**Usage:**
```c#
AssetManager assetManager = AssetManager.CreateFileAssetManager(@"c:\MyGame\Assets");

// Load icon without color replacement (magenta stays as-is)
var iconRaw = assetManager.LoadImage("icon.png", ColorReplacementSettings.None);
Console.WriteLine($"Magenta pixel intact: {iconRaw.GetPixel(0, 0):X8}");

// Load same icon with magenta (0xFFFF00FF) replaced by transparency
var iconTransparent = assetManager.LoadImage("icon.png", 
    new ColorReplacementSettings(0xFFFF00FF));
Console.WriteLine($"Magenta replaced with transparent: {iconTransparent.GetPixel(0, 0):X8}");

// Each variant is cached separately
Console.WriteLine($"Cache entries: {assetManager.Cache.Count}");  // 2

// Reuse cached variant
var iconTransparent2 = assetManager.LoadImage("icon.png", 
    new ColorReplacementSettings(0xFFFF00FF));
Console.WriteLine($"Cache entries: {assetManager.Cache.Count}");  // Still 2 (reused cache)

// Different color replacement = different cache entry
var iconTransparent3 = assetManager.LoadImage("icon.png", 
    new ColorReplacementSettings(0xFFFFFFFF));  // Replace white instead
Console.WriteLine($"Cache entries: {assetManager.Cache.Count}");  // 3
```

**Key Points on Settings-Aware Loading:**
- Settings create separate cache entries: the full cache key is `"assetPath|settingsKey"`, so the same asset with different settings doesn't overwrite the cache.
- The `settings` parameter passed to the loader is an `IAssetSettings` instance you provide, allowing you to customize loading behavior.
- Common use cases include: asset transformation (color replacement, scaling, filtering), environment-specific configurations, localization/culture settings, or feature flags.
- Settings must have consistent `BuildKey()` values across identical configurations to share cache entries effectively.  

## Sponsor 
If this project is useful for you, you can support development:
- Boosty: https://boosty.to/rds1983
- Telegram Wallet: https://t.me/rds1983

### Crypto

USDT (TON): `UQCQy6tFInPvqinE44zHY4R0rYS3niaBikkqiSyGmyoAMwyO`

TON: `UQCQy6tFInPvqinE44zHY4R0rYS3niaBikkqiSyGmyoAMwyO`
