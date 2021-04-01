using Unity.Animation;
using Unity.DataFlowGraph;
using Unity.Entities;

//Delay entity destruction till Handle/s are destroyed/clean-up from the animation graph and system component is removed due to ISystemState type.
public struct PlayerBlendDataRuntime : ISystemStateComponentData
{
	public GraphHandle Graph;
	public NodeHandle<MixerNode> MixerNode;
	public NodeHandle<ConvertDeltaTimeToFloatNode> DeltaTimeFloatNode;
	public NodeHandle<BlendNode> BlendNode;
	public NodeHandle<ComponentNode> ComponentNode;
}
