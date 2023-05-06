using System;
using System.Collections;

namespace Core.Interface
{
	public interface ICoroutineManager : IDisposable
	{
		Guid StartCoroutine(IEnumerator enumerator);

		void StopCoroutine(Guid guid);
	}
}