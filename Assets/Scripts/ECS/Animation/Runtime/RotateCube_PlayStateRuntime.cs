using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct RotateCube_PlayStateRuntime : IComponentData
{
	public GraphHandle GraphHandle;
	public NodeHandle<ClipPlayerNode> NodeHandle;
}
