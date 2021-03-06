using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public sealed class BlendTree1DSystem : BlendTree1DSystemBase
{
	protected override void CreateGraph(Entity entity, ref Rig rig, ref BlendTree1DRuntime blendTreeData)
	{
		var ecb = EcbSystem.CreateCommandBuffer();
		var nodeSet = GraphSystem.Set;

		//Created Node
		var data = new BlendTree1DKernelRuntime
		{
			BlendTreeNode = nodeSet.Create<BlendTree1DNode>(),
			ComponentNode = nodeSet.CreateComponentNode(entity),
			DeltaTimeNode = nodeSet.Create<ConvertDeltaTimeToFloatNode>(),
			InputNode = nodeSet.Create<Blend1DExtractParameterNode>(),
			FloatRcpNode = nodeSet.Create<FloatRcpNode>(),
			TimeCounterNode = nodeSet.Create<TimeCounterNode>(),
			TimeLoopNode = nodeSet.Create<TimeLoopNode>()
		};

		//Connected Node
		nodeSet.Connect(data.ComponentNode, data.DeltaTimeNode,
			ConvertDeltaTimeToFloatNode.KernelPorts
				.Input); //Connect the ComponentNode to the DeltaTime and passes update on component to DeltaTimeNode input
		nodeSet.Connect(data.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Float, data.TimeCounterNode,
			TimeCounterNode.KernelPorts
				.DeltaTime); //DeltaTime Node passes the deltaTime (float) output to TimeCounterNode Input Time
		nodeSet.Connect(data.TimeCounterNode, TimeCounterNode.KernelPorts.Time, data.TimeLoopNode,
			TimeLoopNode.KernelPorts
				.InputTime); //TimeCounter Node passes The Time outpit to the TimeLoopNode InputTime input
		nodeSet.Connect(data.TimeLoopNode, TimeLoopNode.KernelPorts.OutputTime, data.BlendTreeNode,
			BlendTree1DNode.KernelPorts
				.NormalizedTime); //TimeLoopNode passes the OutputTime output to the BlendTreeNode normalizedTime Input

		nodeSet.Connect(data.BlendTreeNode, BlendTree1DNode.KernelPorts.Duration, data.FloatRcpNode,
			FloatRcpNode.KernelPorts.Input); //BlendTreeNode passes the Duration output to the FloatRcpNode Input
		nodeSet.Connect(data.FloatRcpNode, FloatRcpNode.KernelPorts.Output, data.TimeCounterNode,
			TimeCounterNode.KernelPorts.Speed); //FloatRcpNode passes the output to the TimeCounterNode Speed Input

		nodeSet.Connect(data.BlendTreeNode, BlendTree1DNode.KernelPorts.Output, data.ComponentNode,
			NodeSetAPI.ConnectionType.Feedback);
		nodeSet.Connect(data.ComponentNode, data.InputNode, Blend1DExtractParameterNode.KernelPorts.Input,
			NodeSetAPI.ConnectionType.Feedback);
		nodeSet.Connect(data.InputNode, Blend1DExtractParameterNode.KernelPorts.Output, data.BlendTreeNode,
			BlendTree1DNode.KernelPorts
				.BlendParameter); //InputNode passes the Output to the BlendTreeNode BlendParameter output

		//Send Message to Update Node param
		nodeSet.SendMessage(data.TimeLoopNode, TimeLoopNode.SimulationPorts.Duration, 1.0f);
		nodeSet.SendMessage(data.BlendTreeNode, BlendTree1DNode.SimulationPorts.Rig, rig);
		nodeSet.SendMessage(data.BlendTreeNode, BlendTree1DNode.SimulationPorts.BlendTree, blendTreeData.BlendTree);

		ecb.AddComponent(entity, data);
	}

	protected override void DestroyGraph(Entity entity, ref BlendTree1DKernelRuntime nodeHandle)
	{
		var ecb = EcbSystem.CreateCommandBuffer();

		var nodeSet = GraphSystem.Set;
		nodeSet.Destroy(nodeHandle.TimeCounterNode);
		nodeSet.Destroy(nodeHandle.DeltaTimeNode);
		nodeSet.Destroy(nodeHandle.FloatRcpNode);
		nodeSet.Destroy(nodeHandle.BlendTreeNode);
		nodeSet.Destroy(nodeHandle.InputNode);
		nodeSet.Destroy(nodeHandle.ComponentNode);
		nodeSet.Destroy(nodeHandle.TimeLoopNode);

		ecb.RemoveComponent<BlendTree1DKernelRuntime>(entity);
	}
}