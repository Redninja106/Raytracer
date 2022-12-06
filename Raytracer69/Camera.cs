using System.Numerics;

public struct Camera
{
    public Matrix4x4 transform;

    public Camera(Matrix4x4 transform)
    {
        this.transform = transform;
    }

    public Ray GetRay(Vector2 uv)
    {
        return new(Vector3.Transform(Vector3.Zero, transform), Vector3.TransformNormal(new(uv, 1), transform));
    }
}
