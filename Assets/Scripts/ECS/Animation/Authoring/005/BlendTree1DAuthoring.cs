using JetBrains.Annotations;
using Unity.Animation;
using Unity.Animation.Hybrid;
using Unity.Entities;
using UnityEditor.Animations;
using UnityEngine;

[ConverterVersion("BlendTree1D", 1)]
[RequireComponent(typeof(RigComponent))]
public sealed class BlendTree1DAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
	[SerializeField] private BlendTree blendTree;

	[SerializeField] private float velocityStep = .2f;

	public void Convert(Entity entity, EntityManager dstManager, [NotNull] GameObjectConversionSystem conversionSystem)
	{
		conversionSystem.DeclareAssetDependency(gameObject, blendTree);

		var rigComponent = dstManager.GetComponentData<Rig>(entity);
		var clipConfiguration = new ClipConfiguration {Mask = ClipConfigurationMask.LoopValues};
		var bakeOptions = new BakeOptions
			{RigDefinition = rigComponent.Value, ClipConfiguration = clipConfiguration, SampleRate = 60.0f};

		var blendTreeIndex = BlendTreeConversion.Convert(blendTree, entity, dstManager, bakeOptions);

		var blendTreeComponents = dstManager.GetBuffer<BlendTree1DResource>(entity);
		var blobBlendTreeRef =
			BlendTreeBuilder.CreateBlendTree1DFromComponents(blendTreeComponents[blendTreeIndex], dstManager, entity);

		conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref blobBlendTreeRef);

		var blendTreeRuntime = new BlendTree1DRuntime
		{
			BlendTree = blobBlendTreeRef
		};

		dstManager.AddComponentData(entity, blendTreeRuntime);

		var blendTreeParamRuntime = new BlendTree1DParamRuntime
		{
			VelocityStep = velocityStep,
			VelocityX = 0.0f
		};

		dstManager.AddComponentData(entity, blendTreeParamRuntime);

		dstManager.AddComponent<DeltaTimeRuntime>(entity);
	}
}