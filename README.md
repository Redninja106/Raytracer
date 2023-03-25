# Raytracer

A C# raytracer gpu accelerated using ilgpu and simulationframework.

- it's not very fast but it at least runs in real time (500x500 @ 50 samples/pixel).
- supports dynamic scenes with materials and models (ie scene is not hardcoded on gpu). this is done via:
  - a list of models (spheres, boxes, etc)
  - a list of materials (diffuse, metallic, etc)
  - a list of instances with a transform matrix and an index for each of the two previous lists
  - each list can have elements of different sizes, so a buffer is maintained for each one that stores the start of each element in bytes into the main buffer
  - this is probably why its so slow
- averages image over multiple frames while camera is not moving to reduce noise

- models
  - box
  - sphere
- materials
  - reflective
  - diffuse
  - glass (broken)
  - normal (visualizes normals)
  - metal

![image](https://user-images.githubusercontent.com/45476006/227733580-de074987-82cd-4081-98ed-3263d99ce10b.png)

![image](https://user-images.githubusercontent.com/45476006/227733556-9d2568d1-fca8-4512-ad6b-59526a621afb.png)
