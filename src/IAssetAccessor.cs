using System.IO;

namespace AssetManagementBase
{
	public interface IAssetAccessor
	{
		bool Exists(string assetName);
		Stream Open(string assetName);
	}
}