using Unity.Animation;
using Unity.Entities;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public abstract class PlayerBlendSystemBase : SystemBase
{
	protected ProcessDefaultAnimationGraph graphSystem;
	//protected EndSimulationEntityCommandBufferSystem _ecbSystem;

	protected sealed override void OnCreate()
	{
		base.OnCreate();
		graphSystem = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();
		//_ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
	}

	protected sealed override void OnUpdate()
	{
		CompleteDependency();

		//Create if it has non of both PlayerBlendNodeBuffer and PlayerClipDataRuntime.
		//Currently creating graphs and node set must happen on main thread and it causes structural change
		Entities
			.WithNone<PlayerBlendClipNodeBuffer, PlayerBlendDataRuntime>() //must not have or else the graph is already created
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref Rig rig, in PlayerBlendKernelRuntime kernelBlendNode)
				=> CreateGraph(e, ref rig, kernelBlendNode)).Run();

		//Destroy if it has no Clip buffer.
		Entities
			.WithNone<PlayerBlendClipBuffer>() //if it has no clip buffer component then we destroy the graph.
			.WithAll<Rig>() //it must have a rig component or our whole logic is flawed.
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref PlayerBlendDataRuntime playerBlendData)
				=> DestroyGraph(e, ref playerBlendData)).Run();
	}

	protected sealed override void OnDestroy()
	{
		if (graphSystem == null)
			return;

		if(graphSystem.RefCount > 0)
			graphSystem.RemoveRef();

		base.OnDestroy();
	}


	protected abstract void CreateGraph(Entity e, ref Rig rig, in PlayerBlendKernelRuntime kernelBlendNode);
	protected abstract void DestroyGraph(Entity e, ref PlayerBlendDataRuntime playerBlendData);
}
