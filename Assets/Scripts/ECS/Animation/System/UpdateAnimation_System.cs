using Unity.Animation;
using Unity.Entities;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public class UpdateAnimation_System : SystemBase
{
	protected override void OnUpdate()
	{
		var worldDeltaTime = World.Time.DeltaTime;
		Entities.ForEach((Entity e, ref DeltaTimeRuntime deltaTime) => { deltaTime.Value = worldDeltaTime; })
			.ScheduleParallel();
	}
}