using ingot.Core;

namespace ingot.Tests.Compile;

public class CompilerStateResetsBetweenCompilationsTest
{
    [Fact]
    public void Compile_CompilerStateResetsBetweenCompilations()
    {
        CompilerState.Reset();
        CompilerState.Info("first");
        Assert.Single(CompilerState.GetLogs());

        CompilerState.Reset();
        CompilerState.Info("second");
        Assert.Single(CompilerState.GetLogs());
        Assert.Contains("second", CompilerState.GetLogs()[0]);
    }
}