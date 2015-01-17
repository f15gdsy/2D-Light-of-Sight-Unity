# 2D-Line-of-Sight-Unity
<br>

## What is it?
A 2D line of sight library for Unity3d, that is, it achieves something like [this] (https://dl.dropboxusercontent.com/u/27907965/images/los_0.png).

#### Features
1. Can cope with any shape, as long as the corresponding collider is attached with the object.
2. Has [normal shadow mode] (https://dl.dropboxusercontent.com/u/27907965/images/los_2.png), and [invert shadow mode] (https://dl.dropboxusercontent.com/u/27907965/images/los_1.png).
Usually invert mode is what we want, but with normal shadow mode you may be able to draw something on the shadow with RenderTexture (requires a pro liscence which I dont have).

<br><br>

## How to use?

#### 1. Create something like the invert shadow mode.
1. Attach LOSLight.cs to the object which will be the light source
2. Tick the invert box, and select obstacle layer.
3. Attach LOSObstacle to the object which will be an obstacle.
4. Set the obstacle object's layer to the corresponding layer set in step 2.

And it should work!

