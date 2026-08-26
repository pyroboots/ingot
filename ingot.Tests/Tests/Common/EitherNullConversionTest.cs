using ingot.Core.Common;

namespace ingot.Tests.Common;

public class EitherNullConversionTest
{
    [Fact]
    public void ImplicitConversion_NullReferenceAssignsNullEither()
    {
        Either<Dictionary<string, string>, string>? textures = (string?)null;
        Assert.Null(textures);
    }

    [Fact]
    public void Constructor_NullValueThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Either<string, int>((string)null!));
    }
}
