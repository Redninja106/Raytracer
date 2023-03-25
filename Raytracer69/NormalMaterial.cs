using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer69;

// renders normals for debugging
public struct NormalMaterial : IMaterial<NormalMaterial>
{
    // must be 4-byte aligned
    int _pad;

    public bool Scatter(Ray ray, Vector3 normal, float t, out Vector3 attenuation, out Vector3 direction)
    {
        attenuation = normal;
        direction = default;
        return false;
    }
}
