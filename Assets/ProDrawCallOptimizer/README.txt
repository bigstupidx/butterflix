ProDrawcall optimizer is an editor extension that will help you reduce
the number of draw calls on your project with just a single clicks.

This package lets you remap automatically each of your UV maps on your
meshes to a single atlas, hence having one material with an atlas for all
the meshes that share the same shader.

Have specific / custom shaders?, dont worry, the tool is flexible enough to accept any kind of shader, just add them in the custom shaders tab.

NOTES:
- Easy, learn after watching one of the videos.
- No scripting required.
- Meshes automatically set up and adjusted.
- Full lightmapping support.
- Your source assets will not be touched.
- Full multiple material per game object support
- Examples included for you to see and play.
- Supports tiled materials
- Generate prefabs from the baked objects


/******************************* TUTORIAL ************************************/
For more detailed info on how the package works, check this video on how the tool works:
https://www.youtube.com/watch?v=U2rbuHplBMI

How custom shaders work with the tool:
https://www.youtube.com/watch?v=SqCf2AaW98Y&feature=youtu.be

How to correctly set your objects to get the most of it:
https://www.youtube.com/watch?v=NBaXorFya8E&feature=youtu.be

If you are having problems with textures looking weird, you can also check
this video on what causes this issue:
https://www.youtube.com/watch?v=SK9NLz6k2D0&feature=youtu.be

In order to load the examples, please double click and import the
"ProDrawcallExamples.unitypackage" which can be found in the project

If you are using LOD groups, Mike from M2H Studios shared a short code that
helps out automating the process, check it out at: (https://gist.github.com/naruse/4edc15f7c389ea8f793b)

/**** Contact me ****/
Any comments / suggestions / bugs?, drop me a line at:
support@pencilsquaregames.com
/********************/

/**** Known Issues *****/
- When combining objects with multiple materials on texture formats different from ARGB32, RGB24 and Alpha8, it will drop an error.
Solution to this:
Change the build settings to stand alone and Bake the atlas OR change the texture format of the textures to combine to either
ARGB32, RGB24 or Alpha8 on the multiple materials objects
/********************/

Tips to optimize drawcalls:
- Try to use similar texture sizes for each atlas.
- If your shader uses more than one texture try to make textures have the
same size, else there will be resizing to the main texture.
- Adjust the generated atlas size to the closest generated size in order to
not lose quality on your meshes.
- Avoid Shadows realtime shadows when possible.


Check my other projects!
- Pro Pivot Modifier: Modify your meshes pivot quickly and easy.
-- https://www.assetstore.unity3d.com/en/#!/content/8913

- Pro Mouse: Set the cursor position wherever you want
-- https://www.assetstore.unity3d.com/en/#!/content/8910
