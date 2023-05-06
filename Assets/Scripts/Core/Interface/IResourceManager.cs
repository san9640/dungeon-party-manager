using System;
using System.Collections;

namespace Core.Interface
{
	public interface IResourceManager : IDisposable
	{
		IEnumerator LoadAsync<T>(string path, Action<T> doneCallback) where T : class;

		void Load<T>(string path, Action<T> doneCallback) where T : class;

		void ClearAll();
	}
}