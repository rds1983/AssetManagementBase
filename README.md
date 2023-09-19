### AssetManagementBase
[![NuGet](https://img.shields.io/nuget/v/AssetManagementBase.svg)](https://www.nuget.org/packages/AssetManagementBase/) [![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

AssetManagementBase is basic C# asset management library that isn't tied to any particular game engine.

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
If _assembly's name is "Assembly.Name" then the above code will create AssetManager that loads resourcies with prefix "Assembly.Name.Prefix.".

If we don't the assembly's name prepended to the prefix, then pass 'false' as the third param when calling CreateResourceAssetManager. I.e.
```c#
AssetManager assetManager = AssetManager.CreateResourceAssetManager(_assembly, "Full.Path.Resources", false);
```

### Loading Assets
After AssetManager is created, it could be used following way:
```c#
    string data = assetManager.LoadString("data/mydata.txt");
```

### Custom Asset Types
This guide will demonstrate how to expand AssetManager with more loader methods.
Let's say we have following type:
  ```c#
    public class UserProfile
    {
        public string Name;
        public int Score;
    }
  ```
Which we store in following XML:
```xml
    <?xml version="1.0" encoding="utf-8" ?>
    <UserProfile>
      <Name>AssetManagementBase</Name>
      <Score>10000</Score>
    </UserProfile>
```

Now following code will let us to load it through the AssetManager:
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

		public static UserProfile LoadUserProfile(this AssetManager assetManager, string assetName) => assetManager.UseLoader(_userProfileLoader, assetName);
	}
}
```

It consists of delegate(_userProfileLoader) that does the loading and AssetManager's extension method.

Now it should be possible to load user profile with following code:
```c#
  UserProfile userProfile = assetManager.LoadUserProfile("profile.xml");
```  
