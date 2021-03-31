using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct PlayerShootReloadDataAssetRuntime : ISystemStateBufferElementData
{
	public NodeHandle<ClipPlayerNode> ClipNode;
}
