﻿namespace Core.Interface
{
	/// <summary>
	/// Send 받을 이벤트가 있는 경우에 구현해야 하는 인터페이스
	/// </summary>
	public interface IEventListener
	{
		void OnEvent(Event e);
	}
}