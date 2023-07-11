using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace VersionSourceGenerator;

/// <inheritdoc/>
[Generator(LanguageNames.CSharp)]
public class VersionGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var versionFiles = context.AdditionalTextsProvider
            .Where(static a => a.Path.ToLower().EndsWith("version.txt"))
            .Select(static (a, c) => (FileName: Path.GetFileName(a.Path), Content: a.GetText(c)?.ToString()!));

        context.RegisterSourceOutput(
            versionFiles,
            static (context, versionFile) => Generate(context, versionFile.Content, versionFile.FileName));
    }

    private static void Generate(SourceProductionContext context, string versionTxt, string versionTxtFilename)
    {
        Regex regex = new("^(0|[1-9]\\d*)\\.(0|[1-9]\\d*)\\.(0|[1-9]\\d*)(?:-((?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\\+([0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?$");
        if (!regex.IsMatch(versionTxt))
        {
            throw new Exception($"Invalid version from {versionTxtFilename}: {versionTxt}");
        }

        string versionTxtFilenameLower = versionTxtFilename.ToLower();

        int sourceNameSep = versionTxtFilenameLower.IndexOf(".txt");
        int classNameSep = versionTxtFilenameLower.Contains(".version.txt") ?
            versionTxtFilenameLower.IndexOf(".version.txt") :
            versionTxtFilenameLower.IndexOf("version.txt");

        string sourceName = $"{versionTxtFilename.Substring(0, sourceNameSep)}.g.cs";
        string className = $"{versionTxtFilename.Substring(0, classNameSep)}Version";

        versionTxt = versionTxt.Trim();
        if (versionTxt.ToLower().FirstOrDefault() == 'v')
        {
            versionTxt = versionTxt.Substring(1);
        }

        string core;
        string major;
        string minor;
        string patch;
        string prerelease = "";
        string metadata = "";
        string isPrerelease = "false";
        string prereleaseIdentifiers = "";
        string metadataIdentifiers = "";

        int dashPos = versionTxt.IndexOf('-');
        int plusPos = versionTxt.IndexOf('+');

        core = versionTxt.Substring(0, 
            dashPos > 0 && plusPos > 0 ?
            Math.Min(dashPos, plusPos) :
            (dashPos > 0 ?
                dashPos :
                (plusPos > 0 ?
                    plusPos :
                    versionTxt.Length)));

        var coreSplit = core.Split('.');
        major = coreSplit[0];
        minor = coreSplit[1];
        patch = coreSplit[2];

        if (dashPos > 0)
        {
            if (plusPos > 0)
            {
                prerelease = versionTxt.Substring(dashPos + 1, plusPos - dashPos - 1);
            } 
            else
            {
                prerelease = versionTxt.Substring(dashPos + 1);
            }
            prereleaseIdentifiers = string.Join(", ", prerelease.Split('.').Select(s => $"\"{s}\""));
            isPrerelease = "true";
        }

        if (plusPos > 0)
        {
            metadata = versionTxt.Substring(plusPos + 1);
            metadataIdentifiers = string.Join(", ", metadata.Split('.').Select(s => $"\"{s}\""));
        }

        string source = $$"""
                // Auto-generated code
                using System;
                using System.Collections;
                using System.Collections.Generic;
                namespace VersionSourceGenerator
                {
                    /// <summary>
                    /// The generated version from the text file {{versionTxtFilename}}
                    /// </summary>
                    public static class {{className}}
                    {
                        public const string Core = "{{core}}";
                        public const int Major = {{major}};
                        public const int Minor = {{minor}};
                        public const int Patch = {{patch}};
                        public const bool IsPrerelease = {{isPrerelease}};
                        public static readonly string Prerelease = "{{prerelease}}";
                        public static readonly string[] PrereleaseIdentifiers = new string[] {{{prereleaseIdentifiers}}};
                        public static readonly string Metadata = "{{metadata}}";
                        public static readonly string[] MetadataIdentifiers = new string[] {{{metadataIdentifiers}}};
                
                        public const string Full = "{{versionTxt}}";
                    }
                }
                """;

        context.AddSource(sourceName, SourceText.From(source, Encoding.UTF8));
    }
}
