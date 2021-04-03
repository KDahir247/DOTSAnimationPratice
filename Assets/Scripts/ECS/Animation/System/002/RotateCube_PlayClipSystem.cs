using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;
using Unity.Mathematics;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public sealed class RotateCube_PlayClipSystem : SystemBase
{
	private ProcessDefaultAnimationGraph _animationGraphSystem;
	private EndSimulationEntityCommandBufferSystem _ecbSystem;

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
	}

	protected override void OnUpdate()
	{
		var ecb = _ecbSystem.CreateCommandBuffer();

		//Create an animation graph if the component isn't created and attached to the entity.
		Entities
			.WithName("CreateGraph")
			.WithNone<RotateCube_PlayStateRuntime>()
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref RotateCube_PlayClipRuntime animation, ref Rig rig) =>
			{
				var data = CreateGraph(e, ref rig, ref animation);
				ecb.AddComponent(e, data);
			}).Run();

		//Update the graph if the clip component changed
		Entities
			.WithName("UpdateGraph")
			.WithChangeFilter<RotateCube_PlayClipRuntime>()
			.WithoutBurst()
			.ForEach(
				(Entity e, ref RotateCube_PlayClipRuntime animation, ref RotateCube_PlayStateRuntime animationState) =>
					_animationGraphSystem.Set.SendMessage(animationState.NodeHandle,
						ClipPlayerNode.SimulationPorts.Clip, animation.clip)).Run();

		var pingPongTime = math.sin((float) World.Unmanaged.CurrentTime.ElapsedTime) * .5f + .5f;
		Entities
			.WithName("ChangeSpeed")
			.WithoutBurst()
			.ForEach((Entity entity, ref RotateCube_PlayStateRuntime animationState,
				in RotateCube_PlayClipRuntime animation) =>
			{
				var value = AnimationCurveEvaluator.Evaluate(pingPongTime, animation.curve);
				_animationGraphSystem.Set.SetData(animationState.NodeHandle, ClipPlayerNode.KernelPorts.Speed, value);
			}).Run();
	}


	protected override void OnDestroy()
	{
		if (_animationGraphSystem == null)
			return;

		Entities
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity entity, ref RotateCube_PlayStateRuntime animationState) =>
				DestroyGraph(entity, ref animationState)).Run();

		//Remove ref count on animation graph system to signify the we are done using it.
		if (_animationGraphSystem.RefCount > 0)
			_animationGraphSystem.RemoveRef();

		base.OnDestroy();
	}


	private RotateCube_PlayStateRuntime CreateGraph(Entity entity,
		ref Rig rig, ref RotateCube_PlayClipRuntime animation)
	{
		//Retrieve node set from graph system.
		var set = _animationGraphSystem.Set;

		var data = new RotateCube_PlayStateRuntime
		{
			NodeHandle = set.Create<ClipPlayerNode>(),
			DeltaTimeNode = set.Create<ConvertDeltaTimeToFloatNode>(),
			EntityNode = set.CreateComponentNode(entity)
		};

		//Connect kernel ports.

		//Connect the entityNode to the deltaNode and pass the DeltaTime Input (struct) to the deltaNode.
		set.Connect(data.EntityNode, data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
		//Connect the deltaNode to the ClipPlayerNode and pass the DeltaTime output (float) to the ClipPlayerNode and assign it to the DeltaTime in the ClipPlayerNode.
		set.Connect(data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Float, data.NodeHandle,
			ClipPlayerNode.KernelPorts.DeltaTime);
		//Connect the clipPlayer node to the EntityNode and passes the Output of the ClipPlayerNode
		//We must pass a enum under NodeSet API Connection type which allows feeding information back to an upstream node without forming a cycle
		set.Connect(data.NodeHandle, ClipPlayerNode.KernelPorts.Output, data.EntityNode,
			NodeSetAPI.ConnectionType.Feedback);

		//EntityNode -> ConvertDeltaTimeToFloatNode -> ClipPlayerNode -> Repeat.

		//Send message to set parameters on the CliPlayerNode

		//Set data for ClipPlayer Kernal Port
		set.SetData(data.NodeHandle, ClipPlayerNode.KernelPorts.Speed, 1.0f);
		//Send Message to ClipPlayer SimulationPort.
		set.SendMessage(data.NodeHandle, ClipPlayerNode.SimulationPorts.Configuration,
			new ClipConfiguration {Mask = ClipConfigurationMask.LoopTime});
		set.SendMessage(data.NodeHandle, ClipPlayerNode.SimulationPorts.Rig, rig);
		set.SendMessage(data.NodeHandle, ClipPlayerNode.SimulationPorts.Clip, animation.clip);
		return data;
	}

	private void DestroyGraph(Entity e, ref RotateCube_PlayStateRuntime data)
	{
		var set = _animationGraphSystem.Set;
		set.Destroy(data.NodeHandle);
		set.Destroy(data.DeltaTimeNode);
		set.Destroy(data.EntityNode);
		EntityManager.RemoveComponent<RotateCube_PlayStateRuntime>(e);
	}
}