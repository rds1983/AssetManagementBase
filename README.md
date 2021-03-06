# AssetManagementBase
[![NuGet](https://img.shields.io/nuget/v/AssetManagementBase.svg)](https://www.nuget.org/packages/AssetManagementBase/) [![Chat](https://img.shields.io/discord/628186029488340992.svg)](https://discord.gg/ZeHxhCY)

AssetManagementBase is basic asset management library.

# Adding Reference
https://www.nuget.org/packages/AssetManagementBase
    
# Creating AssetManager
In order to create AssetManager [IAssetResolver](https://github.com/rds1983/AssetManagementBase/blob/master/src/IAssetResolver.cs) parameter must be passed to the constructor.

AssetManagementBase provides 2 implementation of IAssetResolver:
  * [FileAssetResolver](https://github.com/rds1983/AssetManagementBase/blob/master/src/FileAssetResolver.cs) that opens Stream using File.OpenRead. Sample AssetManager creation code:
```c#
    FileAssetResolver assetResolver = new FileAssetResolver(Path.Combine(PathUtils.ExecutingAssemblyDirectory, "Assets"));
    AssetManager assetManager = new AssetManager(assetResolver);
```

  * [ResourceAssetResolver](https://github.com/rds1983/AssetManagementBase/blob/master/src/ResourceAssetResolver.cs) that opens Stream using Assembly.GetManifestResourceStream. Sample AssetManager creation code:
```c#
    ResourceAssetResolver assetResolver = new ResourceAssetResolver(typeof(MyGame).Assembly, "Resources.");
    AssetManager assetManager = new AssetManager(GraphicsDevice, assetResolver);
```

# Loading Assets
After AssetManager is created, it could be used following way:
```c#
    string data = assetManager.Load<string>("data/mydata.txt");
```

AssetManagementBase allows to load following asset types out of the box:

Type|AssetLoader Type|Description
----|----------------|-----------
string|[StringLoader](https://github.com/rds1983/AssetManagementBase/blob/master/src/StringLoader.cs)|Loads any resource as string
byte[]|[ByteArrayLoader](https://github.com/rds1983/AssetManagementBase/blob/master/src/ByteArrayLoader.cs)|Loads any resource as byte array

# Custom Asset Types
This guide will demonstrate how to register additional loaders for any types.
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

First of all, we need to write the loader class:
  ```c#
	internal class UserProfileLoader : IAssetLoader<UserProfile>
	{
		public UserProfile Load(AssetLoaderContext context, string assetName)
		{
			var data = context.Load<string>(assetName);

			var xDoc = XDocument.Parse(data);

			var result = new UserProfile
			{
				Name = xDoc.Root.Element("Name").Value,
				Score = int.Parse(xDoc.Root.Element("Score").Value)
			};

			return result;
		}
	}
```

Now there are two ways to register this loader:
1. Mark the type with the attribute AssetLoaderAttribute. I.e.
  ```c#
    [AssetLoader(typeof(UserProfileLoader))]
    public class UserProfile
    ...
  ```

 2. Call a static method AssetManager.SetAssetManager. I.e.
  ```c#
    AssetManager.SetAssetLoader(new UserProfileLoader());
  ```

Now it should be possible to load user profile with following code:
```c#
  UserProfile userProfile = assetManager.Load<UserProfile>("profile.xml");
```  
