
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Models;
using DotLiquid;

namespace LogoFX.Ogen.Cli
{
  [Command(
      ExtendedHelpText = @"
  Remarks:
    This command should only be used on Tuesdays.
  ")]
  public sealed class DefaultCommand
  {
    private IConsole _console;

    public DefaultCommand(IConsole console)
    {
      _console = console;
    }

    [Argument(0, "api", "The API file name.")]
    [FileExists]
    public string Api { get; }

    [Option("-o|--output <FOLDER>", "The output folder", CommandOptionType.SingleValue)]
    public string OutputFolder { get; }

    [Option(Description = "The custom templates folder")]
    [DirectoryExists]
    public string Templates { get; }

    private void OnExecute(CommandLineApplication app)
    {
      if (string.IsNullOrWhiteSpace(Api))
      {
        WriteLineColored("\nAt least the <api> argument is required.\n", ConsoleColor.Red);
        app.ShowHelp();
        return;
      }
      var stopwatch = Stopwatch.StartNew();

      using (var sr = new StreamReader(Api))
      {
        var line = sr.ReadToEnd();

        var document = new OpenApiStringReader().Read(line, out var diagnostics);

        foreach (var item in document.Components.Schemas)
        {
          _console.WriteLine($"Schema - {item.Key} of type {item.Value.Type}");

          switch (item.Value.Type)
          {
            case OpenApiTypes.Object:
              if (item.Value.Properties.Any())
              {
                foreach (var property in item.Value.Properties)
                {
                  if (property.Value.Reference != null)
                  {
                    _console.WriteLine($"Property {property.Key} of type {property.Value.Reference.Id}. Description: {property.Value.Description}");
                  }
                  else
                  {
                    _console.WriteLine($"Property {property.Key} of type {property.Value.Type}. Description: {property.Value.Description}");
                  }
                }
              }

              break;
            case OpenApiTypes.Array:
              _console.WriteLine($"of items of type of {item.Value.Items.Reference.Id} ({item.Value.Items.Type})");
              break;
            default:

              break;
          }

        }

        RegisterViewModel(typeof(OpenApiDocument));
        //RegisterViewModel(typeof(OpenApiTypes));
        //var hash1 = Hash.FromAnonymousObject(this);
        var template = Template.Parse("Title: {{Info.Title}}. Version: {{Info.Version}}");
        var hash = Hash.FromAnonymousObject(document);
        //hash.Merge(hash1);
        var output = template.Render(hash);
        _console.WriteLine(output);
      }

      stopwatch.Stop();
      _console.WriteLine($"Elapsed {stopwatch.Elapsed}");
    }

    private void WriteLineColored (string format, ConsoleColor color, params object[] arg) {
      var consoleColor = _console.ForegroundColor;
      _console.ForegroundColor = color;
      _console.WriteLine(format, arg);
      _console.ForegroundColor = consoleColor;
    }

    private void RegisterViewModel(Type rootType)
    {
      rootType
          .Assembly
          .GetTypes()
          .Where(t => t.Namespace == rootType.Namespace)
          .ToList()
          .ForEach(RegisterSafeTypeWithAllProperties);
    }

    private void RegisterSafeTypeWithAllProperties(Type type)
    {
      var properties = type
          .GetProperties()
          .Select(p => p.Name)
          .ToArray();

      Template.RegisterSafeType(type, properties);
    }
  }

  public static class OpenApiTypes {

    public const string Array = "array";

    public const string Object = "object";
  }


}
