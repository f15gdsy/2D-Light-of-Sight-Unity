# 2D Light of Sight (v0.9 alpha)
<br>

###

## Demo
1. [Radial Shadow] (https://dl.dropboxusercontent.com/u/27907965/games/LOS/RadialLightDemo/RadialLightDemo.html)
2. [Fullscreen Shadow] (https://dl.dropboxusercontent.com/u/27907965/games/LOS/FullScreenLightDemo/FullScreenLightDemo.html)

<br>

## What is it?
A 2D dynamic lighting plugin for Unity3d, that is, it achieves something like [this] (https://dl.dropboxusercontent.com/u/27907965/images/LOS/los_radial_light_0.png) and [this] (https://dl.dropboxusercontent.com/u/27907965/images/LOS/los_full_screen_light_0.png).

#### Features
1. Can cope with any shape, as long as the corresponding collider is attached with the object.
2. Has [radial shadow mode] (https://dl.dropboxusercontent.com/u/27907965/images/los_2.png), and [fullscreen shadow mode] (https://dl.dropboxusercontent.com/u/27907965/images/los_1.png).
3. Event system. Objects can be notified if it gets lit or gets unlit.
4. Can work with perspective camera too. (Tested with topdown view, not tested with 45 degree isometric view)
5. All features are supported by Unity #FREE# version!!!

#### How to use:
1. Set camera. Place LOSCamera script on the camera that is used to render the shadow. (Or ignore this step if you only have one camera, the editor will do the work for you.)
2. Create a light. In the GameObject dropdown, select LOS 2D Lighting / Radial Light (or other lights) to create a 2D light gameobject. For the 2D light to work properly, the obstacle layer property should be set in the inspector. 
3. Create an obstacle. Create any premitive or sprites, and attach Unity collider to them. Then attach LOSObstacle script to the gameobject. Also don't forget to set its layer to match what's set in step 2.
4. Adjust degree step to match the precision you want.

#### Tips & Tricks:
1. Sometimes for moving lights, the degree step (which stands for precision) should be set lower, something like 0.1f or 0.2f would be good enough. However, the smaller the degree step (better precision), usually means more costly. I'm planning to work on better scene management to improve this.
2. The Full Screen Light requires its position is within the camera so that to work properly.
3. Currently it only supports 3D colliders, however, I'm planing to make it work on 2D colliders too. It should not be too difficult.
4. If you want to use perspective camera to render effects like [Teleglitch] (https://www.youtube.com/watch?v=7tcxT_WxItE), and orthographic camera to render line of sight normally, you can use orthographic camera as LOSCamera, but render Teleglitch shadow using perspective camera (by playing with layer mask).


<br>

## TODO in the next version
1. Add APIs for toggling the lighting & event system on off.
2. Add suport for 2D colliders.
3. Add callback function for LOSEventSource.

<br>

## Remarks
Feel free to use it! It's under MIT liscence!
Just a reference back to here will be very appreciated :D
Also any comments, questions, and suggestions are welcome!
Unity forum: http://forum.unity3d.com/threads/light-of-sight-2d-dynamic-lighting-open-source.295968/
