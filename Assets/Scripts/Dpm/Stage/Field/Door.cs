using UnityEngine;

namespace Dpm.Stage.Field
{
	public class Door : MonoBehaviour
	{
		[SerializeField]
		private GameObject closed;

		[SerializeField]
		private GameObject opened;

		public bool IsOpened
		{
			get => opened.activeSelf;
			set
			{
				opened.SetActive(value);
				closed.SetActive(!value);
			}
		}

		// TODO : 클릭 확인
	}
}