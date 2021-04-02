using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct PlayerShootReloadDataRuntime : ISystemStateComponentData
{
	public NodeHandle<ConvertDeltaTimeToFloatNode> DeltaTimeNode;
	public NodeHandle<MixerNode> MixerNode;
	public NodeHandle<ComponentNode> ComponentNode;
}