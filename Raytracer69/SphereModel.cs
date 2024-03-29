﻿using System.Numerics;

public struct SphereModel : IModel<SphereModel>
{
    public float radius;

    public SphereModel(float radius)
    {
        this.radius = radius;
    }

    public bool Intersect(Ray ray, out Vector3 normal, out float t)
    {
        var co = ray.origin;
        var a = Vector3.Dot(ray.direction, ray.direction);
        var halfB = Vector3.Dot(co, ray.direction);
        var c = Vector3.Dot(co, co) - radius * radius;

        var discriminant = halfB * halfB - a * c;

        if (discriminant < 0)
        { 
            t = 0;
            normal = Vector3.Zero;
            return false;
        }

        var sqrtD = MathF.Sqrt(discriminant);

        t = (-halfB - sqrtD) / a;

        if (discriminant is not 0 && t < ray.tMin)
        {
            t = (-halfB + sqrtD) / a;
        }
        normal = Vector3.Normalize(ray.At(t));
        return t < ray.tMax && t > ray.tMin;
    }
}