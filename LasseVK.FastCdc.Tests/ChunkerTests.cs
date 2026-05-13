using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace LasseVK.FastCdc.Tests;

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
        yield return (new ChunkingOptions { HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00111111 }, "6-bit hash mask");
        yield return (new ChunkingOptions { HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111111 }, "8-bit hash mask");
        yield return (new ChunkingOptions { HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_00111111_11111111 }, "14-bit hash mask");
        yield return (new ChunkingOptions { HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_01111111_11111111 }, "15-bit hash mask");
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

    private static IEnumerable<TestCaseData> StabilityTestCases()
    {
        yield return new TestCaseData(PseudoRandomBytes(12345, 128 * 1024), (int[])[
            1365, 3584, 1438, 1673, 1232, 1061, 1072, 2167, 3218, 1616, 5591, 7052, 1877, 4205, 1281, 1258, 3266, 3652, 1108, 1027, 6661, 1216, 1041, 1055, 1663, 10439, 1680, 1435, 8019, 1550
          , 2676, 6711, 11088, 2096, 5245, 4539, 5187, 2295, 1351, 6068, 314,
        ], new ChunkingOptions()).SetName("128k pseudo-random 12345");

        yield return new TestCaseData(PseudoRandomBytes(12345), (int[])[
            1365, 3584, 1438, 1673, 1232, 1061, 1072, 2167, 3218, 1616, 5591, 7052, 1699,
        ], new ChunkingOptions()).SetName("32k pseudo-random 12345");

        yield return new TestCaseData(PseudoRandomBytes(1234567890, 64 * 1024), (int[])[
            1362, 4278, 2290, 2154, 2797, 3427, 2492, 2800, 2798, 2678, 1608, 5015, 2723, 8095, 1401, 3963, 1105, 1332, 7152, 5630, 436,
        ], new ChunkingOptions()).SetName("64k pseudo-random 1234567890");

        yield return new TestCaseData(PseudoRandomBytes(1234567890, 64 * 1024)
          , (int[])
            [
                1362, 1383, 1696, 1039, 1121, 1111, 1404, 1253, 1390, 1036, 1166, 1334, 1041, 1131, 1093, 1198, 1680, 1209, 1665, 1094, 1670, 1150, 1175, 1588, 1268, 1097, 2893, 1343, 1212, 1165, 1408
              , 1492, 1406, 1500, 1267, 1695, 1031, 1025, 1127, 1106, 1294, 1327, 2245, 1492, 1059, 1643, 1154, 1315, 983,
            ], new ChunkingOptions
            {
                HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111111,
            }).SetName("64k pseudo-random 1234567890 with non-default options");
    }

    [TestCaseSource(nameof(StabilityTestCases))]
    public void Chunk_StabilityTest(byte[] bytes, int[] expectedChunkLengths, ChunkingOptions options)
    {
        // This will used as a canary to detect changes in the algorithm that impacts chunk cutting and cut point detection.
        var chunks = Chunker.Chunk(bytes, options).ToList();

        int offset = 0;
        var expectedChunks = new List<Chunk>();
        foreach (int length in expectedChunkLengths)
        {
            expectedChunks.Add(new Chunk(offset, length));
            offset += length;
        }

        Assert.That(chunks, Is.EqualTo(expectedChunks).AsCollection);
    }

    [Test]
    public void Chunk_NullAction_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Chunker.Chunk(new byte[1024], (Action<Chunk>)null!));
    }

    [Test]
    public void Chunk_NullStream_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Chunker.Chunk((Stream)null!));
    }

    [Test]
    public void Chunk_NullByteArray_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Chunker.Chunk((byte[])null!));
    }
}