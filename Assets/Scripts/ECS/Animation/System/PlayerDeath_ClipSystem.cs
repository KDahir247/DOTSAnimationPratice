using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public class PlayerDeath_ClipSystem : SystemBase
{
	private EntityQuery _animationDataQuery;
	private ProcessDefaultAnimationGraph _graphSystem;
	private EndSimulationEntityCommandBufferSystem _ecBSystem;

	protected override void OnCreate()
	{
		/*RequireForUpdate(GetEntityQuery(new EntityQueryDesc()
		{
			All = new ComponentType[]
			{
				typeof(DeltaTimeRuntime),
				typeof(PlayerDeath_PlayClipRuntime)
			}
		}));*/

		base.OnCreate();
		_graphSystem = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();
		_graphSystem.AddRef();
		_ecBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		
		//Used for destroying the graph and see if any entity has graph is so destroy.
		_animationDataQuery = GetEntityQuery(new EntityQueryDesc()
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

		_graphSystem.RemoveRef();
		//_graphSystem.Dispose();

		base.OnDestroy();
	}

	protected override void OnUpdate()
	{
		CompleteDependency();
		EntityCommandBuffer ecb = _ecBSystem.CreateCommandBuffer();

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
			.ForEach((Entity e, ref PlayerDeath_PlayStateRuntime animationState, ref PlayerDeath_PlayClipRuntime animation) =>
			{
				_graphSystem.Set.SendMessage(animationState.ClipPlayerNode,ClipPlayerNode.SimulationPorts.Clip, animation.clip);
			}).Run();

		Entities
			.WithName("Destroy_Graph")
			.WithNone<PlayerDeath_PlayClipRuntime>()
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref PlayerDeath_PlayStateRuntime animationState) =>
			{
				_graphSystem.Dispose(animationState.Graph);

				if(!_animationDataQuery.IsEmpty)
					ecb.RemoveComponent(e, typeof(PlayerDeath_PlayStateRuntime));
			}).Run();
	}



	static PlayerDeath_PlayStateRuntime CreateGraph(Entity entity, ProcessDefaultAnimationGraph graphSystem,
		ref Rig rig, ref PlayerDeath_PlayClipRuntime animation)
	{
		GraphHandle graph = graphSystem.CreateGraph();
		var data = new PlayerDeath_PlayStateRuntime()
		{
			Graph = graph,
			ClipPlayerNode = graphSystem.CreateNode<ClipPlayerNode>(graph)
		};
		var deltaTimeNode = graphSystem.CreateNode<ConvertDeltaTimeToFloatNode>(graph);
		var entityNode = graphSystem.CreateNode(graph, entity);

		NodeSet set = graphSystem.Set;
		set.Connect(entityNode,deltaTimeNode,ConvertDeltaTimeToFloatNode.KernelPorts.Input);
		set.Connect(deltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Output,data.ClipPlayerNode,ClipPlayerNode.KernelPorts.DeltaTime);
		set.Connect(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Output,entityNode, NodeSetAPI.ConnectionType.Feedback);
		
		set.SetData(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Speed, 1.0f);
		set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Configuration, new ClipConfiguration(){Mask = ClipConfigurationMask.LoopTime});
		set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Rig, rig);
		set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, animation.clip);
		
		return data;
	}
}
