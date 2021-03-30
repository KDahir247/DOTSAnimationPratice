using Unity.Animation;
using Unity.Burst;
using Unity.DataFlowGraph;

public class ConvertDeltaTimeToFloatNode : ConvertToBase<ConvertDeltaTimeToFloatNode,DeltaTimeRuntime,float,ConvertDeltaTimeToFloatNode.Kernel>
{
	[BurstCompile]
	//Create a animation graph kernel with a tag KernelData and a port to represent as the input and output of the animation graph node.
	public struct Kernel : IGraphKernel<KernelData, KernelDefs>
	{
		//Execution of the animation graph node.
		public void Execute(RenderContext ctx, in KernelData _, ref KernelDefs ports)
		{
			//the output kernel port is the is the resolved kernel port input (DeltaTimeRuntime) Value (float)
			ctx.Resolve(ref ports.Output) = ctx.Resolve(ports.Input).Value;
		}
	}
}
