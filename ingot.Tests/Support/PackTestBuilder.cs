using ingot.Core;
using ingot.Tests.Content;

namespace ingot.Tests.Support;

internal static class PackTestBuilder
{
    public static Pack Create(string description = "test") =>
        Pack.Create(TestUuids.Behaviour, "test pack", description, TestUuids.Resource);
}