namespace LVK.FastCdc.Tests;

public class ChunkerTests
{
    private static IEnumerable<(byte[] Data, string Name)> ByteArraysTestCases()
    {
        yield return (new byte[32768], "32k all zeroes");

        byte[] bytes1 = new byte[32768];
        Array.Fill<byte>(bytes1, 1);
        yield return (bytes1, "32k all ones");

        byte[] bytes2 = new byte[32768];
        for (int index = 0; index < bytes2.Length; index++)
        {
            bytes2[index] = (byte)index;
        }
        yield return (bytes2, "32k all bytes");

        yield return (PseudoRandomBytes(12345), "32k pseudo-random 12345");
        yield return (PseudoRandomBytes(54321), "32k pseudo-random 54321");
        yield return (PseudoRandomBytes(11111), "32k pseudo-random 11111");
        yield return (PseudoRandomBytes(983759873), "32k pseudo-random 983759873");
        yield return (PseudoRandomBytes(1234567890, 37173), "37k pseudo-random 1234567890");
    }

    private static IEnumerable<(ChunkingOptions Options, string Name)> OptionsTestCases()
    {
        yield return (new ChunkingOptions(), "default");
        yield return (new ChunkingOptions { HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111111 }, "8-bit mask");
        yield return (new ChunkingOptions { HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00111111 }, "6-bit mask");
    }

    public static IEnumerable<TestCaseData> TestCases()
    {
        var byteArrays = ByteArraysTestCases().ToList();
        var options = OptionsTestCases().ToList();

        foreach ((byte[] Data, string Name) byteArray in byteArrays)
        {
            foreach ((ChunkingOptions Options, string Name) option in options)
            {
                yield return new TestCaseData(byteArray.Data, option.Options).SetName($"{byteArray.Name} {option.Name}");
            }
        }
    }

    private static byte[] PseudoRandomBytes(int seed, int length = 32768)
    {
        var rng = new Random(seed);
        byte[] bytes = new byte[length];
        rng.NextBytes(bytes);
        return bytes;
    }

    [TestCaseSource(nameof(TestCases))]
    public void Chunk_AllThreeOverloads_ProduceSameChunks(byte[] data, ChunkingOptions options)
    {
        var chunks1 = Chunker.Chunk(data, options).ToList();
        Console.WriteLine("produced {0} chunks", chunks1.Count);

        var chunks2 = new List<Chunk>();
        Chunker.Chunk(data, chunks2.Add, options);

        var stream = new MemoryStream(data);
        var chunks3 = Chunker.Chunk(stream, options).ToList();

        Assert.That(chunks1, Is.EqualTo(chunks2).AsCollection);
        Assert.That(chunks2, Is.EqualTo(chunks3).AsCollection);
        Assert.That(chunks1, Is.EqualTo(chunks3).AsCollection);
    }

    [TestCaseSource(nameof(TestCases))]
    public void Chunk_Span_ReturnsChunksThatSpanWholeContent(byte[] data, ChunkingOptions options)
    {
        var chunks = new List<Chunk>();
        Chunker.Chunk(data, chunks.Add, options);

        int sum = chunks.Sum(c => c.Length);
        Assert.That(sum, Is.EqualTo(data.Length));

        for (int index = 1; index < chunks.Count; index++)
        {
            Assert.That(chunks[index].Offset, Is.EqualTo(chunks[index - 1].Offset + chunks[index - 1].Length));
        }
    }

    [TestCaseSource(nameof(TestCases))]
    public void Chunk_ByteArray_ReturnsChunksThatSpanWholeContent(byte[] data, ChunkingOptions options)
    {
        var chunks = Chunker.Chunk(data, options).ToList();

        int sum = chunks.Sum(c => c.Length);
        Assert.That(sum, Is.EqualTo(data.Length));

        for (int index = 1; index < chunks.Count; index++)
        {
            Assert.That(chunks[index].Offset, Is.EqualTo(chunks[index - 1].Offset + chunks[index - 1].Length));
        }
    }

    [TestCaseSource(nameof(TestCases))]
    public void Chunk_Stream_ReturnsChunksThatSpanWholeContent(byte[] data, ChunkingOptions options)
    {
        var stream = new MemoryStream(data);
        var chunks = Chunker.Chunk(stream, options).ToList();

        int sum = chunks.Sum(c => c.Length);

        Assert.That(sum, Is.EqualTo(data.Length));

        for (int index = 1; index < chunks.Count; index++)
        {
            Assert.That(chunks[index].Offset, Is.EqualTo(chunks[index - 1].Offset + chunks[index - 1].Length));
        }
    }

    [TestCaseSource(nameof(TestCases))]
    public void Chunk_OneByteOffset_OnlyImpactsFirstChunk(byte[] data, ChunkingOptions options)
    {
        var chunks1 = Chunker.Chunk(data, options).ToList();
        var chunks2 = Chunker.Chunk([123, ..data], options).ToList();

        Assert.That(chunks1.Skip(1).Select(c => c.Length), Is.EqualTo(chunks2.Skip(1).Select(c => c.Length)).AsCollection);
    }
}