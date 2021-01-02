﻿namespace AssetManagementBase
{
	public interface IAssetLoader<out T>
	{
		T Load(AssetLoaderContext context, string assetName);
	}
}
