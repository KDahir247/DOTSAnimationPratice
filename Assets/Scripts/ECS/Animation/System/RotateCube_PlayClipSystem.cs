using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public class RotateCube_PlayClipSystem : SystemBase
{
	private ProcessDefaultAnimationGraph _animationGraphSystem;
	private EndSimulationEntityCommandBufferSystem _ecbSystem;
	private EntityQuery _animationQuery;
	protected override void OnCreate()
	{
		//will probably create the system since we are updating before DefaultAnimationSystemGroup and ProcessDefaultAnimationGraph update in group. 
		_animationGraphSystem = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();
		// Increase the reference count on the graph system so it knows
		// that we want to use it
		_animationGraphSystem.AddRef();

		_ecbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

		//Connected components in the graph will be executed in one job.
		_animationGraphSystem.Set.RendererModel = NodeSet.RenderExecutionModel.Islands;

		_animationQuery = GetEntityQuery(new EntityQueryDesc()
		{
			All = new ComponentType[]
			{
				typeof(RotateCube_PlayStateRuntime)
			},
			None = new ComponentType[]
			{
				typeof(RotateCube_PlayClipRuntime)
			},
		});

	}

	protected override void OnUpdate()
	{
		CompleteDependency();

		EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();

		//Create an animation graph if the component isn't created and attached to the entity.
		Entities
			.WithName("CreateGraph")
			.WithNone<RotateCube_PlayStateRuntime>()
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref RotateCube_PlayClipRuntime animation, ref Rig rig) =>
			{
				var data = CreateGraph(e, _animationGraphSystem, ref rig, ref animation);
				ecb.AddComponent(e, data);
			}).Run();

		//Update the graph if the clip component changed
		Entities
			.WithName("UpdateGraph")
			.WithChangeFilter<RotateCube_PlayClipRuntime>()
			.WithoutBurst()
			.ForEach((Entity e, ref RotateCube_PlayClipRuntime animation, ref RotateCube_PlayStateRuntime animationState) =>
			{
				_animationGraphSystem.Set.SendMessage(animationState.NodeHandle, ClipPlayerNode.SimulationPorts.Clip, animation.clip);
			}).Run();

		//Remove the graph system and Clips if the Clip Component is missing, but the State component is present
		Entities
			.WithNone<RotateCube_PlayClipRuntime>()
			.WithoutBurst().ForEach((Entity e, ref RotateCube_PlayStateRuntime animationState) =>
			{
				//Destroying all the manage node from the graph Handle and removing the graph handle when the managed node destroyed.
				_animationGraphSystem.Dispose(animationState.GraphHandle);

				if(!_animationQuery.IsEmpty)
					ecb.RemoveComponent(_animationQuery, typeof(RotateCube_PlayStateRuntime));
			}).Run();
	}


	protected override void OnDestroy()
	{
		if (_animationGraphSystem == null)
			return;

		//Remove ref count on animation graph system to signify the we are done using it.
		if(_animationGraphSystem.RefCount > 0)
			_animationGraphSystem.RemoveRef();

		base.OnDestroy();
	}


	private static RotateCube_PlayStateRuntime CreateGraph(Entity entity, ProcessDefaultAnimationGraph graphSystem,
		ref Rig rig, ref RotateCube_PlayClipRuntime animation)
	{
		//create a animation graph and store the handle to the graph.
		GraphHandle graph = graphSystem.CreateGraph();

		RotateCube_PlayStateRuntime data = new RotateCube_PlayStateRuntime()
		{
			GraphHandle = graph,
			NodeHandle = graphSystem.CreateNode<ClipPlayerNode>(graph)
		};

		//Create an ConvertDeltaTimeToFloatNode Node in graph.
		var deltaNode = graphSystem.CreateNode<ConvertDeltaTimeToFloatNode>(graph);
		//Create an EntityNode in graph.
		var entityNode = graphSystem.CreateNode(graph, entity);

		//Retrieve node set from graph system.
		var set = graphSystem.Set;

		//Connect kernel ports.

		//Connect the entityNode to the deltaNode and pass the DeltaTime Input (struct) to the deltaNode.
		set.Connect(entityNode, deltaNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
		//Connect the deltaNode to the ClipPlayerNode and pass the DeltaTime output (float) to the ClipPlayerNode and assign it to the DeltaTime in the ClipPlayerNode.
		set.Connect(deltaNode, ConvertDeltaTimeToFloatNode.KernelPorts.Output, data.NodeHandle, ClipPlayerNode.KernelPorts.DeltaTime);
		//Connect the clipPlayer node to the EntityNode and passes the Output of the ClipPlayerNode
		//We must pass a enum under NodeSet API Connection type which allows feeding information back to an upstream node without forming a cycle
		set.Connect(data.NodeHandle, ClipPlayerNode.KernelPorts.Output,entityNode, NodeSetAPI.ConnectionType.Feedback);

		//EntityNode -> ConvertDeltaTimeToFloatNode -> ClipPlayerNode -> Repeat.

		//Send message to set parameters on the CliPlayerNode

		//Set data for ClipPlayer Kernal Port
		set.SetData(data.NodeHandle, ClipPlayerNode.KernelPorts.Speed, 1.0f);
		//Send Message to ClipPlayer SimulationPort.
		set.SendMessage(data.NodeHandle, ClipPlayerNode.SimulationPorts.Configuration, new ClipConfiguration(){Mask  = ClipConfigurationMask.LoopTime});
		set.SendMessage(data.NodeHandle, ClipPlayerNode.SimulationPorts.Rig, rig);
		set.SendMessage(data.NodeHandle, ClipPlayerNode.SimulationPorts.Clip, animation.clip);
		return data;
	}
}
