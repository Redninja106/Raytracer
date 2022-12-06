using ILGPU;
using ILGPU.Algorithms.Random;
using SimulationFramework;
using System.Numerics;

public struct Kernel
{
    public Scene scene;
    public Camera camera;
    public int width;
    public int height;
    public XorShift32 random;
    public int samples;
    public int maxDepth;
    public int frames;

    public Kernel() { }

    public static unsafe void Main(Index2D index, ArrayView<Color> renderTarget, Kernel kernel)
    {
        kernel.random = new((uint)(kernel.frames + index.X ^ (index.Y << 8) ^ (kernel.width << 16) ^ (kernel.height << 24)));
        Vector3 newColor = kernel.DispatchRay(index.X, index.Y);
        
        newColor.X = MathF.Sqrt(newColor.X);
        newColor.Y = MathF.Sqrt(newColor.Y);
        newColor.Z = MathF.Sqrt(newColor.Z);

        var rtIndex = index.Y * kernel.width + index.X;
        var currentValue = renderTarget[rtIndex];

        Vector3 currentColor = new(currentValue.R / 255f, currentValue.G / 255f, currentValue.B / 255f);

        var weight = 1f / kernel.frames;
        var color = currentColor * (1f - weight) + weight * newColor;
        
        uint colorValue = 
            (uint)(color.X * 255) << 24 |
            (uint)(color.Y * 255) << 16 |
            (uint)(color.Z * 255) << 8 |
            255;

        renderTarget[rtIndex] = *(Color*)&colorValue;
    }

    public Vector3 DispatchRay(int x, int y)
    {
        Vector3 color = Vector3.Zero;

        for (int i = 0; i < samples; i++)
        {

            float sampleX = x + random.NextFloat();
            float sampleY = y + random.NextFloat();

            Vector2 uv = new(sampleX / width, sampleY / height);

            uv -= Vector2.One * .5f;
            uv.X *= width / (float)height;

            Ray ray = camera.GetRay(uv);
            color += scene.RayColor(ray, maxDepth);
        }

        return color / samples;
    }

    //private Vector4 TraceRay(Ray ray, int depth)
    //{
    //    Vector4 color = Vector4.One;

    //    while (depth > 0)
    //    {
    //        depth--;
    //        color *= RayColor(ray);
    //        ray.origin = ;
    //        ray.direction = Vector3.Reflect(ray.direction, normal);
    //    }

    //    return color;
    //}

    //private Vector4 RayColor(Ray ray)
    //{
    //    if (sphere.Intersect(ray, out float t))
    //    {
    //    }
    //    else
    //    {
    //    }
    //}
}
