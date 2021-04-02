using System.Data;
using JetBrains.Annotations;
using Unity.Animation;
using Unity.Assertions;
using Unity.Collections;
using Unity.DataFlowGraph;
using Unity.Entities;
using Unity.Mathematics;

[UpdateBefore(typeof(DefaultAnimationSystemGroup))]
public sealed class PlayerShootReload_System : SystemBase
{
	private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
	private ProcessDefaultAnimationGraph _graphSystem;

	protected override void OnCreate()
	{
		_endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
		_graphSystem = World.GetOrCreateSystem<ProcessDefaultAnimationGraph>();
		_graphSystem.AddRef();
		_graphSystem.Set.RendererModel = NodeSet.RenderExecutionModel.Islands;
	}

	protected override void OnUpdate()
	{
		//Creating Graph

		Entities
			.WithNone<PlayerShootReloadDataRuntime>()
			.WithName("CreateGraph")
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref Rig rig)
				=> CreateGraph(e, ref rig, _graphSystem))
			.Run();

		//Updating Graph
		var clipBuffer = GetBufferFromEntity<ShootReload_PlayClipBuffer>();
		var deltaTime = World.Time.DeltaTime;

		Entities
			.WithAll<PlayerShootReloadDataRuntime, ShootReload_PlayClipBuffer>()
			.WithName("UpdateGraph")
			.WithoutBurst()
			.ForEach((Entity e, ref PlayerShootReloadDataRuntime data, ref PlayerShootReloadTimerRuntime timer) =>
			{
				if (timer.Ticks < clipBuffer[e][0].Clip.Value.Duration * 5)
				{
					_graphSystem.Set.SetData(data.MixerNode, MixerNode.KernelPorts.Weight, 0);

					timer.RefreshTime = clipBuffer[e][0].Clip.Value.Duration * 5 +
					                    clipBuffer[e][1].Clip.Value.Duration + .8f;
				}
				else
				{
					_graphSystem.Set.SetData(data.MixerNode, MixerNode.KernelPorts.Weight, 1);
				}

				timer.Ticks = timer.Ticks + deltaTime;

				timer.Ticks = math.select(timer.Ticks, 0,
					timer.Ticks > timer.RefreshTime);
			}).Run();

		//Removing Graph

		Entities
			.WithNone<PlayerShootReloadRuntime>()
			.WithName("RemoveGraph")
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref PlayerShootReloadDataRuntime animationHandle) =>
				DestroyGraph(e, _graphSystem, ref animationHandle)).Run();
	}

	protected override void OnDestroy()
	{
		if (_graphSystem == null)
			return;

		Entities
			.WithName("DestroyGraph")
			.WithoutBurst()
			.WithStructuralChanges()
			.ForEach((Entity e, ref PlayerShootReloadDataRuntime data) => DestroyGraph(e, _graphSystem, ref data))
			.Run();


		if (_graphSystem.RefCount > 0)
			_graphSystem.RemoveRef();

		base.OnDestroy();
	}

	private void CreateGraph(Entity e, ref Rig rig, ProcessDefaultAnimationGraph graphSystem)
	{
		if (!EntityManager.HasComponent<ShootReload_PlayClipBuffer>(e))
			return;

		var ecb = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer();

		var set = graphSystem.Set;
		var data = new PlayerShootReloadDataRuntime();
		data.DeltaTimeNode = set.Create<ConvertDeltaTimeToFloatNode>();
		data.ComponentNode = set.CreateComponentNode(e);

		var clipBuffer = EntityManager.GetBuffer<ShootReload_PlayClipBuffer>(e);
		Assert.AreNotEqual(clipBuffer.Length, 0);

		var clipPlayerNodes = new NativeArray<NodeHandle<ClipPlayerNode>>(clipBuffer.Length, Allocator.Temp);

		data.MixerNode = set.Create<MixerNode>();

		clipPlayerNodes[0] = set.Create<ClipPlayerNode>();
		clipPlayerNodes[1] = set.Create<ClipPlayerNode>();
		set.SetData(clipPlayerNodes[0], ClipPlayerNode.KernelPorts.Speed, 1.0f);
		set.SetData(clipPlayerNodes[1], ClipPlayerNode.KernelPorts.Speed, 1.0f);

		set.Connect(data.ComponentNode, data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
		set.Connect(data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Float, clipPlayerNodes[0],
			ClipPlayerNode.KernelPorts.DeltaTime);
		set.Connect(data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Float, clipPlayerNodes[1],
			ClipPlayerNode.KernelPorts.DeltaTime);
		set.Connect(clipPlayerNodes[0], ClipPlayerNode.KernelPorts.Output, data.MixerNode,
			MixerNode.KernelPorts.Input0);
		set.Connect(clipPlayerNodes[1], ClipPlayerNode.KernelPorts.Output, data.MixerNode,
			MixerNode.KernelPorts.Input1);
		//set.Connect(set.Create<BlendNode>(), BlendNode.KernelPorts.Weight, data.MixerNode, MixerNode.KernelPorts.Weight);
		set.Connect(data.MixerNode, MixerNode.KernelPorts.Output, data.ComponentNode,
			NodeSetAPI.ConnectionType.Feedback);

		set.SendMessage(clipPlayerNodes[0], ClipPlayerNode.SimulationPorts.Configuration,
			new ClipConfiguration {Mask = ClipConfigurationMask.LoopTime});
		set.SendMessage(clipPlayerNodes[1], ClipPlayerNode.SimulationPorts.Configuration,
			new ClipConfiguration {Mask = ClipConfigurationMask.LoopTime});
		set.SendMessage(clipPlayerNodes[0], ClipPlayerNode.SimulationPorts.Rig, rig);
		set.SendMessage(clipPlayerNodes[1], ClipPlayerNode.SimulationPorts.Rig, rig);
		set.SendMessage(clipPlayerNodes[0], ClipPlayerNode.SimulationPorts.Clip, clipBuffer[0].Clip);
		set.SendMessage(clipPlayerNodes[1], ClipPlayerNode.SimulationPorts.Clip, clipBuffer[1].Clip);
		set.SendMessage(data.MixerNode, MixerNode.SimulationPorts.Rig, rig);
		set.SetData(data.MixerNode, MixerNode.KernelPorts.Weight, 0);

		var clipNodeBuffer = ecb.AddBuffer<PlayerShootReloadClipNodeBuffer>(e);
		for (byte i = 0; i < clipPlayerNodes.Length; i++)
			clipNodeBuffer.Add(new PlayerShootReloadClipNodeBuffer {ClipNode = clipPlayerNodes[i]});

		ecb.AddComponent(e, data);
	}

	private void DestroyGraph(Entity e, [NotNull] ProcessDefaultAnimationGraph graphSystem, ref PlayerShootReloadDataRuntime data)
	{
		if (!EntityManager.HasComponent<PlayerShootReloadClipNodeBuffer>(e))
			throw new InvalidExpressionException("entity missing PlayerShootReloadDataAssetRuntime");

		var set = graphSystem.Set;

		var clipNodeBuffer =
			EntityManager.GetBuffer<PlayerShootReloadClipNodeBuffer>(e);

		for (byte i = 0; i < clipNodeBuffer.Length; i++)
			set.Destroy(clipNodeBuffer[i].ClipNode);

		EntityManager.RemoveComponent<PlayerShootReloadClipNodeBuffer>(e);

		set.Destroy(data.ComponentNode);
		set.Destroy(data.DeltaTimeNode);
		set.Destroy(data.MixerNode);
	}
}