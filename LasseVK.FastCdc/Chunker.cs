using System.Buffers;

namespace LasseVK.FastCdc;

/// <summary>
/// Implements FastCdc chunking algorithm.
/// </summary>
public static class Chunker
{
    /// <summary>
    /// Calculates the chunks from a ReadOnlySpan of bytes. Due to ReadOnlySpan preventing an enumerator from being generated,
    /// this overload instead uses a callback to process each chunk.
    /// </summary>
    /// <param name="data">
    /// The dataset to chunk.
    /// </param>
    /// <param name="processChunk">
    /// The callback that will be invoked for each chunk.
    /// </param>
    /// <param name="options">
    /// Optional <see cref="ChunkingOptions"/> to use.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="options"/> is invalid. See <see cref="ChunkingOptions.Validate"/> for details.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="processChunk"/> is null.
    /// </exception>
    public static void Chunk(ReadOnlySpan<byte> data, Action<Chunk> processChunk, ChunkingOptions? options = null)
    {
        if (processChunk is null)
        {
            throw new ArgumentNullException(nameof(processChunk));
        }

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

    /// <summary>
    /// Calculates the chunks from a byte array
    /// </summary>
    /// <param name="data">
    /// The dataset to chunk.
    /// </param>
    /// <param name="options">
    /// Optional <see cref="ChunkingOptions"/> to use.
    /// </param>
    /// <returns>
    /// An enumerable of <see cref="LasseVK.FastCdc.Chunk"/>s.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="options"/> is invalid. See <see cref="ChunkingOptions.Validate"/> for details.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="data"/> is null.
    /// </exception>
    public static IEnumerable<Chunk> Chunk(byte[] data, ChunkingOptions? options = null)
    {
        if (data is null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        options ??= new();
        options.Validate();

        return enumerable();

        IEnumerable<Chunk> enumerable()
        {
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
    }

    /// <summary>
    /// Calculates the chunks from a stream
    /// </summary>
    /// <param name="stream">
    /// The dataset to chunk.
    /// </param>
    /// <param name="options">
    /// Optional <see cref="ChunkingOptions"/> to use.
    /// </param>
    /// <returns>
    /// An enumerable of <see cref="LasseVK.FastCdc.Chunk"/>s.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="options"/> is invalid. See <see cref="ChunkingOptions.Validate"/> for details.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is null.
    /// </exception>
    public static IEnumerable<Chunk> Chunk(Stream stream, ChunkingOptions? options = null)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        options ??= new();
        options.Validate();

        return enumerable();

        IEnumerable<Chunk> enumerable()
        {
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
                            long currentPosition = stream.Position;
                            yield return new Chunk(offset, streamIndex - offset);

                            if (stream.Position != currentPosition)
                            {
                                stream.Position = currentPosition;
                            }

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
}