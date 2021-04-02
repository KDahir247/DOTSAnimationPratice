using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct PlayerDeath_PlayStateRuntime : IComponentData
{
	public NodeHandle<ClipPlayerNode> ClipPlayerNode;
	public NodeHandle<ConvertDeltaTimeToFloatNode> DeltaTimeNode;
	public NodeHandle<ComponentNode> EntityNode;
}