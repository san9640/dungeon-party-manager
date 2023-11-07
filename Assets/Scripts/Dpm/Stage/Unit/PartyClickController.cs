using Core.Interface;
using Dpm.CoreAdapter;
using Dpm.Stage.Physics;
using Dpm.Stage.Room;
using Dpm.Stage.UI.Event;
using Dpm.Utility.Constants;
using Dpm.Utility.Extensions;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Dpm.Stage.Unit
{
	public class PartyClickController : IUpdatable
	{
		private bool _isClicked = false;

		private Character _selected;

		private const int UpdatePriority = Utility.Constants.UpdatePriority.StageUnits - 1;

		private Party _targetParty;

		private SpawnArea _spawnArea;

		private bool _isAvailable = true;

		public void Init(Party targetParty, SpawnArea spawnArea)
		{
			_targetParty = targetParty;
			_spawnArea = spawnArea;

			_isAvailable = true;

			CoreService.FrameUpdate.RegisterUpdate(this, UpdatePriority);

			CoreService.Event.Subscribe<PauseButtonPressedEvent>(OnPauseButtonPressed);
			CoreService.Event.Subscribe<ResumeButtonPressedEvent>(OnResumeButtonPressed);
		}

		public void Dispose()
		{
			CoreService.Event.Unsubscribe<PauseButtonPressedEvent>(OnPauseButtonPressed);
			CoreService.Event.Unsubscribe<ResumeButtonPressedEvent>(OnResumeButtonPressed);

			CoreService.FrameUpdate.UnregisterUpdate(this, UpdatePriority);

			ReleaseCharacter();

			_spawnArea = null;
			_isClicked = false;
			_selected = null;
			_targetParty = null;
		}

		private void OnPauseButtonPressed(Core.Interface.Event e)
		{
			ReleaseCharacter();
			_isClicked = false;

			_isAvailable = false;
		}

		private void OnResumeButtonPressed(Core.Interface.Event e)
		{
			_isAvailable = true;
		}

		public void UpdateFrame(float dt)
		{
			if (!_isAvailable)
			{
				return;
			}

			if (!_isClicked)
			{
				if (Input.GetMouseButtonDown(0))
				{
					_isClicked = true;

					UpdateClickedCharacter();
				}
			}
			else
			{
				if (!Input.GetMouseButton(0))
				{
					ReleaseCharacter();

					_isClicked = false;
					_selected = null;
				}
				else if (_selected != null)
				{
					_selected.Position = GetCurrentMouseWorldPos();
				}
			}
		}

		private Vector2 GetCurrentMouseWorldPos()
		{
			Debug.Assert(Camera.main != null, "Camera.main != null");

			var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			return mouseWorldPos.ConvertToVector2();
		}

		private void UpdateClickedCharacter()
		{
			var clickedPos = GetCurrentMouseWorldPos();

			foreach (var member in _targetParty.Members)
			{
				if (member.Bounds.Contains(clickedPos))
				{
					_selected = member;

					break;
				}
			}
		}

		private void ReleaseCharacter()
		{
			if (_selected == null)
			{
				return;
			}

			var mousePos = GetCurrentMouseWorldPos();

			_selected.Position = _spawnArea.GetClosestSpawnPos(_targetParty, _selected, mousePos);
			_selected.OriginPos = _selected.Position;

			_selected = null;
		}
	}
}