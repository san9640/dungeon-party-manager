using System;
using System.Collections.Generic;
using System.Linq;
using Dpm.Utility;
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
		}

		[SerializeField]
		private new SpriteRenderer renderer;

		[SerializeField]
		private SpriteAnimationInfo[] animationInfo;

		private Dictionary<string, SpriteAnimationInfo> _animDict;

		private string _currentAnimName = "idle";
		private float _currentAnimTimePassed = 0;

		private const float TransitionSpriteDelay = 0.12f;

		private Direction _lookDirection = Direction.Right;

		public Direction LookDirection
		{
			get => _lookDirection;
			set
			{
				// 2의 거듭제곱 형태인지 확인
				if (value != Direction.None && ((value - 1) & value) == 0)
				{
					_lookDirection = value;

					renderer.flipX = _lookDirection == Direction.Left || _lookDirection == Direction.Down;
				}
			}
		}

		private void Awake()
		{
			// Name -> Info로 데이터 변경
			_animDict = animationInfo.ToDictionary(info => info.name);

			Rebuild();
		}

		public void Update()
		{
			_currentAnimTimePassed += Time.deltaTime;

			Rebuild();
		}

		public void SetAnimation(string animName)
		{
			if (!_animDict.ContainsKey(animName))
			{
#if UNITY_EDITOR
				Debug.LogError($"Has no anim named [{animName}].");
#endif
				return;
			}

			if (_currentAnimName == animName)
			{
				return;
			}

			_currentAnimName = animName;
			_currentAnimTimePassed = 0;

			Rebuild();
		}

		public void RemoveAnimation()
		{
			SetAnimation("idle");
		}

		private void Rebuild()
		{
			var currentFrame = (int) (_currentAnimTimePassed / TransitionSpriteDelay);

			renderer.sprite = _animDict[_currentAnimName][currentFrame];
		}
	}
}