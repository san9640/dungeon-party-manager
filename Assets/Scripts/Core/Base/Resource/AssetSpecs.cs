using System;
using System.Collections.Generic;

namespace Core.Base.Resource
{
	public interface IAssetSpecs : IDisposable
	{
		Type AssetType { get; }

		Dictionary<string, string> KeyToPath { get; }
	}

	/// <summary>
	/// 에셋 Key-Path 스펙 홀더
	/// </summary>
	/// <typeparam name="T">에셋 타입</typeparam>
	[Serializable]
	public class AssetSpecs<T> : IAssetSpecs where T : UnityEngine.Object
	{
		public Type AssetType => typeof(T);

		public Dictionary<string, string> KeyToPath { get; }

		public AssetSpecs(Dictionary<string, string> keyToPath)
		{
			KeyToPath = keyToPath;
		}

		public void Dispose()
		{
			KeyToPath.Clear();
		}
	}
}