using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Animation;
using Unity.Animation.Hybrid;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[ConverterVersion("RetargetARig", 1)]
[RequireComponent(typeof(RigComponent))]
public sealed class RetargetRigAAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
	[SerializeField] private GameObject sourceRigPrefab;

	[SerializeField] private AnimationClip sourceClip;

	[SerializeField] private List<string> retargetMap;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		var dstRig = GetComponent<RigComponent>();
		var srcRig = sourceRigPrefab.GetComponent<RigComponent>();
		var query = CreateRemapQuery(srcRig, dstRig, retargetMap);

		var srcRigPrefab = conversionSystem.GetPrimaryEntity(sourceRigPrefab);
		var srcRigDefinition = dstManager.GetComponentData<Rig>(srcRigPrefab);
		var dstRigDefinition = dstManager.GetComponentData<Rig>(entity);

		var setup = new RigRemapRuntime
		{
			SrcClip = sourceClip.ToDenseClip(),
			SrcRig = srcRigDefinition.Value,
			RemapTable = query.ToRigRemapTable(srcRigDefinition.Value, dstRigDefinition.Value)
		};

		dstManager.AddComponentData(entity, setup);
		dstManager.AddComponent<DeltaTimeRuntime>(entity);
	}

	public void DeclareReferencedPrefabs([NotNull] List<GameObject> referencedPrefabs)
	{
		referencedPrefabs.Add(sourceRigPrefab);
	}


	private RigRemapQuery CreateRemapQuery(RigComponent srcRig, RigComponent dstRig, List<string> retargetMap)
	{
		var status = "";

		var translationChannels = new List<ChannelMap>();
		var rotationChannels = new List<ChannelMap>();
		var translationOffsets = new List<RigTranslationOffset>();
		var rotationOffsets = new List<RigRotationOffset>();

		//Inverse Quaternion = (x * -1, y * -1, z * -1, w)
		var srcRootRotInv = math.inverse(srcRig.transform.rotation);
		var dstRootRotInv = math.inverse(dstRig.transform.rotation);

		//We are iterating through the retargetMap collection..
		for (var mapIter = 0; mapIter < retargetMap.Count; mapIter++)
		{
			var successFlag = false;

			//Splitting the element in each array when there a space. Three element
			//ex. "First Second Third" = ["First", "Second", "Third"]
			var splitMap = retargetMap[mapIter].Split(new[] {' '}, 3);

			//We are iterating through the for loop on the number of bones the srcPrefab.
			for (var srcBoneIter = 0; srcBoneIter < srcRig.Bones.Length; srcBoneIter++)
				//We are checking if the splitMap element has atleast one element and if the first element matches any bone name of the srcRig prefab bone
				if (splitMap.Length > 0 && splitMap[0] == srcRig.Bones[srcBoneIter].name)
					//We are iterating through the for loop on the number of bones we have on the gameObject attached to this script.
					for (var dstBoneIter = 0; dstBoneIter < dstRig.Bones.Length; dstBoneIter++)
						//We are checking if the splitMap element has atleast two element and if the second element matches any bone name of this gameObject
						if (splitMap.Length > 1 && splitMap[1] == dstRig.Bones[dstBoneIter].name)
							//We are checking if the splitMap has atleast three element
							if (splitMap.Length > 2)
							{
								//We are getting all the parent gameObject name of the current srcRig bone at srcBoneIter up until and excluding the srcRig.transform
								//Ex.
								// if srcRig.Bones[srcBoneIter] = Chest
								// then the srcPath = Skeleton/Root/Spine/Chest (it traverse the hierarchy and get the parent up until the srcRig.transform)
								var srcPath =
									RigGenerator.ComputeRelativePath(srcRig.Bones[srcBoneIter], srcRig.transform);
								var dstPath
									= RigGenerator.ComputeRelativePath(dstRig.Bones[dstBoneIter], dstRig.transform);

								if (splitMap[2] == "TR" || splitMap[2] == "T")
								{
									var translationOffset = new RigTranslationOffset();

									//self bone position.y / target bone position.y to get the average height of the bone (assumed to be y/ standing up right)
									translationOffset.Scale = dstRig.Bones[dstBoneIter].position.y /
									                          srcRig.Bones[srcBoneIter].position.y;

									var dstParentRot =
										math.mul(dstRootRotInv, dstRig.Bones[dstBoneIter].parent.rotation);
									var srcParentRot =
										math.mul(srcRootRotInv, srcRig.Bones[srcBoneIter].parent.rotation);

									translationOffset.Rotation = mathex.mul(math.inverse(dstParentRot), srcParentRot);

									translationOffsets.Add(translationOffset);
									translationChannels.Add(new ChannelMap
									{
										SourceId = srcPath, DestinationId = dstPath,
										OffsetIndex = translationOffsets.Count
									});
								}

								if (splitMap[2] == "TR" || splitMap[2] == "R")
								{
									var rotationOffset = new RigRotationOffset();

									var dstParentRot = math.mul(dstRootRotInv,
										dstRig.Bones[dstBoneIter].parent.transform.rotation);
									var srcParentRot = math.mul(srcRootRotInv,
										srcRig.Bones[srcBoneIter].parent.transform.rotation);

									var dstRot = math.mul(dstRootRotInv,
										dstRig.Bones[dstBoneIter].transform.rotation);
									var srcRot = math.mul(srcRootRotInv,
										srcRig.Bones[srcBoneIter].transform.rotation);

									rotationOffset.PreRotation = mathex.mul(math.inverse(dstParentRot), srcParentRot);
									rotationOffset.PostRotation = mathex.mul(math.inverse(srcRot), dstRot);

									rotationOffsets.Add(rotationOffset);
									rotationChannels.Add(new ChannelMap
									{
										SourceId = srcPath, DestinationId = dstPath, OffsetIndex = rotationOffsets.Count
									});
								}

								successFlag = true;
							}

			if (!successFlag) status = status + mapIter + " ";
		}

		var rigRemapQuery = new RigRemapQuery();

		rigRemapQuery.TranslationChannels = new ChannelMap[translationChannels.Count];

		for (var iter = 0; iter < translationChannels.Count; iter++)
			rigRemapQuery.TranslationChannels[iter] = translationChannels[iter];

		rigRemapQuery.TranslationOffsets = new RigTranslationOffset[translationOffsets.Count + 1];
		rigRemapQuery.TranslationOffsets[0] = new RigTranslationOffset();

		for (var iter = 0; iter < translationOffsets.Count; iter++)
			rigRemapQuery.TranslationOffsets[iter + 1] = translationOffsets[iter];

		rigRemapQuery.RotationChannels = new ChannelMap[rotationChannels.Count];

		for (var iter = 0; iter < rotationChannels.Count; iter++)
			rigRemapQuery.RotationChannels[iter] = rotationChannels[iter];

		rigRemapQuery.RotationOffsets = new RigRotationOffset[rotationOffsets.Count + 1];
		rigRemapQuery.RotationOffsets[0] = new RigRotationOffset();

		for (var iter = 0; iter < rotationOffsets.Count; iter++)
			rigRemapQuery.RotationOffsets[iter + 1] = rotationOffsets[iter];

		if (!string.IsNullOrEmpty(status)) Debug.LogError("Faulty Entries : " + status);

		return rigRemapQuery;
	}
}