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
}