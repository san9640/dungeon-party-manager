using System;
using Core.Base.Resource;

namespace Core.Interface
{
	public interface IAssetManager : IDisposable
	{
		/// <summary>
		/// 전체 로드
		/// FIXME : 리소스 양이 너무 많아서 검은 화면으로 정지해있는 시간이 길어진다면, 로딩 화면에서 애니메이션이라도 재생하도록 LoadAsync로 변경 필요
		/// </summary>
		void Load(IAssetSpecs[] assetSpec);

		/// <summary>
		/// 로드된 특정 에셋을 가져옴
		/// </summary>
		/// <param name="key">에셋 키 (Json)</param>
		/// <param name="result">로드된 에셋</param>
		/// <typeparam name="T">에셋 타입</typeparam>
		bool TryGet<T>(string key, out T result) where T : UnityEngine.Object;
	}
}