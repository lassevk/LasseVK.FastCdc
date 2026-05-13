namespace LasseVK.FastCdc;

#if NET8_0_OR_GREATER
/// <summary>
/// Represents a single chunk in a dataset split by <see cref="Chunker"/>.
/// </summary>
/// <param name="Offset">
/// The offset of the chunk in the dataset.
/// </param>
/// <param name="Length">
/// The length of the chunk in the dataset.
/// </param>
public readonly record struct Chunk(int Offset, int Length);
#else
/// <summary>
/// Represents a single chunk in a dataset split by <see cref="Chunker"/>.
/// </summary>
/// <param name="Offset">
/// The offset of the chunk in the dataset.
/// </param>
/// <param name="Length">
/// The length of the chunk in the dataset.
/// </param>
public readonly struct Chunk(int Offset, int Length);
#endif