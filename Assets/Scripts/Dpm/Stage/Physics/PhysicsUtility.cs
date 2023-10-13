namespace Dpm.Stage.Physics
{
	public static class PhysicsUtility
	{
		public static float GetDistanceBtwCollider(ICustomCollider a, ICustomCollider b)
		{
			// FIXME : 원래는 Bounds 사이의 거리를 구해야 함
			return (a.Position - b.Position).magnitude;
		}
	}
}