using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct RotateCube_PlayStateRuntime : IComponentData
{
	public NodeHandle<ClipPlayerNode> NodeHandle;
	public NodeHandle<ConvertDeltaTimeToFloatNode> DeltaTimeNode;
	public NodeHandle<ComponentNode> EntityNode;
}