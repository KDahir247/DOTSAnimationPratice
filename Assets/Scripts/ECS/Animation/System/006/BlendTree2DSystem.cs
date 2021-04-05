using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public sealed class BlendTree2DSystem : BlendTree2DSystemBase
{
	protected override void CreateGraph(Entity entity, ref Rig rig, ref BlendTree2DRuntime blendTreeData)
	{
		CreateDebugBoneRig(entity, ref rig);
		var ecb = EsBufferSystem.CreateCommandBuffer();

		//Create graph and hook up node a set send message to the correct node
		var set = GraphSystem.Set;

		var kernelData = new BlendTree2DKernelRuntime
		{
			ComponentNode = set.CreateComponentNode(entity),
			DeltaTimeNode = set.Create<ConvertDeltaTimeToFloatNode>(),
			BlendTree2DNode = set.Create<BlendTree2DNode>(),
			TimeCounterNode = set.Create<TimeCounterNode>(),
			TimeLoopNode = set.Create<TimeLoopNode>(),
			RcpNode = set.Create<FloatRcpNode>(),
			InputNode = set.Create<Blend2DExtractParameterNode>()
		};

		set.Connect(kernelData.ComponentNode, kernelData.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Input);
		set.Connect(kernelData.DeltaTimeNode, ConvertDeltaTimeToFloatNode.KernelPorts.Float, kernelData.TimeCounterNode,
			TimeCounterNode.KernelPorts.DeltaTime);
		set.Connect(kernelData.TimeCounterNode, TimeCounterNode.KernelPorts.Time, kernelData.TimeLoopNode,
			TimeLoopNode.KernelPorts.InputTime);
		set.Connect(kernelData.TimeLoopNode, TimeLoopNode.KernelPorts.OutputTime, kernelData.BlendTree2DNode,
			BlendTree2DNode.KernelPorts.NormalizedTime);

		set.Connect(kernelData.BlendTree2DNode, BlendTree2DNode.KernelPorts.Duration, kernelData.RcpNode,
			FloatRcpNode.KernelPorts.Input);
		set.Connect(kernelData.RcpNode, FloatRcpNode.KernelPorts.Output, kernelData.TimeCounterNode,
			TimeCounterNode.KernelPorts.Speed);

		set.Connect(kernelData.BlendTree2DNode, BlendTree2DNode.KernelPorts.Output, kernelData.ComponentNode,
			NodeSetAPI.ConnectionType.Feedback);

		set.Connect(kernelData.ComponentNode, kernelData.InputNode, Blend2DExtractParameterNode.KernelPorts.Input,
			NodeSetAPI.ConnectionType.Feedback);

		set.Connect(kernelData.InputNode, Blend2DExtractParameterNode.KernelPorts.ParameterX,
			kernelData.BlendTree2DNode, BlendTree2DNode.KernelPorts.BlendParameterX);
		set.Connect(kernelData.InputNode, Blend2DExtractParameterNode.KernelPorts.ParameterY,
			kernelData.BlendTree2DNode, BlendTree2DNode.KernelPorts.BlendParameterY);
		//Send Messages to Blend2DNode

		set.SendMessage(kernelData.TimeLoopNode, TimeLoopNode.SimulationPorts.Duration, 1.0f);
		set.SendMessage(kernelData.BlendTree2DNode, BlendTree2DNode.SimulationPorts.Rig, rig);
		set.SendMessage(kernelData.BlendTree2DNode, BlendTree2DNode.SimulationPorts.BlendTree,
			blendTreeData.BlendTreeAsset);

		//Add the kernelData to this entity
		ecb.AddComponent(entity, kernelData);
	}

	private void CreateDebugBoneRig(Entity entity, ref Rig rig)
	{
		var ecb = EsBufferSystem.CreateCommandBuffer();

		var debugEntityRig = RigUtils.InstantiateDebugRigEntity(rig.Value, EntityManager,
			new BoneRendererProperties
			{
				BoneShape = BoneRendererUtils.BoneShape.Line, Color = new float4(0, 1, 0, 1), Size = 1
			});

		ecb.AddComponent(debugEntityRig, new LocalToParent {Value = float4x4.identity});
		ecb.AddComponent(debugEntityRig, new Parent {Value = entity});
	}

	protected override void DestroyGraph(Entity entity, ref BlendTree2DKernelRuntime nodeHandle)
	{
		var ecb = EsBufferSystem.CreateCommandBuffer();

		var nodeSet = GraphSystem.Set;

		nodeSet.Destroy(nodeHandle.ComponentNode);
		nodeSet.Destroy(nodeHandle.DeltaTimeNode);
		nodeSet.Destroy(nodeHandle.BlendTree2DNode);
		nodeSet.Destroy(nodeHandle.TimeCounterNode);
		nodeSet.Destroy(nodeHandle.TimeLoopNode);
		nodeSet.Destroy(nodeHandle.RcpNode);
		nodeSet.Destroy(nodeHandle.InputNode);

		ecb.RemoveComponent<BlendTree2DKernelRuntime>(entity);
	}
}