# DOTSAnimationPratice
Studying how the new DOTS animation system works.
<br/>
Going to learn intermediate to advance feature such as blending, remapping, blendtree (1D,2D), etc..
<br/>
The current version I'm using is com.unity.animation  0.9.0-preview.6. 
with URP + Hybrid as the rendering.
<br/>
<br/>
001 Single clip animation executing on GPU Hybrid Ecs and Pure Ecs. 
<br/>
[![me](https://github.com/KDahir247/DOTSAnimationPratice/blob/main/Assets/Gif/001.gif)
<br/>
002 Single clip animation of rotating cube on the y axis with dynamic animation speed manipulation using an animationCurve. Executing on the GPU Pure Ecs with comment on what is happening.
<br/>
[![me](https://github.com/KDahir247/DOTSAnimationPratice/blob/main/Assets/Gif/002.gif)
<br/>
003 Two clip animation that use a mixerNode to blend animations from on clip to another. This study uses hard blend (0, 1) without any interpolation for the MixerNode KernelPort Weight. Hard blend can easily be changed to a interpolated blend by changing the MixerNode Kernel Port weight to interpolated value from 0 (shooting) to 1 (reloading). Executing on the GPU Pure Ecs.
<br/>
[![me](https://github.com/KDahir247/DOTSAnimationPratice/blob/main/Assets/Gif/003.gif)
<br/>
