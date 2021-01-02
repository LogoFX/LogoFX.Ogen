using System;
using Microsoft.Extensions.DependencyInjection;
using McMaster.Extensions.CommandLineUtils;
using System.Reflection;

namespace LogoFX.Ogen.Cli
{
  class Program
  {
    public static int Main(string[] args)
    {
      ConfigureServices();

      return CommandLineApplication.Execute<DefaultCommand>(args);
    }

    private static void ConfigureServices()
    {
      var services = new ServiceCollection()
                      //.AddSingleton<IMyService, MyServiceImplementation>()
                      .AddSingleton<IConsole>(PhysicalConsole.Singleton)
                      .BuildServiceProvider();
    }
  }
}
