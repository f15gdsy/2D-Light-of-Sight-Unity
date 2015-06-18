# 2D Light of Sight (v0.9 alpha)
<br>

###

## Demo
1. [Radial Shadow] (https://dl.dropboxusercontent.com/u/27907965/games/LOS/RadialLightDemo/RadialLightDemo.html)
2. [Fullscreen Shadow] (https://dl.dropboxusercontent.com/u/27907965/games/LOS/FullScreenLightDemo/FullScreenLightDemo.html)
3. [Ludum Dare #32 entry: "Long Live The Great Leader!"] (http://ludumdare.com/compo/ludum-dare-32/?action=preview&uid=24851)

<br>

## What is it?
A 2D dynamic lighting plugin for Unity3d, that is, it achieves something like [this] (https://dl.dropboxusercontent.com/u/27907965/images/LOS/los_radial_light_0.png) and [this] (https://dl.dropboxusercontent.com/u/27907965/images/LOS/los_full_screen_light_0.png).

#### Features
1. Can cope with any shape, as long as the corresponding collider is attached with the object.
2. Has [radial mode] (https://dl.dropboxusercontent.com/u/27907965/images/los_2.png), and [fullscreen mode] (https://dl.dropboxusercontent.com/u/27907965/images/los_1.png).
3. Event system. Objects can be notified if it gets lit or gets unlit.
4. Can work with perspective camera too. (Tested with topdown view, not tested with 45 degree isometric view)
5. Works with 2D & 3D Physics!
6. All features are supported by Unity #FREE# version!!! This may not be as important as now Unity5 unlocks all engine feature.

#### How to use:
1. Set camera. Place LOSCamera script on the camera that is used to render the shadow. (Or ignore this step if you only have one camera, the editor will do the work for you.)
2. Create a light. In the GameObject dropdown, select LOS 2D Lighting / Radial Light (or other lights) to create a 2D light gameobject. For the 2D light to work properly, the obstacle layer property should be set in the inspector. 
3. Create an obstacle. Create any premitive or sprites, and attach Unity collider to them. Then attach LOSObstacle script to the gameobject. Also don't forget to set its layer to match what's set in step 2.
4. Adjust degree step to match the precision you want.

#### Tips & Tricks:
1. **Toggle 2D / 3D Physics**: The latest version of this plugin supports 2D & 3D physics in Unity. All you need is to set the Physics Opt field in LOSManager's inspector.
2. **Precision VS Performance**: Sometimes for moving lights, the degree step (which stands for precision) should be set lower, something like 0.1f or 0.2f would be good enough. However, the smaller the degree step (better precision), usually means more costly. I'm planning to work on better scene management to improve this.
3. **Full Screen Light Tips**: The Full Screen Light requires its position is within the camera so that to work properly.
4. **Fake 3D Shadow**: If you want to use perspective camera to render effects like [my LD#32 game] (http://ludumdare.com/compo/ludum-dare-32/?action=preview&uid=24851), you can use orthographic camera to render other things normally, and render shadow using perspective camera (by playing with layer mask). This effect is inspired by a game called *Teleglitch*.
5. **Blur Lights & Shdows**: With Unity5 and above, you can blur the edge of the lights & shadows! Check out [the blured version of full screen light] (https://dl.dropboxusercontent.com/u/27907965/images/LOS/Screen%20Shot%202015-04-23%20at%205.38.45%20pm.png). Here is how it works: render the light in a separate camera which has blur image effect component attached. One thing to note is that, this camera should be rendered at last (by setting the camera's Depth low). So the background (not blured) actually rendered before the light, but I just made the background semi-transparent to make the light visible. You can check out the Full Screen Light Blur example in the plugin.



<br>

## TODO
1. Currently in Unity 5, full screen light has issue with capsule collider, but other parts works fine.
2. Directional light.

<br>

## Remarks
Feel free to use it! It's under MIT liscence!
Just a reference back to here will be very appreciated :D
Also any comments, questions, and suggestions are welcome!
Unity forum: http://forum.unity3d.com/threads/light-of-sight-2d-dynamic-lighting-open-source.295968/
