using Unity.Animation.Hybrid;
using Unity.Entities;
using UnityEngine;

//make sure any cached data was prepared using the active code. if not then the scene will be reconverted
[ConverterVersion("PrimitiveRotation",1)]
public class PrimitiveRotationAnimationAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    private AnimationClip clip;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (clip == null)
            return;

        //conversion result on this gameObject depends on source asset. Any changes on clip will
        //trigger a reconversion on this gameObject to an entity.
        conversionSystem.DeclareAssetDependency(gameObject, clip);

        //Convert unityengine animation clip to a DOTS dense clip (class to a blob ref struct container)
        var denseClip = clip.ToDenseClip();
        //Try to see if a the blob is already in the BlobAssetStore if so it will fetch rather then create. if not it will create and then cache
        conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref denseClip);

        //Add RotateCube_PlayClip Component to the entity with the dense clip as BlobAssetReference<Clip> param
        dstManager.AddComponentData(entity,new RotateCube_PlayClipRuntime()
        {
            clip = denseClip
        });

        //Add DeltaTime Component to the entity with default value (float 0.0f)
        dstManager.AddComponent<DeltaTimeRuntime>(entity);
    }
}
