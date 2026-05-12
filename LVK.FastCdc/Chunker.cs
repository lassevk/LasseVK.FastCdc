using System.Buffers;

namespace LVK.FastCdc;

public static class Chunker
{
    public static void Chunk(ReadOnlySpan<byte> data, Action<Chunk> processChunk, ChunkingOptions? options = null)
    {
        options ??= new();
        options.Validate();

        int offset = 0;
        ulong hash = GearHash.EmptyHash;
        for (int index = 0; index < data.Length; index++)
        {
            hash = GearHash.Append(hash, data[index]);
            if ((index - offset >= options.MinimumChunkSize && (hash & options.HashMask) == 0) || index - offset >= options.MaximumChunkSize)
            {
                processChunk(new Chunk(offset, index - offset));
                offset = index;
                hash = GearHash.EmptyHash;
            }
        }

        if (offset < data.Length)
        {
            processChunk(new Chunk(offset, data.Length - offset));
        }
    }

    public static IEnumerable<Chunk> Chunk(byte[] data, ChunkingOptions? options = null)
    {
        options ??= new();
        options.Validate();

        int offset = 0;
        ulong hash = GearHash.EmptyHash;
        for (int index = 0; index < data.Length; index++)
        {
            hash = GearHash.Append(hash, data[index]);
            if ((index - offset >= options.MinimumChunkSize && (hash & options.HashMask) == 0) || index - offset >= options.MaximumChunkSize)
            {
                yield return new Chunk(offset, index - offset);
                offset = index;
                hash = GearHash.EmptyHash;
            }
        }

        if (offset < data.Length)
        {
            yield return new Chunk(offset, data.Length - offset);
        }
    }

    public static IEnumerable<Chunk> Chunk(Stream stream, ChunkingOptions? options = null)
    {
        options ??= new();
        options.Validate();

        int offset = 0;
        stream.Position = 0;
        ulong hash = GearHash.EmptyHash;

        byte[] buffer = ArrayPool<byte>.Shared.Rent(32768);
        try
        {
            int inBuffer;
            int bufferOffset = 0;
            while ((inBuffer = stream.Read(buffer)) > 0)
            {
                for (int index = 0; index < inBuffer; index++)
                {
                    hash = GearHash.Append(hash, buffer[index]);

                    int streamIndex = bufferOffset + index;

                    if ((streamIndex - offset >= options.MinimumChunkSize && (hash & options.HashMask) == 0) || streamIndex - offset >= options.MaximumChunkSize)
                    {
                        yield return new Chunk(offset, streamIndex - offset);
                        offset = streamIndex;
                        hash = GearHash.EmptyHash;
                    }
                }

                bufferOffset += inBuffer;

            }

            if (offset < stream.Length)
            {
                yield return new Chunk(offset, (int)(stream.Length - offset));
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}