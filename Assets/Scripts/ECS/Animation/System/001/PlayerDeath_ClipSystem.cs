using JetBrains.Annotations;
using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public sealed class PlayerDeath_ClipSystem : SystemBase
{
	private EntityQuery _animationDataQuery;
	private EndSimulationEntityCommandBufferSystem _ecBSystem;
	private ProcessDefaultAnimationGraph _graphSystem;

	protected override void OnCreate()
	{
		base.OnCreate();
		_graphSystem = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();
		_graphSystem.AddRef();
		_ecBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();


		//Used for destroying the graph and see if any entity has graph is so destroy.
		_animationDataQuery = GetEntityQuery(new EntityQueryDesc
		{
			All = new ComponentType[]
			{
				typeof(PlayerDeath_PlayStateRuntime)
			},
			None = new ComponentType[]
			{
				typeof(PlayerDeath_PlayClipRuntime)
			}
		});

		_graphSystem.Set.RendererModel = NodeSet.RenderExecutionModel.Islands;
	}

	protected override void OnDestroy()
	{
		if (_graphSystem == null)
			return;

		Entities
			.WithName("DestroyGraph")
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref PlayerDeath_PlayStateRuntime data) => DestroyGraph(e, _graphSystem, ref data))
			.Run();

		if (_graphSystem.RefCount > 0)
			_graphSystem.RemoveRef();

		base.OnDestroy();
	}

	private void DestroyGraph(Entity entity, [NotNull] ProcessDefaultAnimationGraph graphSystem,
		ref PlayerDeath_PlayStateRuntime data)
	{
		var set = graphSystem.Set;
		set.Destroy(data.EntityNode);
		set.Destroy(data.DeltaTimeNode);
		set.Destroy(data.ClipPlayerNode);

		EntityManager.RemoveComponent<PlayerDeath_PlayStateRuntime>(entity);
	}

	protected override void OnUpdate()
	{
		CompleteDependency();
		var ecb = _ecBSystem.CreateCommandBuffer();

		Entities
			.WithName("Start_Graph")
			.WithNone<PlayerDeath_PlayStateRuntime>()
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref Rig rig, ref PlayerDeath_PlayClipRuntime animation) =>
			{
				var state = CreateGraph(e, _graphSystem, ref rig, ref animation);
				ecb.AddComponent(e, state);
			}).Run();

		Entities
			.WithName("Update_Graph")
			.WithChangeFilter<PlayerDeath_PlayClipRuntime>()
			.WithoutBurst()
			.ForEach(
				(Entity e, ref PlayerDeath_PlayStateRuntime animationState,
					ref PlayerDeath_PlayClipRuntime animation) => _graphSystem.Set.SendMessage(
					animationState.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip,
					animation.clip)).Run();

		Entities
			.WithName("Destroy_Graph")
			.WithNone<PlayerDeath_PlayClipRuntime>()
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach(
				(Entity e, ref PlayerDeath_PlayStateRuntime animationState, ref PlayerDeath_PlayStateRuntime data) =>
				{
					Debug.Log("Destroying");
					DestroyGraph(e, _graphSystem, ref data);

					if (!_animationDataQuery.IsEmpty)
						ecb.RemoveComponent(e, typeof(PlayerDeath_PlayStateRuntime));
				}).Run();
	}


	private static PlayerDeath_PlayStateRuntime CreateGraph(Entity entity,
		[NotNull] ProcessDefaultAnimationGraph graphSystem,
		ref Rig rig, ref PlayerDeath_PlayClipRuntime animation)
	{
		var set = graphSystem.Set;

		var data = new PlayerDeath_PlayStateRuntime
		{
			ClipPlayerNode = set.Create<ClipPlayerNode>(),
			DeltaTimeNode = set.Create<ConvertDeltaTimeToFloatNode>(),
			EntityNode = set.CreateComponentNode(entity)
		};

		set.Connect(data.EntityNode, data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
		set.Connect(data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Float, data.ClipPlayerNode,
			ClipPlayerNode.KernelPorts.DeltaTime);
		set.Connect(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Output, data.EntityNode,
			NodeSetAPI.ConnectionType.Feedback);

		set.SetData(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Speed, 1.0f);
		set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Configuration,
			new ClipConfiguration {Mask = ClipConfigurationMask.LoopTime});
		set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Rig, rig);
		set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, animation.clip);

		return data;
	}
}