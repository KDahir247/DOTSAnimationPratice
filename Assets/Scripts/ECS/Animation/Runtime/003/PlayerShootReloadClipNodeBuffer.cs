using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

public struct PlayerShootReloadClipNodeBuffer : ISystemStateBufferElementData
{
	public NodeHandle<ClipPlayerNode> ClipNode;
}
