using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Animation;
using Unity.Animation.Hybrid;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[ConverterVersion("RetargetRig", 1)]
[RequireComponent(typeof(RigComponent))]
public sealed class RetargetRigAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
	[SerializeField] private GameObject rigSrcPrefab;

	[SerializeField] private AnimationClip clip;

	public void Convert(Entity entity, EntityManager dstManager, [NotNull] GameObjectConversionSystem conversionSystem)
	{
		var dstRig = GetComponent<RigComponent>();
		var srcRig = rigSrcPrefab.GetComponent<RigComponent>();
		var query = CreateAutoRemapQuery(srcRig, dstRig);

		var srcRigPrefab = conversionSystem.GetPrimaryEntity(rigSrcPrefab);
		var srcRigDefinition = dstManager.GetComponentData<Rig>(srcRigPrefab);
		var dstRigDefinition = dstManager.GetComponentData<Rig>(entity);

		var setup = new RigRemapRuntime
		{
			SrcClip = clip.ToDenseClip(),
			SrcRig = srcRigDefinition.Value,
			RemapTable = query.ToRigRemapTable(srcRigDefinition.Value, dstRigDefinition.Value)
		};

		dstManager.AddComponentData(entity, setup);
		dstManager.AddComponent<DeltaTimeRuntime>(entity);
	}

	public void DeclareReferencedPrefabs([NotNull] List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(rigSrcPrefab);
	}


	//SrcRig is the rig we want to remap to.
	//dstRig is the rig that will be overwritten by the SrcRig.
	//NOTE that both the naming of the srcRig and dstRig must be identical (Create a standard naming convention for the bones).
	[NotNull]
	private RigRemapQuery CreateAutoRemapQuery([NotNull] RigComponent srcRig, [NotNull] RigComponent dstRig)
	{
		var translationChannels = new List<ChannelMap>();
		var rotationChannels = new List<ChannelMap>();

		var translationOffsets = new List<RigTranslationOffset>();
		var rotationOffsets = new List<RigRotationOffset>();

		var srcRootRotInv = math.inverse(srcRig.transform.rotation);
		var dstRootRotInv = math.inverse(dstRig.transform.rotation);

		for (var boneIter = 0; boneIter < srcRig.Bones.Length; boneIter++)
			if (srcRig.Bones[boneIter].parent != null)
				if (srcRig.Bones[boneIter].name == dstRig.Bones[boneIter].name)
				{
					var srcPath = RigGenerator.ComputeRelativePath(srcRig.Bones[boneIter], srcRig.transform);
					var dstPath = RigGenerator.ComputeRelativePath(dstRig.Bones[boneIter], dstRig.transform);

					var dstParentRot = math.mul(dstRootRotInv, dstRig.Bones[boneIter].parent.transform.rotation);
					var srcParentRot = math.mul(srcRootRotInv, srcRig.Bones[boneIter].parent.transform.rotation);
					var rotation = mathex.mul(math.inverse(dstParentRot), srcParentRot);

					//Got to calculate the translation.
					//Only applying translationOffset to the first bone (most cases the hips)
					if (boneIter == 1)
					{
						var translationOffset = new RigTranslationOffset
						{
							Scale = dstRig.Bones[boneIter].position.y / srcRig.Bones[boneIter].position.y,
							Rotation = rotation
						};

						translationOffsets.Add(translationOffset);
						translationChannels.Add(new ChannelMap
							{DestinationId = dstPath, SourceId = srcPath, OffsetIndex = translationOffsets.Count});
					}

					//Got to calculate the rotation.
					var dstRot = math.mul(dstRootRotInv, dstRig.Bones[boneIter].transform.rotation);
					var srcRot = math.mul(srcRootRotInv, srcRig.Bones[boneIter].transform.rotation);

					var rotationOffset = new RigRotationOffset
					{
						PreRotation = rotation,
						PostRotation = mathex.mul(math.inverse(srcRot), dstRot)
					};

					rotationOffsets.Add(rotationOffset);
					rotationChannels.Add(new ChannelMap
						{SourceId = srcPath, DestinationId = dstPath, OffsetIndex = rotationOffsets.Count});
				}

		var rigRemapQuery = new RigRemapQuery
		{
			TranslationChannels = new ChannelMap[translationChannels.Count],
			RotationChannels = new ChannelMap[rotationChannels.Count],
			TranslationOffsets = new RigTranslationOffset[translationChannels.Count + 1],
			RotationOffsets = new RigRotationOffset[rotationChannels.Count + 1]
		};

		rigRemapQuery.TranslationOffsets[0] = new RigTranslationOffset();
		rigRemapQuery.RotationOffsets[0] = new RigRotationOffset();

		for (var iter = 0; iter < translationChannels.Count; iter++)
		{
			rigRemapQuery.TranslationChannels[iter] = translationChannels[iter];
			rigRemapQuery.TranslationOffsets[iter + 1] = translationOffsets[iter];
		}

		for (var iter = 0; iter < rotationChannels.Count; iter++)
		{
			rigRemapQuery.RotationChannels[iter] = rotationChannels[iter];
			rigRemapQuery.RotationOffsets[iter + 1] = rotationOffsets[iter];
		}

		return rigRemapQuery;
	}
}