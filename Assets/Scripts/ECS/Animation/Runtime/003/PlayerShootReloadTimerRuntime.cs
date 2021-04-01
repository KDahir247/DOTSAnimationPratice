
using Unity.Entities;

public struct PlayerShootReloadTimerRuntime : IComponentData
{
	public float Ticks;
	public float RefreshTime; //max cap on Ticks before starting back to zero.
}
