using System.Xml.Linq;

namespace AssetManagementBase.Tests
{
	public static class AssetManagerExtensions
	{
		private static AssetLoader<UserProfile> _userProfileLoader = (ctx, assetName) =>
		{
			var data = ctx.AssetManager.LoadString(assetName);

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
