using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.DataFlowGraph;

public sealed class ConvertDeltaTimeToFloatNode : KernelNodeDefinition<ConvertDeltaTimeToFloatNode.KernelDefs>
{
	public struct KernelDefs : IKernelPortDefinition
	{
		public DataInput<ConvertDeltaTimeToFloatNode, DeltaTimeRuntime> Input; //IComponentData Structure
		public DataOutput<ConvertDeltaTimeToFloatNode, float> Float;
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
			ctx.Resolve(ref ports.Float) = ctx.Resolve(ports.Input).Value;
		}
	}
}