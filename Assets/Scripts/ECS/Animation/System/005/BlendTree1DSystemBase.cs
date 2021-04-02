using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public abstract class BlendTree1DSystemBase : SystemBase
{
	protected EndSimulationEntityCommandBufferSystem EcbSystem;
	protected ProcessDefaultAnimationGraph GraphSystem;

	protected override void OnCreate()
	{
		base.OnCreate();
		EcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		GraphSystem = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();

		GraphSystem.AddRef();
		GraphSystem.Set.RendererModel = NodeSet.RenderExecutionModel.Islands;
	}

	protected override void OnUpdate()
	{
		Entities
			.WithNone<BlendTree1DKernelRuntime>()
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref Rig rig, ref BlendTree1DRuntime blendTreeData) =>
				CreateGraph(e, ref rig, ref blendTreeData))
			.Run();


		Entities
			.WithNone<BlendTree1DRuntime>()
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref BlendTree1DKernelRuntime nodeHandle)
				=> DestroyGraph(e, ref nodeHandle))
			.Run();
	}

	protected override void OnDestroy()
	{
		if (GraphSystem == null)
			return;

		Entities
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref BlendTree1DKernelRuntime nodeHandle)
				=> DestroyGraph(e, ref nodeHandle))
			.Run();

		GraphSystem.RemoveRef();
		base.OnDestroy();
	}

	protected abstract void CreateGraph(Entity entity, ref Rig rig, ref BlendTree1DRuntime blendTreeData);
	protected abstract void DestroyGraph(Entity entity, ref BlendTree1DKernelRuntime nodeHandle);
}