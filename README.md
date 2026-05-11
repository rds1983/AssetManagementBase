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
