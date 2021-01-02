using System.IO;

namespace AssetManagementBase
{
	public interface IAssetResolver
	{
		Stream Open(string assetName);
	}
}
