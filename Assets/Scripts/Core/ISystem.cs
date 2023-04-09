using System;

namespace Core
{
    /// <summary>
    /// 시스템 기본 구현 필요 사항
    /// FIXME : 여기에 싱글톤 Instance getter를 넣을지...
    /// </summary>
    public interface ISystem : IDisposable
    {
        void Update();
    }
}