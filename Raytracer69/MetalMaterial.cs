using ILGPU.Algorithms.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Raytracer69;

// mixes a diffuse and reflective material
public struct MetalMaterial : IMaterial<MetalMaterial>
{
    public float Metallic;
    public Vector3 Albedo;

    public MetalMaterial(Vector3 albedo, float metallic)
    {
        Albedo = albedo;
        Metallic = metallic;
    }

    public unsafe bool Scatter(Ray ray, Vector3 normal, float t, out Vector3 attenuation, out Vector3 direction)
    {
        DiffuseMaterial diffuse = new(Albedo, new XorShift32(*(uint*)&(t)));
        ReflectiveMaterial reflective = new(Albedo);

        return diffuse.Scatter(ray, normal, t, out attenuation, out direction);
        if (diffuse.Random.NextFloat() > Metallic)
        {
        }
        else
        {
            return reflective.Scatter(ray, normal, t, out attenuation, out direction);
        }
    }
}