using Unity.Entities;
using Unity.Mathematics;

public struct BlendTree2DParamRuntime : IComponentData
{
	public float2 InputMapping;
	public float2 StepMapping;
}