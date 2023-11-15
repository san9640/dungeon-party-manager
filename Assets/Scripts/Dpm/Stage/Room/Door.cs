using System;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using TMPro;
using UnityEngine;

namespace Dpm.Stage.Room
{
	public class Door : MonoBehaviour, IDisposable
	{
		[SerializeField]
		private GameObject closed;

		[SerializeField]
		private GameObject opened;

		[SerializeField]
		private TextMeshPro buffText;

		[HideInInspector]
		public int Id = -1;

		[HideInInspector]
		public int Index = -1;

		public bool IsOpened
		{
			get => opened.activeSelf;
			set
			{
				opened.SetActive(value);
				closed.SetActive(!value);
			}
		}

		public string BuffText
		{
			get => buffText.text;
			set => buffText.text = value;
		}

		// TODO : 클릭 확인
		private void OnMouseDown()
		{
			CoreService.Event.Publish(DoorClickEvent.Create(Id));
		}

		public void BuffTextShow(bool isShow)
		{
			buffText.gameObject.SetActive(isShow);
		}

		public void Dispose()
		{
			Id = -1;
			Index = -1;
			IsOpened = false;
			BuffText = null;
		}
	}
}