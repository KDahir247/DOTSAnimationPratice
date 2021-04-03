using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.DataFlowGraph;

public class Blend2DExtractParameterNode : KernelNodeDefinition<Blend2DExtractParameterNode.KernelDefs>
{
	public struct KernelDefs : IKernelPortDefinition
	{
		public DataInput<Blend2DExtractParameterNode, BlendTree2DParamRuntime> Input;
		public DataOutput<Blend2DExtractParameterNode, float> ParameterX;
		public DataOutput<Blend2DExtractParameterNode, float> ParameterY;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct KernelData : IKernelData
	{
	}

	[BurstCompile]
	public struct Kernel : IGraphKernel<KernelData, KernelDefs>
	{
		public void Execute(RenderContext ctx, in KernelData data, ref KernelDefs ports)
		{
			ctx.Resolve(ref ports.ParameterX) = ctx.Resolve(ports.Input).InputMapping.x;
			ctx.Resolve(ref ports.ParameterY) = ctx.Resolve(ports.Input).InputMapping.y;
		}
	}
}