using System;
using System.Collections.Generic;
using System.Linq;
using Dpm.Utility;
using Dpm.Utility.Constants;
using UnityEngine;

namespace Dpm.Stage.Render
{
	public class SpriteAnimator : MonoBehaviour
	{
		[Serializable]
		public class SpriteAnimationInfo
		{
			public string name;
			public Sprite[] sprites;

			public Sprite this[int index] => sprites[index % sprites.Length];

			public bool IsLastFrame(int index) => sprites.Length == index + 1;
		}

		[SerializeField]
		private new SpriteRenderer renderer;

		[SerializeField]
		private SpriteAnimationInfo[] animationInfo;

		private Dictionary<string, SpriteAnimationInfo> _animDict;

		[SerializeField]
		private bool isLoop = true;

		private string _currentAnimName = "idle";
		private float _currentAnimTimePassed = 0;

		private const float TransitionSpriteDelay = 0.12f;

		private Direction _lookDirection = Direction.Right;

		private int _currentFrame = 0;

		public Direction LookDirection
		{
			get => _lookDirection;
			set
			{
				// 2의 거듭제곱 형태인지 확인
				if (value != Direction.None && ((value - 1) & value) == 0)
				{
					_lookDirection = value;

					renderer.flipX = _lookDirection is Direction.Left or Direction.Down;
				}
			}
		}

		public float Rotation
		{
			get => transform.rotation.z;
			set => transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, value);
		}

		private void Awake()
		{
			// Name -> Info로 데이터 변경
			_animDict = animationInfo.ToDictionary(info => info.name);

			Rebuild();
		}

		private void OnEnable()
		{
			SetAnimation("idle", isLoop);
		}

		public void Update()
		{
			if (!isLoop && _animDict[_currentAnimName].IsLastFrame(_currentFrame))
			{
				return;
			}

			var prevFrame = _currentFrame;

			_currentAnimTimePassed += Time.deltaTime;

			_currentFrame = (int) (_currentAnimTimePassed / TransitionSpriteDelay);

			if (prevFrame != _currentFrame)
			{
				Rebuild();
			}
		}

		public void SetAnimation(string animName, bool loop = true)
		{
			if (!_animDict.ContainsKey(animName))
			{
#if UNITY_EDITOR
				Debug.LogError($"Has no anim named [{animName}].");
#endif
				return;
			}

			_currentAnimName = animName;
			_currentAnimTimePassed = 0;
			_currentFrame = 0;
			isLoop = loop;

			Rebuild();
		}

		public void RemoveAnimation()
		{
			SetAnimation("idle");
		}

		private void Rebuild()
		{
			renderer.sprite = _animDict[_currentAnimName][_currentFrame];
		}
	}
}