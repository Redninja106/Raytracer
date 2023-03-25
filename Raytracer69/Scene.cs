using ILGPU;
using Raytracer69;
using SimulationFramework;
using System.Numerics;

public unsafe struct Scene
{
    public ArrayView<byte> materials;
    public ArrayView<int> materialIndices;
    public ArrayView<byte> models;
    public ArrayView<int> modelIndices;
    public ArrayView<SceneInstance> instances;

    public MaterialKind GetMaterialKind(int index)
    {
        fixed (byte* bytes = &materials[materialIndices[index]]) 
        {
            return *(MaterialKind*)bytes;
        } 
    }
    public ModelKind GetModelKind(int index)
    {
        fixed (byte* bytes = &models[modelIndices[index]])
        {
            return *(ModelKind*)bytes;
        }
    }

    public T GetMaterial<T>(int index) where T : unmanaged, IMaterial<T>
    {
        fixed (byte* bytes = &materials[materialIndices[index]]) 
        {
            return *(T*)(bytes + sizeof(MaterialKind));
        }
    }

    public T GetModel<T>(int index) where T : unmanaged, IModel<T>
    {
        fixed (byte* bytes = &models[modelIndices[index]])
        {
            return *(T*)(bytes + sizeof(ModelKind));
        }
    }

    public Vector3 RayColor(Ray ray, int depth)
    {
        Vector3 color = Vector3.One;
        while (depth-- > 0)
        {
            Vector3 closestNormal = Vector3.Zero;
            float closestT = float.PositiveInfinity;
            int closestMaterial = 0;
            for (int i = 0; i < instances.Length; i++)
            {
                Ray transformedRay = ray.Transform(instances[i].rayTransform);

                if (HitModel(instances[i].model, transformedRay, out var normal, out var t))
                {
                    if (t < closestT)
                    {
                        closestT = t;
                        closestNormal = normal;
                        closestMaterial = instances[i].material;
                        ray.tMax = t;
                    }
                }
            }

            if (closestT != float.PositiveInfinity)
            {
                if (ScatterMaterial(closestMaterial, ray, closestNormal, closestT, out var attenution, out var direction))
                {
                    color *= attenution;
                    ray = new(ray.At(closestT), direction);
                }
                else
                {
                    return color * attenution;
                } 
            }
            else
            {
                return color * SkyBox(ray);
            }
        }

        return color;
    }

    Vector3 SkyBox(Ray ray)
    {
        return Vector3.Lerp(new(.6f, .6f, .8f), new(.3f, .3f, 1), ray.direction.Normalized().Y * .5f + .5f);
    }

    private bool HitModel(int modelIndex, Ray ray, out Vector3 normal, out float t)
    {
        switch (GetModelKind(modelIndex))
        {
            case ModelKind.Sphere:
                return HitModel<SphereModel>(modelIndex, ray, out normal, out t);
            case ModelKind.Cube:
                return HitModel<CubeModel>(modelIndex, ray, out normal, out t);
            default:
                t = 0;
                normal = Vector3.Zero;
                return false;
        }
    }

    private bool HitModel<T>(int modelIndex, Ray ray, out Vector3 normal, out float t) where T : unmanaged, IModel<T>
    {
        var model = GetModel<T>(modelIndex);
        return model.Intersect(ray, out normal, out t);
    }

    private bool ScatterMaterial(int materialIndex, Ray ray, Vector3 normal, float t, out Vector3 attenuation, out Vector3 direction)
    {
        switch (GetMaterialKind(materialIndex))
        {
            case MaterialKind.Reflective:
                return ScatterMaterial<ReflectiveMaterial>(materialIndex, ray, normal, t, out attenuation, out direction);
            case MaterialKind.Diffuse:
                return ScatterMaterial<DiffuseMaterial>(materialIndex, ray, normal, t, out attenuation, out direction);
            case MaterialKind.Glass:
                return ScatterMaterial<GlassMaterial>(materialIndex, ray, normal, t, out attenuation, out direction);
            case MaterialKind.Normal:
                return ScatterMaterial<NormalMaterial>(materialIndex, ray, normal, t, out attenuation, out direction);
            case MaterialKind.Metal:
                return ScatterMaterial<MetalMaterial>(materialIndex, ray, normal, t, out attenuation, out direction);
            default:
                attenuation = direction = Vector3.Zero;
                return false;
        }
    }

    private bool ScatterMaterial<T>(int materialIndex, Ray ray, Vector3 normal, float t, out Vector3 attenuation, out Vector3 direction) where T : unmanaged, IMaterial<T>
    {
        var model = GetMaterial<T>(materialIndex);
        return model.Scatter(ray, normal, t, out attenuation, out direction);
    }
}
