using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.DataFlowGraph;
using Unity.Mathematics;

public sealed class Blend1DExtractParameterNode : KernelNodeDefinition<Blend1DExtractParameterNode.KernelDefs>
{
	public struct KernelDefs : IKernelPortDefinition
	{
		public DataInput<Blend1DExtractParameterNode, BlendTree1DParamRuntime> Input;
		public DataOutput<Blend1DExtractParameterNode, float> Output;
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
			ctx.Resolve(ref ports.Output) = math.clamp(ctx.Resolve(ports.Input).VelocityX, 0, 1);
		}
	}
}