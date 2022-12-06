using ILGPU;
using ILGPU.Runtime;
using System.Runtime.InteropServices;

class IndexedBufferWriter
{
    List<byte> bytes = new();
    List<int> indices = new();
    MemoryBuffer1D<byte, Stride1D.Dense>? bytesBuffer;
    MemoryBuffer1D<int, Stride1D.Dense>? indicesBuffer;

    public void Clear()
    {
        bytes.Clear();
        indices.Clear();
        bytesBuffer = null;
        indicesBuffer = null;
    }

    public void AddElement<TEnum, TElement>(TEnum kind, TElement element) 
        where TEnum : unmanaged, Enum
        where TElement : unmanaged
    {
        indices.Add(bytes.Count);
        AddBytes(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref kind, 1)));
        AddBytes(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref element, 1)));
    }

    private void AddBytes(Span<byte> bytes)
    {
        byte[] byteArray;
        if (bytes.Length % 4 == 0)
        {
            byteArray = new byte[bytes.Length];
        }
        else
        {
            byteArray = new byte[bytes.Length + (4 - bytes.Length % 4)];
        }
        bytes.CopyTo(byteArray);
        this.bytes.AddRange(byteArray);
    }

    public void GetViews(Accelerator accelerator, out ArrayView<byte> bytesView, out ArrayView<int> indicesView)
    {
        if (bytesBuffer is null || bytesBuffer.Length != this.bytes.Count)
        {
            bytesBuffer?.Dispose();
            bytesBuffer = accelerator.Allocate1D<byte>(bytes.ToArray());
        }
        if (indicesBuffer is null || indicesBuffer.Length != this.indices.Count)
        {
            indicesBuffer?.Dispose();
            indicesBuffer = accelerator.Allocate1D<int>(indices.ToArray());
        }

        bytesView = bytesBuffer.AsArrayView<byte>(0, bytesBuffer.Length);
        indicesView = indicesBuffer.AsArrayView<int>(0, indicesBuffer.Length);
    }
}