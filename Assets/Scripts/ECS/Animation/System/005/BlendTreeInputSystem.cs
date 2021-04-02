using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

//System just retrieve input and store it back to the BlendTree1DParamRuntime component
public sealed class BlendTreeInputSystem : SystemBase
{
	protected override void OnUpdate()
	{
		var delta = Input.GetAxis("Vertical");
		Entities
			.ForEach((ref BlendTree1DParamRuntime input)
				=> input.VelocityX = math.clamp(input.VelocityX + delta * input.VelocityStep, 0.0f, 1.0f))
			.ScheduleParallel();
	}
}