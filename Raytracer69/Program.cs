using ILGPU;
using ILGPU.Algorithms.Random;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ImGuiNET;
using Raytracer69;
using Silk.NET.Maths;
using SimulationFramework;
using SimulationFramework.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

ITexture? texture = null;
float scale = 1.0f;
int resolutionX = 500, resolutionY = 500;
bool locked = true;
Scene scene = new();
Camera camera = new(Matrix4x4.Identity);
float camRotX = 0, camRotY = 0;
Vector3 camPos = Vector3.Zero;
int samples = 50;
int maxDepth = 50;
int frames = 1;
Vector2? lastMousePos = null;
IndexedBufferWriter models = new();
IndexedBufferWriter materials = new();
List<SceneInstance> instances = new();
MemoryBuffer1D<SceneInstance, Stride1D.Dense>? instanceBuffer = null;

Context? context = null;
CudaDevice? device = null;
CudaAccelerator? accelerator = null;
MemoryBuffer2D<Color, Stride2D.DenseX>? buffer = null;
Action<AcceleratorStream, Index2D, ArrayView<Color>, Kernel>? compiledKernel = null;

instances.Add(new(0, 0, Matrix4x4.CreateTranslation(new(0, 0, 2))));
instances.Add(new(1, 3, Matrix4x4.CreateTranslation(new(1f, .5f, .5f))));
instances.Add(new(2, 1, Matrix4x4.CreateTranslation(new(-2f, .5f, 2))));
instances.Add(new(3, 2, Matrix4x4.CreateTranslation(new(0, 2001, 0))));
instances.Add(new(0, 6, Matrix4x4.CreateTranslation(new(0, 0, -2))));
//instances.Add(new(4, 4));
instances.Add(new(5, 5, Matrix4x4.CreateTranslation(0,1,0)));
models.AddElement(ModelKind.Sphere, new SphereModel(1));
models.AddElement(ModelKind.Sphere, new SphereModel(.5f));
models.AddElement(ModelKind.Sphere, new SphereModel(.5f));
models.AddElement(ModelKind.Sphere, new SphereModel(2000));
models.AddElement(ModelKind.Sphere, new SphereModel(.5f));
models.AddElement(ModelKind.Cube, new CubeModel(new(.5f, .5f, .5f)));
materials.AddElement(MaterialKind.Reflective, new ReflectiveMaterial(Vector3.One * .8f));
materials.AddElement(MaterialKind.Diffuse, new DiffuseMaterial(Vector3.UnitX, XorShift32.Create(Random.Shared)));
materials.AddElement(MaterialKind.Diffuse, new DiffuseMaterial(Vector3.One * .5f, XorShift32.Create(Random.Shared)));
materials.AddElement(MaterialKind.Diffuse, new DiffuseMaterial(Vector3.UnitZ, XorShift32.Create(Random.Shared)));
materials.AddElement(MaterialKind.Glass, new GlassMaterial(1.5f));
materials.AddElement(MaterialKind.Glass, new NormalMaterial());
materials.AddElement(MaterialKind.Glass, new MetalMaterial(new(0, 0, 1), .5f));

Simulation.Create(Init, Render).Run();

void Init(AppConfig config)
{
    context = Context.CreateDefault();
    device = context.GetCudaDevice(0);
    accelerator = device.CreateCudaAccelerator(context);
    compiledKernel = accelerator.LoadAutoGroupedKernel<Index2D, ArrayView<Color>, Kernel>(Kernel.Main);
}

void Render(ICanvas canvas)
{
    canvas.Clear(Color.Black);
    HandleInput();
    Layout();

    if (texture is null || texture.Width != resolutionX || texture.Height != resolutionY)
    {
        resolutionX = Math.Max(resolutionX, 16);
        resolutionY = Math.Max(resolutionY, 16);

        texture?.Dispose();
        texture = Graphics.CreateTexture(resolutionX, resolutionY);
        buffer = accelerator!.Allocate2DDenseX<Color>(new(resolutionX, resolutionY));
    }

    camera.transform = Matrix4x4.CreateRotationX(camRotX) * Matrix4x4.CreateRotationY(camRotY) * Matrix4x4.CreateTranslation(-camPos);

    if (instanceBuffer is null || instanceBuffer.Length != instances.Count)
    {
        instanceBuffer?.Dispose();
        instanceBuffer = accelerator.Allocate1D(instances.ToArray());
    }

    scene = new();
    scene.instances = instanceBuffer.AsArrayView<SceneInstance>(0, instanceBuffer.Length);
    models.GetViews(accelerator, out scene.models, out scene.modelIndices);
    materials.GetViews(accelerator, out scene.materials, out scene.materialIndices);

    Kernel kernel = new()
    {
        scene = scene,
        camera = camera,
        width = resolutionX,
        height = resolutionY,
        random = new(1),
        maxDepth = maxDepth,
        samples = samples,
        frames = frames,
    };

    compiledKernel?.Invoke(accelerator!.DefaultStream, new(resolutionX, resolutionY), buffer!.ToArrayView().AsContiguous(), kernel);

    buffer!.ToArrayView().AsContiguous().CopyToCPU(texture.Pixels);
    texture.ApplyChanges();
    canvas.Translate(canvas.Width / 2f, canvas.Height / 2f);
    canvas.DrawTexture(texture, 0, 0, resolutionX * scale, resolutionY * scale, Alignment.Center);

    frames++;
}

void HandleInput()
{
    bool moved = false;

    if (Mouse.IsButtonDown(MouseButton.Right))
    {
        Vector2 mouseDelta = Mouse.Position - (lastMousePos ?? Mouse.Position);
        mouseDelta *= MathF.PI * 0.001f;
        camRotX -= mouseDelta.Y;
        camRotY += mouseDelta.X;
        lastMousePos = Mouse.Position;

        if (mouseDelta != Vector2.Zero)
        {
            moved = true;
        }
    }
    else
    {
        lastMousePos = null;
    }

    Vector3 delta = Vector3.Zero;
    if (Keyboard.IsKeyDown(Key.W)) delta -= Vector3.UnitZ; 
    if (Keyboard.IsKeyDown(Key.A)) delta += Vector3.UnitX; 
    if (Keyboard.IsKeyDown(Key.S)) delta += Vector3.UnitZ; 
    if (Keyboard.IsKeyDown(Key.D)) delta -= Vector3.UnitX; 
    if (Keyboard.IsKeyDown(Key.C)) delta -= Vector3.UnitY; 
    if (Keyboard.IsKeyDown(Key.Space)) delta += Vector3.UnitY;

    if (delta != Vector3.Zero)
        moved = true;

    camPos += Vector3.Transform(delta * Time.DeltaTime, Matrix4x4.CreateRotationX(camRotX) * Matrix4x4.CreateRotationY(camRotY));

    if (moved)
        frames = 1;
}

void Layout()
{
    ImGui.SliderFloat("Scale", ref scale, .5f, 10.0f);

    if (
        ImGui.DragInt("Resolution X", ref resolutionX) ||
        ImGui.DragInt("Resolution Y", ref resolutionY)
        )
    {
        frames = 1;
    }

    ImGui.Checkbox("Lock Size & Resolution", ref locked);

    if (locked)
    {
    }
    
    ImGui.Separator();

    ImGui.SliderInt("Samples", ref samples, 1, 100);
    ImGui.SliderInt("Max Depth", ref maxDepth, 1, 100);
    ImGui.Text("Image averaged over " + frames + " frames.");

    ImGui.Separator();
}