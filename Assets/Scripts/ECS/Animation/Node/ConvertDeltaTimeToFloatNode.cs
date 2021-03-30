
using Unity.Animation;
using Unity.Burst;
using Unity.DataFlowGraph;

public class ConvertDeltaTimeToFloatNode : ConvertToBase<ConvertDeltaTimeToFloatNode,DeltaTimeRuntime,float,ConvertDeltaTimeToFloatNode.Kernel>
{
	[BurstCompile]
	public struct Kernel : IGraphKernel<KernelData, KernelDefs>
	{
		public void Execute(RenderContext ctx, in KernelData data, ref KernelDefs ports)
		{
			ctx.Resolve(ref ports.Output) = ctx.Resolve(ports.Input).Value;
		}
	}
}
