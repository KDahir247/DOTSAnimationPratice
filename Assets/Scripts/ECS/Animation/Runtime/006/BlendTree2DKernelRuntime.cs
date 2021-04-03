using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct BlendTree2DKernelRuntime : ISystemStateComponentData
{
	public NodeHandle<ComponentNode> ComponentNode;
	public NodeHandle<ConvertDeltaTimeToFloatNode> DeltaTimeNode;
	public NodeHandle<BlendTree2DNode> BlendTree2DNode;
	public NodeHandle<TimeCounterNode> TimeCounterNode;
	public NodeHandle<TimeLoopNode> TimeLoopNode;
	public NodeHandle<FloatRcpNode> RcpNode;
	public NodeHandle<Blend2DExtractParameterNode> InputNode;
}