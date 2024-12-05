using System.IO;

namespace AssetManagementBase
{
	public interface IAssetAccessor
	{
		string Name { get; }

		bool Exists(string path);
		Stream Open(string path);
	}
}