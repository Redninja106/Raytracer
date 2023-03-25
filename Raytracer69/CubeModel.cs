using ILGPU.Algorithms.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer69;
public struct CubeModel : IModel<CubeModel>
{
    public Vector3 Size;
    float pad;

    public CubeModel(Vector3 size)
    {
        Size = size;
    }

    public bool Intersect(Ray ray, out Vector3 normal, out float t)
    {
        Vector3 m = Vector3.One / ray.direction;
        Vector3 n = m * ray.origin;
        Vector3 k = Vector3.Abs(m) * Size;
        Vector3 t1 = -n - k;
        Vector3 t2 = -n + k;
        
        // tNear
        var tNear = MathF.Max(MathF.Max(t1.X, t1.Y), t1.Z);
        var tFar = MathF.Min(MathF.Min(t2.X, t2.Y), t2.Z);

        if (tNear > tFar || tFar < 0) 
        {
            normal = default;
            t = 0;

            return false;
        }

        if (tNear > 0)
        {
            t = tNear;
            normal = Step(new(tNear), t1);
        }
        else
        {
            t = tFar;
            normal = Step(t2, new(tFar));
        }

        //normal *= -new Vector3(Sign(ray.direction.X), Sign(ray.direction.Y), Sign(ray.direction.Z));

        return true;
    }

    Vector3 Step(Vector3 a, Vector3 b)
    {
        return new(
            Step(a.X, b.X),
            Step(a.Y, b.Y),
            Step(a.Z, b.Z)
            );
    }

    float Step(float a, float b)
    {
        return a < b ? 0f : 1f;
    }

    float Sign(float n)
    {
        return n == 0 ? 0 : n < 0 ? -1 : 1f;
    }
}
