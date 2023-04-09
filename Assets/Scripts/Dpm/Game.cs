using System;
using Core;
using Core.Message;
using UnityEngine;

namespace Dpm
{
    public class Game : MonoBehaviour
    {
        private EventSystem _eventSystem;
        private UpdateSystem _updateSystem;

        private void Awake()
        {
            _eventSystem = EventSystem.Instance;
            _updateSystem = UpdateSystem.Instance;
        }

        private void OnDestroy()
        {
            (_eventSystem as IDisposable).Dispose();
            (_updateSystem as IDisposable).Dispose();
        }

        private void Update()
        {
            // FIXME : 순서는 바뀔 수 있음
            (_eventSystem as ISystem).Update();
            (_updateSystem as ISystem).Update();
        }
    }
}