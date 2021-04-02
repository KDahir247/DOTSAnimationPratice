using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct BlendTree1DKernelRuntime : ISystemStateComponentData
{
	public NodeHandle<TimeCounterNode>
		TimeCounterNode; //Node take a float (DeltaTime) and a float (speed) and return OutputDeltaTime(DeltaTime * Speed) and the accumulated time in its memory. Required by TimeLoop Node input (not equal to elapsed time)

	public NodeHandle<TimeLoopNode>
		TimeLoopNode; //Node take a float (Time) and return a OutputTime (normailzedTime * Duration), normalizedTime(InputTime/Duration), and the Cycle.

	public NodeHandle<ConvertDeltaTimeToFloatNode>
		DeltaTimeNode; //Node take DeltaTimeRuntime component and return the float representation of delta time as the output.

	public NodeHandle<FloatRcpNode>
		FloatRcpNode; //Node take float input and return the reciprocal of the float as the output (1 / f).

	public NodeHandle<BlendTree1DNode>
		BlendTreeNode; //Node take in a blob reference of BlendTree1D, rig and return the blob reference of BlendTree, rig, and the BlendTree information. It also create and connects new animation node and destroy it.

	public NodeHandle<Blend1DExtractParameterNode>
		InputNode; //Node take a BlendTree1DParamRuntime component and return a clamped (0,1) float representation of the current threshold on the animationBlend Parameter component.

	public NodeHandle<ComponentNode> ComponentNode;
}