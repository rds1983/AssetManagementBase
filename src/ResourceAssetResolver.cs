﻿using AssetManagementBase.Utility;
using System;
using System.IO;
using System.Reflection;

namespace AssetManagementBase
{
	public class ResourceAssetResolver : IAssetResolver
	{
		private Assembly _assembly;

		public Assembly Assembly
		{
			get { return _assembly; }

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				_assembly = value;
			}
		}

		public string Prefix { get; set; }

		public ResourceAssetResolver(Assembly assembly, string prefix)
		{
			Assembly = assembly;
			Prefix = prefix;

			if (!Prefix.EndsWith("."))
			{
				Prefix += ".";
			}
		}

		public Stream Open(string assetName)
		{
			assetName = assetName.Replace(AssetManager.SeparatorSymbol, '.');

			return Res.OpenResourceStream(Assembly, Prefix + assetName);
		}
	}
}
