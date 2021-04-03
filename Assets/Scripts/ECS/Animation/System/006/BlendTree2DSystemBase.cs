using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public abstract class BlendTree2DSystemBase : SystemBase
{
	protected EndSimulationEntityCommandBufferSystem EsBufferSystem;
	protected ProcessDefaultAnimationGraph GraphSystem;

	protected override void OnCreate()
	{
		base.OnCreate();

		GraphSystem = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();
		EsBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		GraphSystem.AddRef();
		GraphSystem.Set.RendererModel = NodeSet.RenderExecutionModel.Islands;
	}

	protected override void OnUpdate()
	{
		Entities
			.WithName("CreateGraph")
			.WithNone<BlendTree2DKernelRuntime>()
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity entity, ref Rig rig, ref BlendTree2DRuntime blendTreeData)
				=> CreateGraph(entity, ref rig, ref blendTreeData))
			.Run();
	}

	protected override void OnDestroy()
	{
		//Call Entity Foreach and Destroy Node in graph.

		Entities
			.WithName("DestroyGraph")
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity entity, ref BlendTree2DKernelRuntime nodeHandle)
				=> DestroyGraph(entity, ref nodeHandle)).Run();

		if (GraphSystem.RefCount > 0)
			GraphSystem.RemoveRef();

		base.OnDestroy();
	}


	protected abstract void CreateGraph(Entity entity, ref Rig rig, ref BlendTree2DRuntime blendTreeData);
	protected abstract void DestroyGraph(Entity entity, ref BlendTree2DKernelRuntime nodeHandle);
}