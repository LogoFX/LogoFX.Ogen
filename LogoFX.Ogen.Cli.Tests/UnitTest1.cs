using System;
using Xunit;
//using Xunit.Abstractions;

namespace LogoFX.Ogen.Cli.Tests
{
    public class UnitTest1
    {
      [Fact]
      public void Test1()
      {
        var a = "A string";
        Assert.True(a == "A string");
      }
    }
}
