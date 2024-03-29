﻿using System.Numerics;

public struct Ray
{
    public Vector3 origin;
    public Vector3 direction;
    public float tMin;
    public float tMax;

    public Ray(Vector3 origin, Vector3 direction)
    {
        this.origin = origin;
        this.direction = direction;
        this.tMin = 0.01f;
        this.tMax = float.PositiveInfinity;
    }

    public Vector3 At(float t)
    {
        return origin + t * direction;
    }

    public Ray Transform(Matrix4x4 rayTransform)
    {
        Ray result = this;
        result.origin = Vector3.Transform(origin, rayTransform);
        result.direction = Vector3.TransformNormal(direction, rayTransform);
        return result;
    }
}
