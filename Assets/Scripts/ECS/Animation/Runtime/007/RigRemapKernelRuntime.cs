using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct RigRemapKernelRuntime : ISystemStateComponentData
{
	public NodeHandle<ConvertDeltaTimeToFloatNode> DeltaTimeNode;
	public NodeHandle<ClipPlayerNode> ClipPlayerNode;
	public NodeHandle<RigRemapperNode> RemapperNode;

	public NodeHandle<ComponentNode> EntityNode;
}