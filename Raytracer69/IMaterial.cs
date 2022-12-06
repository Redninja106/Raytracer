using System.Numerics;

namespace Raytracer69;

public interface IMaterial<T> where T : unmanaged, IMaterial<T>
{
    public unsafe int Size => sizeof(T);
    bool Scatter(Ray ray, Vector3 normal, float t, out Vector3 attenuation, out Vector3 direction);
}
