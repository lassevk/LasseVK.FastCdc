namespace LVK.FastCdc;

/// <summary>
/// Used to provide options to <see cref="Chunker"/>.
/// </summary>
public record ChunkingOptions
{
    /// <summary>
    /// The hash mask to use to determine if a chunk border is detected. This has to consist of an unbroken sequence of 1-bits in the lower bits
    /// position, ie. 2^N-1. The integer value of the hashmask indicates the average size of the chunks.
    /// </summary>
    public ulong HashMask { get; init; } = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000111_11111111;

    /// <summary>
    /// The minimum chunk size to produce. This has to be at least 1024 ie. 1KB.
    /// </summary>
    public int MinimumChunkSize { get; init; } = 1024;

    /// <summary>
    /// The maximum chunk size to produce. This has to be at least twice the value of <see cref="MinimumChunkSize"/>, and at most 128MB.
    /// </summary>
    public int MaximumChunkSize { get; init; } = 128 * 1024;

    /// <summary>
    /// Validates the options.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <see cref="HashMask"/> is invalid, must consist of an unbroken sequence of 1-bits in the lower bits position, ie. 2^N-1.
    /// - or -
    /// <see cref="HashMask"/> is too small, there has to be at least six bit set.
    /// - or -
    /// <see cref="MinimumChunkSize"/> is too small, it must be at least 1024.
    /// - or -
    /// <see cref="MaximumChunkSize"/> is too large, it must be at most 128MB.
    /// - or -
    /// <see cref="MaximumChunkSize"/> must be at least twice the value of <see cref="MinimumChunkSize"/>.
    /// </exception>
    public void Validate()
    {
        if ((HashMask & (HashMask + 1)) != 0)
        {
            throw new ArgumentException("HashMask must be a non-zero sequence of 1-bits, ie. 2^N-1");
        }

        if (HashMask < 0b00111111)
        {
            throw new ArgumentException("HashMask must be at least 6 bits");
        }

        if (MinimumChunkSize < 1024)
        {
            throw new ArgumentException("MinimumChunkSize must be at least 1024");
        }

        if (MaximumChunkSize < MinimumChunkSize * 2)
        {
            throw new ArgumentException("MaximumChunkSize must be at least twice the value of MinimumChunkSize");
        }

        if (MaximumChunkSize > 128 * 1024 * 1024)
        {
            throw new ArgumentException("MaximumChunkSize must be at most 128MB");
        }
    }
}