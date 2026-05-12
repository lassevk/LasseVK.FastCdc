namespace LVK.FastCdc;

public record ChunkingOptions
{
    public ulong HashMask { get; init; } = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000111_11111111;
    public int MinimumChunkSize { get; init; } = 1024;
    public int MaximumChunkSize { get; init; } = 128 * 1024;

    public void Validate()
    {
        if ((HashMask & (HashMask + 1)) != 0 || HashMask == 0)
        {
            throw new ArgumentException("HashMask must be a non-zero sequence of 1-bits, ie. 2^N-1");
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