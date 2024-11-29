using System.IO;

namespace AssetManagementBase
{
	public interface IAssetAccessor
	{
		string Name { get; }

		bool Exists(string assetName);
		Stream Open(string assetName);
	}
}