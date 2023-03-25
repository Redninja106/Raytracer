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
    // albedo
    public Vector3 Color;
    // expose rng so types like MetalMaterial can use it
    public XorShift32 Random;

    public DiffuseMaterial(Vector3 color, XorShift32 random)
    {
        this.Color = color;
        this.Random = random;
    }

    public bool Scatter(Ray ray, Vector3 normal, float t, out Vector3 attenuation, out Vector3 direction)
    {
        unsafe 
        {
            var seed = Random.State ^ *(uint*)&normal.X ^ *(uint*)&normal.Y ^ *(uint*)&normal.Z + *(uint*)&t;

            if (seed is 0)
            {
                seed += 233;
            }

            Random = new(seed);
        }
        direction = normal + RandomUnitVector();
        attenuation = Color;
        return true;
    }

    Vector3 RandomUnitVector()
    {
        Vector3 result;

        do
        {
            result = new(Random.NextFloat(), Random.NextFloat(), Random.NextFloat());
        }
        while (result.LengthSquared() > 1);

        return result;
    }
}
