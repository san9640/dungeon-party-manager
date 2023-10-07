using System;

namespace Core.Interface
{
	public interface IAssetManager : IDisposable
	{
		/// <summary>
		/// 로드된 특정 에셋을 가져옴
		/// </summary>
		/// <param name="specName">에셋 키 (Json)</param>
		/// <param name="result">로드된 에셋</param>
		/// <typeparam name="T">에셋 타입</typeparam>
		/// <returns>로드 성공 여부</returns>
		bool TryGet<T>(string specName, out T result) where T : UnityEngine.Object;

		T UnsafeGet<T>(string specName) where T : UnityEngine.Object;
	}
}