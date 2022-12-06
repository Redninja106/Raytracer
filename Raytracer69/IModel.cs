using System.Numerics;

public interface IModel<T> where T : unmanaged, IModel<T> 
{
    public unsafe int Size => sizeof(T);
    bool Intersect(Ray ray, out Vector3 normal, out float t);
}
