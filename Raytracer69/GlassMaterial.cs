using SimulationFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer69;
internal struct GlassMaterial : IMaterial<GlassMaterial>
{
    public float RefractiveIndex;

    public GlassMaterial(float refractiveIndex)
    {
        RefractiveIndex = refractiveIndex;
    }

    public bool Scatter(Ray ray, Vector3 normal, float t, out Vector3 attenuation, out Vector3 direction)
    {
        attenuation = Vector3.One * .99f;

        direction = Refract(ray.direction.Normalized(), normal, this.RefractiveIndex);

        return true;
    }

    private static Vector3 Refract(Vector3 direction, Vector3 normal, float refraction)
    {
        float cos = MathF.Min(Vector3.Dot(-direction, normal), 1.0f);
        Vector3 perpendicular = refraction * (direction + cos * normal);
        Vector3 parallel = -MathF.Sqrt(float.Abs(1.0f - perpendicular.LengthSquared())) * normal;
        return parallel + perpendicular;
    }
}