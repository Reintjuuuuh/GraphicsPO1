Team members: (names and student IDs)
* Boaz Roskam 7262329
* Rein Spaan 1651137
* Jouke Jonker 2884518

Tick the boxes below for the implemented features. Add a brief note only if necessary, e.g., if it's only partially working, or how to turn it on.

Formalities:
[X] This readme.txt
[X] Cleaned (no obj/bin folders)

Minimum requirements implemented:
[X] Camera: position and orientation controls, field of view in degrees
Controls: wasd, shift and space for movement and mouse for orientation
[X] Primitives: plane, sphere
[X] Lights: at least 2 point lights, additive contribution, shadows without "acne"
The little yellow orbs have a light source underneeth so it is easy to locate the lightsource
[X] Diffuse shading: (N.L), distance attenuation
[X] Phong shading: (R.V) or (N.H), exponent
[X] Diffuse color texture: only required on the plane primitive, image or procedural, (u,v) texture coordinates
[X] Mirror reflection: recursive
[X] Debug visualization: sphere primitives, rays (primary, shadow, reflected, refracted)

Bonus features implemented:
[X] Triangle primitives: must use the algorithm from the lectures, single triangles or meshes
[ ] Interpolated normals: only required on triangle primitives, 3 different vertex normals must be specified
[X] Spot lights: smooth falloff optional
[ ] Glossy reflections: not only of light sources but of other objects
[ ] Anti-aliasing
[ ] Parallelized
Method: ... (for example: parallel-for, async tasks, or threads)
[ ] Textures: on all implemented primitives
[ ] Bump or normal mapping: on all implemented primitives
[ ] Environment mapping: sphere or cube map, without intersecting actual sphere/cube/triangle primitives
[ ] Refraction: also requires a reflected ray at every refractive surface, recursive
[ ] Area lights: soft shadows
[ ] Acceleration structure: bounding box or hierarchy, scene with 5000+ primitives
Performance comparison: ... (provide one measurement of speed/time with and without the acceleration structure)
[ ] GPU implementation
Method: ... (for example: fragment shader, compute shader, ILGPU, or CUDA)
[X] Dinamicly add lighting: Press i to add a light source

Used Sources:
Slides of Graphics Lectures

