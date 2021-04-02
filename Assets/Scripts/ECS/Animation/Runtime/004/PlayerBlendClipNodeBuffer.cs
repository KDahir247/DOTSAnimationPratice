using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

//Delay entity destruction till Handle/s are destroyed/clean-up from the animation graph and system component is removed due to ISystemState type.
public struct PlayerBlendClipNodeBuffer : ISystemStateBufferElementData
{
	public NodeHandle<ClipPlayerNode> ClipNode;
}