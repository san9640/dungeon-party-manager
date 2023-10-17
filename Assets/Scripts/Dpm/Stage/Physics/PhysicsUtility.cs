namespace Dpm.Stage.Physics
{
	public static class PhysicsUtility
	{
		public static float GetDistanceBtwCollider(ICustomCollider a, ICustomCollider b)
		{
			// FIXME : 원래는 Bounds 사이의 거리를 구해야 함
			return (a.Position - b.Position).magnitude;
		}

		public static bool IsOverlapped(ICustomCollider a, ICustomCollider b)
		{
			return a.Bounds.Min.x <= b.Bounds.Max.x && b.Bounds.Min.x <= a.Bounds.Max.x
				&& a.Bounds.Min.y <= b.Bounds.Max.y && b.Bounds.Min.y <= a.Bounds.Max.y;
		}

		public static bool IsOverlapped(Bounds2D a, Bounds2D b)
		{
			return a.Min.x <= b.Max.x && b.Min.x <= a.Max.x &&
			       a.Min.y <= b.Max.y && b.Min.y <= a.Max.y;
		}
	}
}