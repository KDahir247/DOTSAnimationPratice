using JetBrains.Annotations;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.UIElements;

public class AnimationThirdSampleDocument : VisualElement
{
	private Slider _blendSlider;

	public AnimationThirdSampleDocument()
	{
		RegisterCallback<GeometryChangedEvent>(UIQuery);
	}

	private void UIQuery(GeometryChangedEvent evt)
	{
		_blendSlider = this.Q<Slider>("Blend_Slider");

		_blendSlider.RegisterCallback<ChangeEvent<float>>(BlendSliderChange);

		UnregisterCallback<GeometryChangedEvent>(UIQuery);
	}

	private void BlendSliderChange([NotNull] ChangeEvent<float> changeEvent)
	{
		//Send A Message
		var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		var satisfiedEntities =
			entityManager.CreateEntityQuery(typeof(PlayerBlendClipBuffer)).ToEntityArray(Allocator.Temp);
		//Update it
		foreach (var entity in satisfiedEntities)
		{
			if (!entityManager.HasComponent<PlayerBlendKernelRuntime>(entity)) continue;

			var data = entityManager.GetComponentData<PlayerBlendKernelRuntime>(entity);
			data.BlendAmount = changeEvent.newValue;
			entityManager.SetComponentData(entity, data);
		}
	}

	public new class UxmlFactory : UxmlFactory<AnimationThirdSampleDocument, UxmlTraits>
	{
	}

	public new sealed class UxmlTraits : VisualElement.UxmlTraits
	{
	}
}