# Documentation for MeshSparklyEffect

## Usage

0. Make sure the Universal Render Pipeline Asset is set correctly in the Scriptable Render Pipeline Settings in the Graphics section of Project Settings.
1. Add MeshSparklyEffect.cs to GameObject
1. With the error "The SkinnedMeshRenderer is missing.", a Switch MeshFilter button and a property to specify the Skinned Mesh Renderer will appear.
1. By specifying Skinned Mesh Renderer here, parameters to adjust the effect will appear (MeshFilter can also be specified by pressing the Switch MeshFilter Mode button).
1. When the parameter to adjust the effect appears in the inspector of MeshSparklyEffect, you can change the appearance of the effect by adjusting each property.

## Description of inspector properties

![Inspector window](./Images/inspector.png)

### Switch MeshFilter/SkinnedMeshRenderer Mode button

By pressing this button, you can switch between using Skinned Mesh Renderer and MeshFilter.

If the Mesh to be switched to is not registered, an error will be displayed at the top of the inspector, and it will return to the initial state where each parameter is hidden.

### Target SkinnedMeshRenderer/MeshFilter

Specify the mesh to be targeted. 

The position of the particle is determined based on the vertex information of the mesh specified here.

However, this specification alone does not track the Mesh Transform, so it is necessary to add a Constraint to the GameObject with the Visual Effect that will be added to the lower layer (for example, in the sample scene called MeshFilter, a Parent Constraint is used.)

### Color Texture

A texture used to add color to particles.

The UVs of the specified mesh are used to reflect the color on the texture to the particles, so if the texture used on the specified mesh is used, the color corresponding to the vertex position where the particles are emitted will be reflected.

### Rate

The frequency at which particles are emitted.

The higher this value, the more particles are emitted in a short period of time.

### Alpha

Alpha value of the particle.

### Size Decay Curve

An animation curve that specifies how the size of the particles will change depending on the Life Time.

The actual size of the particle is determined by multiplying a random value within the range specified by Size Min-Max by the Size Decay Curve.

### Size Min-Max

It specifies a range of particle sizes that are determined randomly, and can be fixed to a unique size by setting Min and Max to the same value.

The MinMaxSlider can also be used to specify the minimum and maximum values that can be moved by the slider by changing the Low Limit and High Limit.

### Life Time Min-Max

Specifies a range of particle display times that are determined randomly.

Like Size Min-Max, it can be fixed by setting it to the same value.

### Emission Intensity

Emission intensity of particles.

### Rotate Degree

Specifies the rotation angle of the particle in degrees.

### Offset

Specifies how much the particle's appearance position is shifted from the mesh vertex position toward the normal direction.

Adjust this value so that the particles do not overlap with the mesh.

### Switch Texture/Procedural Mode ボタン

This button can be used to switch between using cross-shaped procedural particles or textures as particles.

When you press this button to switch the mode, the parameters that match the current mode will be displayed.

### Spike Width (Procedural Mode)

Specifies the thickness of the spikes of the cross-shaped particles.

### Sparkle Texture (Texture Mode)

Specifies the texture to be used as a particle.

### Convert to Map/Mesh ボタン

The texture generated from the mesh vertex information is displayed in the inspector, and can be returned to the original Mesh display mode by pressing the button again when it says Convert to Mesh.

![Convert to Map](./Images/vertex_map.png)

When the Bake button is pressed, a window will appear to specify the directory where each texture is to be saved, and the file will be saved in the specified location as an EXR file with the combination of the mesh name and the respective map name as the file name.