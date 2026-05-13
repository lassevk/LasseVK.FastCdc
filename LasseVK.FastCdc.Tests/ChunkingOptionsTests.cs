namespace LasseVK.FastCdc.Tests;

public class ChunkingOptionsTests
{
    [Test]
    public void Validate_NonSequentialHashMask_ThrowsArgumentException()
    {
        var options = new ChunkingOptions { HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000101 };

        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Test]
    public void Validate_ZeroHashMask_ThrowsArgumentException()
    {
        var options = new ChunkingOptions { HashMask = 0 };
        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Test]
    public void Validate_MinimumChunkSizeLessThanOne_ThrowsArgumentException()
    {
        var options = new ChunkingOptions { MinimumChunkSize = 0 };
        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Test]
    public void Validate_MaximumChunkSizeLessThanTwiceMinimum_ThrowsArgumentException()
    {
        var options = new ChunkingOptions { MinimumChunkSize = 1024, MaximumChunkSize = 2047 };
        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Test]
    public void Validate_MaximumChunkSizeGreaterThan128Kb_ThrowsArgumentException()
    {
        var options = new ChunkingOptions { MinimumChunkSize = 1024, MaximumChunkSize = 128 * 1024 * 1024 + 1 };
        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Test]
    public void Validate_MaximumChunkSizeEqualTo128Kb_DoesNotThrow()
    {
        var options = new ChunkingOptions { MinimumChunkSize = 1024, MaximumChunkSize = 128 * 1024 * 1024 };
        Assert.DoesNotThrow(options.Validate);
    }

    [Test]
    public void Validate_MinimumChunkSizeNegative_ThrowsArgumentException()
    {
        var options = new ChunkingOptions { MinimumChunkSize = -1 };
        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Test]
    public void Validate_MaximumChunkSizeLessThanMinimum_ThrowsArgumentException()
    {
        var options = new ChunkingOptions { MinimumChunkSize = 100, MaximumChunkSize = 50 };
        Assert.Throws<ArgumentException>(options.Validate);
    }

    [Test]
    public void Validate_ValidSequentialHashMask_DoesNotThrow()
    {
        var options = new ChunkingOptions { HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01111111 };
        Assert.DoesNotThrow(options.Validate);
    }

    [Test]
    public void Validate_ValidDefaultOptions_DoesNotThrow()
    {
        var options = new ChunkingOptions();
        Assert.DoesNotThrow(options.Validate);
    }

    [Test]
    public void Validate_ValidCustomOptions_DoesNotThrow()
    {
        var options = new ChunkingOptions
        {
            MinimumChunkSize = 1024,
            MaximumChunkSize = 8192,
            HashMask = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_01111111,
        };
        Assert.DoesNotThrow(options.Validate);
    }

    [Test]
    public void Validate_MaximumChunkSizeEqualToMinimum_DoesNotThrow()
    {
        var options = new ChunkingOptions { MinimumChunkSize = 1024, MaximumChunkSize = 2048 };
        Assert.DoesNotThrow(options.Validate);
    }

    [Test]
    public void Chunk_StabilityTest()
    {
        // This will used as a canary to detect changes in the algorithm that impacts chunk cutting and cut point detection.

        byte[] bytes = new byte[128 * 1024];
        new Random(12345).NextBytes(bytes);

        var chunks = Chunker.Chunk(bytes, new ChunkingOptions()).ToList();

        int[] expectedChunkLengths =
        [
            1365, 3584, 1438, 1673, 1232, 1061, 1072, 2167, 3218, 1616, 5591, 7052, 1877, 4205, 1281, 1258, 3266, 3652, 1108, 1027, 6661, 1216, 1041, 1055, 1663, 10439, 1680, 1435, 8019, 1550
          , 2676, 6711, 11088, 2096, 5245, 4539, 5187, 2295, 1351, 6068, 314,
        ];

        int offset = 0;
        var expectedChunks = new List<Chunk>();
        foreach (int length in expectedChunkLengths)
        {
            expectedChunks.Add(new Chunk(offset, length));
            offset += length;
        }

        Assert.That(chunks, Is.EqualTo(expectedChunks).AsCollection);
    }
}