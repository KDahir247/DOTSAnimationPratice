using Unity.Animation.Hybrid;
using Unity.Entities;
using UnityEngine;

[ConverterVersion("Blend1DShootReload", 1)]
public class PlayerShootReloadAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    private AnimationClip ShootingClip;
    [SerializeField]
    private AnimationClip ReloadingClip;

    private AnimationClip[] animationClips = new AnimationClip[2];

    //Play the shooting around 10 times then reload
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        if (ShootingClip == null || ReloadingClip == null)
            return;

        animationClips[0] = ShootingClip;
        animationClips[1] = ReloadingClip;
        DynamicBuffer<ShootReload_PlayClipBuffer> clipBuffer = dstManager.AddBuffer<ShootReload_PlayClipBuffer>(entity);
        for (byte i = 0; i < animationClips.Length; i++)
            clipBuffer.Add(new ShootReload_PlayClipBuffer(){Clip =  animationClips[i].ToDenseClip()});

        dstManager.AddComponent<DeltaTimeRuntime>(entity);

        dstManager.AddComponent<PlayerShootReloadRuntime>(entity);

        dstManager.AddComponent<PlayerShootReloadTimerRuntime>(entity);
    }
}
