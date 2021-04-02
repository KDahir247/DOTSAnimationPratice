using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.DataFlowGraph;
using Unity.Mathematics;

public sealed class BlendNode : KernelNodeDefinition<BlendNode.KernelDefs>
{
	public struct KernelDefs : IKernelPortDefinition
	{
		public DataInput<BlendNode, PlayerBlendKernelRuntime> Input; //IComponentData Structure
		public DataOutput<BlendNode, float> Weight; //the values in the struct;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct KernelData : IKernelData
	{
	}

	[BurstCompile]
	public struct Kernel : IGraphKernel<KernelData, KernelDefs>
	{
		public void Execute(RenderContext ctx, in KernelData _, ref KernelDefs ports)
		{
			ctx.Resolve(ref ports.Weight) = math.clamp(ctx.Resolve(ports.Input).BlendAmount, 0, 1);
		}
	}
}