using SimulationFramework;
using System.Numerics;

namespace Raytracer69;

public struct ReflectiveMaterial : IMaterial<ReflectiveMaterial>
{
    Vector3 albedo;

    public ReflectiveMaterial(Vector3 albedo)
    {
        this.albedo = albedo;
    }

    public bool Scatter(Ray ray, Vector3 normal, float t, out Vector3 attenuation, out Vector3 direction)
    {
        attenuation = albedo;
        direction = Vector3.Reflect(ray.direction, normal);
        return true;
    }
}
