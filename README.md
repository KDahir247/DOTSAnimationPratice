# DOTSAnimationPratice
Studying how the new DOTS animation system works.
<br/>
Going to learn intermediate to advance feature such as blending, remapping, blendtree (1D,2D), etc..
<br/>
The current version I'm using is com.unity.animation  0.9.0-preview.6. 
with URP + Hybrid as the rendering.
<br/>
Creating and Destroying animation graph seems like they must happen on the main thread.
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
004 Two clip animation that uses a mixerNode to blend animations from on clip to another. Blending is handled by a seperate animation kernel node and is fed to the MixerNode blend value in the MixerNode KernelPort. The UI update the component value from there the BlendNode retrieve the component and feed it to the MixerNode. This method seem better (performance) due do kernel node utilizing both Jobs and the burst compiler, while changing the MixerNode data directly can't use either the burst compiler and jobs due to structual changes and needing NodeSet (Reference).
<br/>
[![me](https://github.com/KDahir247/DOTSAnimationPratice/blob/main/Assets/Gif/004.gif)
<br/>
005 1D BlendTree for blending three animation together walk, run, sprint.
<br/>
BlendTree are better then working with raw animation clip since it let you create complex behaviour such as blendtree that blend blendtree, also it support 5 blend type (1D, 2D simple directional, 2D freeform directional, 2D freeform cartesian, and direct ). BlendNode handle setting up mixer node or anyother node to work and connecting them and destroying them.
<br/>
[![me](https://github.com/KDahir247/DOTSAnimationPratice/blob/main/Assets/Gif/005.gif)
<br/>
006 2D Simple Direction BlendTree for blending a 8 way plus an idle animation
<br/>
[![me](https://github.com/KDahir247/DOTSAnimationPratice/blob/main/Assets/Gif/006.gif)
<br/>

