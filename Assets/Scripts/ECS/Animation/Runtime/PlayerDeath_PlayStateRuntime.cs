using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct PlayerDeath_PlayStateRuntime : IComponentData
{
	public GraphHandle Graph;
	public NodeHandle<ClipPlayerNode> ClipPlayerNode;
}
