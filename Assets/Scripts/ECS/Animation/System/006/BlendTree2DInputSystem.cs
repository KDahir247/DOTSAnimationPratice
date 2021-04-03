using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class BlendTree2DInputSystem : SystemBase
{
	protected override void OnUpdate()
	{
		var horizontal = Input.GetAxis("Horizontal");
		var vertical = Input.GetAxis("Vertical");

		Entities
			.ForEach((Entity entity, ref BlendTree2DParamRuntime blendParam) =>
			{
				var unMappedTarget = new float2(horizontal, vertical) * blendParam.StepMapping +
				                     blendParam.InputMapping;

				blendParam.InputMapping = math.clamp(unMappedTarget, new float2(-1.0f, -1.0f), new float2(1.0f, 1.0f));
			})
			.ScheduleParallel();
	}
}