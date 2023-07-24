using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using UnityEngine;

namespace Dpm.Stage.Field
{
	public class Door : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private GameObject closed;

		[SerializeField]
		private GameObject opened;

		public int Id = -1;

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
		private void OnMouseDown()
		{
			CoreService.Event.Publish(DoorClickEvent.Create(Id));
		}

		public void Dispose()
		{
			Id = -1;
			IsOpened = false;
		}
	}
}