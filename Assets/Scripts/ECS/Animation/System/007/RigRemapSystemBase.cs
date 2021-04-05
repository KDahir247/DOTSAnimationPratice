using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public abstract class RigRemapSystemBase : SystemBase
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
			.WithNone<RigRemapKernelRuntime>()
			.WithName("CreateGraph")
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref Rig rig, ref RigRemapRuntime data)
				=> CreateGraph(e, ref rig, ref data)).Run();
	}

	protected override void OnDestroy()
	{
		Entities
			.WithName("DestroyGraph")
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref RigRemapKernelRuntime nodeHandles)
				=> DestroyGraph(e, ref nodeHandles)).Run();

		if (GraphSystem.RefCount > 0)
			GraphSystem.RemoveRef();

		base.OnDestroy();
	}

	protected abstract void CreateGraph(Entity e, ref Rig rig, ref RigRemapRuntime setup);
	protected abstract void DestroyGraph(Entity e, ref RigRemapKernelRuntime nodeHandles);
}