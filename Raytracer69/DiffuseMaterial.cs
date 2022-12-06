using ILGPU.Algorithms.Random;
using SimulationFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer69;

internal struct DiffuseMaterial : IMaterial<DiffuseMaterial>
{
    Vector3 color;
    XorShift32 random;

    public DiffuseMaterial(Vector3 color, XorShift32 random)
    {
        this.color = color;
        this.random = random;
    }

    public bool Scatter(Ray ray, Vector3 normal, float t, out Vector3 attenuation, out Vector3 direction)
    {
        unsafe 
        {
            var seed = random.State ^ *(uint*)&normal.X ^ *(uint*)&normal.Y ^ *(uint*)&normal.Z + *(uint*)&t;

            if (seed is 0)
            {
                seed += 233;
            }

            random = new(seed);
        }
        direction = normal + RandomUnitVector();
        attenuation = color;
        return true;
    }

    Vector3 RandomUnitVector()
    {
        Vector3 result;

        do
        {
            result = new(random.NextFloat(), random.NextFloat(), random.NextFloat());
        }
        while (result.LengthSquared() > 1);

        return result;
    }
}
