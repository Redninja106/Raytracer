using System.Numerics;

public struct SceneInstance
{
    public Matrix4x4 rayTransform;
    public int model;
    public int material;

    public SceneInstance(int model, int material, Matrix4x4 transform)
    {
        this.model = model; 
        this.material = material;

        if (!Matrix4x4.Invert(transform, out this.rayTransform)) 
            throw new Exception();
    }
}
