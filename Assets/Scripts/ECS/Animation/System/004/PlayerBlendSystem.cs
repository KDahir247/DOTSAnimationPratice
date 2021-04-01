using Unity.Animation;
using Unity.Assertions;
using Unity.Collections;
using Unity.DataFlowGraph;
using Unity.Entities;

public sealed class PlayerBlendSystem : PlayerBlendSystemBase
{
	protected override void CreateGraph(Entity e, ref Rig rig, in PlayerBlendKernelRuntime kernelBlendNode)
	{
		PlayerBlendDataRuntime data = new PlayerBlendDataRuntime {Graph = graphSystem.CreateGraph()};

		data.DeltaTimeFloatNode = graphSystem.CreateNode<ConvertDeltaTimeToFloatNode>(data.Graph);
		data.BlendNode = graphSystem.CreateNode<BlendNode>(data.Graph);
		data.MixerNode = graphSystem.CreateNode<MixerNode>(data.Graph);
		data.ComponentNode = graphSystem.CreateNode(data.Graph, e);


		DynamicBuffer<PlayerBlendClipBuffer> clipBuffer = EntityManager.GetBuffer<PlayerBlendClipBuffer>(e);
		Assert.AreNotEqual(clipBuffer.Length,0);

		NativeArray<NodeHandle<ClipPlayerNode>> clipPlayerNodes =
			new NativeArray<NodeHandle<ClipPlayerNode>>(clipBuffer.Length, Allocator.Temp);

		clipPlayerNodes[0] = graphSystem.CreateNode<ClipPlayerNode>(data.Graph);
		clipPlayerNodes[1] = graphSystem.CreateNode<ClipPlayerNode>(data.Graph);

		NodeSet set = graphSystem.Set;

		//Set ClipPlayerNode animation speed
		set.SetData(clipPlayerNodes[0], ClipPlayerNode.KernelPorts.Speed, 1.0f);
		set.SetData(clipPlayerNodes[1], ClipPlayerNode.KernelPorts.Speed, 1.0f);

		//DeltaTime Connect
		set.Connect(data.ComponentNode, data.DeltaTimeFloatNode,ConvertDeltaTimeToFloatNode.KernelPorts.Input);
		set.Connect(data.DeltaTimeFloatNode, ConvertDeltaTimeToFloatNode.KernelPorts.Float, clipPlayerNodes[0], ClipPlayerNode.KernelPorts.DeltaTime);
		set.Connect(data.DeltaTimeFloatNode, ConvertDeltaTimeToFloatNode.KernelPorts.Float, clipPlayerNodes[1], ClipPlayerNode.KernelPorts.DeltaTime);

		//Mixer clips Connect
		set.Connect(clipPlayerNodes[0],ClipPlayerNode.KernelPorts.Output, data.MixerNode, MixerNode.KernelPorts.Input0);
		set.Connect(clipPlayerNodes[1],ClipPlayerNode.KernelPorts.Output, data.MixerNode, MixerNode.KernelPorts.Input1);

		//Blend amount Connect
		set.Connect(data.ComponentNode, data.BlendNode, BlendNode.KernelPorts.Input);
		set.Connect(data.BlendNode, BlendNode.KernelPorts.Weight, data.MixerNode, MixerNode.KernelPorts.Weight);

		//Mixer to Component Connect
		set.Connect(data.MixerNode, MixerNode.KernelPorts.Output, data.ComponentNode, NodeSetAPI.ConnectionType.Feedback);

		//Clip Configurations
		set.SendMessage(clipPlayerNodes[0],ClipPlayerNode.SimulationPorts.Configuration, new ClipConfiguration(){Mask = ClipConfigurationMask.LoopTime});
		set.SendMessage(clipPlayerNodes[1], ClipPlayerNode.SimulationPorts.Configuration,
			new ClipConfiguration() {Mask = ClipConfigurationMask.LoopTime});

		//Passing model Rig (bones, joint, etc..) to the ClipPlayerNode
		set.SendMessage(clipPlayerNodes[0], ClipPlayerNode.SimulationPorts.Rig, rig);
		set.SendMessage(clipPlayerNodes[1], ClipPlayerNode.SimulationPorts.Rig, rig);

		//Setting Clip for ClipPlayerNode
		set.SendMessage(clipPlayerNodes[0], ClipPlayerNode.SimulationPorts.Clip, clipBuffer[0].Clip);
		set.SendMessage(clipPlayerNodes[1], ClipPlayerNode.SimulationPorts.Clip, clipBuffer[1].Clip);

		//Passing model Rig (bones, joint, etc..) to the MixerNode
		set.SendMessage(data.MixerNode, MixerNode.SimulationPorts.Rig, rig);

		DynamicBuffer<PlayerBlendClipNodeBuffer> clipNodeBuffer = EntityManager.AddBuffer<PlayerBlendClipNodeBuffer>(e);
		for (byte i = 0; i < clipNodeBuffer.Length; i++) clipNodeBuffer.Add(new PlayerBlendClipNodeBuffer() {ClipNode = clipPlayerNodes[i]});

		EntityManager.AddComponentData(e, data);
	}

	protected override void DestroyGraph(Entity e, ref PlayerBlendDataRuntime playerBlendData)
	{
		if (!EntityManager.HasComponent<PlayerBlendClipNodeBuffer>(e))
			return;

		NodeSet set = graphSystem.Set;

		DynamicBuffer<PlayerBlendClipNodeBuffer> clipNodeBuffer = EntityManager.AddBuffer<PlayerBlendClipNodeBuffer>(e);

		for (byte i = 0; i < clipNodeBuffer.Length; i++)
		{
			set.Destroy(clipNodeBuffer[i].ClipNode);
		}

		EntityManager.RemoveComponent<PlayerBlendClipNodeBuffer>(e);

		set.Destroy(playerBlendData.ComponentNode);
		set.Destroy(playerBlendData.DeltaTimeFloatNode);
		set.Destroy(playerBlendData.BlendNode);
		if(playerBlendData.MixerNode != null)
			set.Destroy(playerBlendData.MixerNode);

	}
}
