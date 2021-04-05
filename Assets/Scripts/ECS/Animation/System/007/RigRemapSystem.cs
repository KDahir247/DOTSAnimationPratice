using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public sealed class RigRemapSystem : RigRemapSystemBase
{
	protected override void CreateGraph(Entity e, ref Rig rig, ref RigRemapRuntime setup)
	{
		var ecb = EcbSystem.CreateCommandBuffer();
		var set = GraphSystem.Set;

		var data = new RigRemapKernelRuntime
		{
			ClipPlayerNode = set.Create<ClipPlayerNode>(),
			DeltaTimeNode = set.Create<ConvertDeltaTimeToFloatNode>(),
			EntityNode = set.CreateComponentNode(e),
			RemapperNode = set.Create<RigRemapperNode>()
		};

		set.SetData(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Speed, 1.0f);

		set.Connect(data.EntityNode, data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
		set.Connect(data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Float, data.ClipPlayerNode,
			ClipPlayerNode.KernelPorts.DeltaTime);
		set.Connect(data.ClipPlayerNode, ClipPlayerNode.KernelPorts.Output, data.RemapperNode,
			RigRemapperNode.KernelPorts.Input);
		set.Connect(data.RemapperNode, RigRemapperNode.KernelPorts.Output, data.EntityNode,
			NodeSetAPI.ConnectionType.Feedback);

		set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Configuration,
			new ClipConfiguration {Mask = ClipConfigurationMask.LoopTime});

		//The playerClipNode Rig input will retrieve the srcRig that we want to copy since the animation clip is mapped to that rig.
		set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Rig, new Rig {Value = setup.SrcRig});
		set.SendMessage(data.ClipPlayerNode, ClipPlayerNode.SimulationPorts.Clip, setup.SrcClip);
		//The Rig we want to remap to
		set.SendMessage(data.RemapperNode, RigRemapperNode.SimulationPorts.SourceRig, new Rig {Value = setup.SrcRig});
		//The Rig that we are currently mapped to
		set.SendMessage(data.RemapperNode, RigRemapperNode.SimulationPorts.DestinationRig, rig);
		set.SendMessage(data.RemapperNode, RigRemapperNode.SimulationPorts.RemapTable, setup.RemapTable);

		ecb.AddComponent(e, data);
	}

	protected override void DestroyGraph(Entity e, ref RigRemapKernelRuntime nodeHandles)
	{
		var ecb = EcbSystem.CreateCommandBuffer();

		var set = GraphSystem.Set;

		set.Destroy(nodeHandles.EntityNode);
		set.Destroy(nodeHandles.RemapperNode);
		set.Destroy(nodeHandles.ClipPlayerNode);
		set.Destroy(nodeHandles.DeltaTimeNode);

		ecb.RemoveComponent<RigRemapKernelRuntime>(e);
	}
}