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
                        /// <summary>
                        /// The core part of the version. This contains <major>.<minor>.<patch>
                        /// </summary>
                        public const string Core = "{{core}}";
                
                        /// <summary>
                        /// The major part of the version.
                        /// </summary>
                        public const int Major = {{major}};
                
                        /// <summary>
                        /// The minor part of the version.
                        /// </summary>
                        public const int Minor = {{minor}};
                
                        /// <summary>
                        /// The patch part of the version.
                        /// </summary>
                        public const int Patch = {{patch}};
                
                        /// <summary>
                        /// Evaluates as <c>true</c> if the version is a prerelease; otherwise, <c>false</c>.
                        /// </summary>
                        public const bool IsPrerelease = {{isPrerelease}};
                
                        /// <summary>
                        /// The prerelease part of the version. This is the value after the <c>-</c> and before the <c>+</c> character of the version.
                        /// </summary>
                        public static readonly string Prerelease = "{{prerelease}}";

                        /// <summary>
                        /// The prerelease part of the version separated by dot <c>.</c>. This is the value after the <c>-</c> and before the <c>+</c> character of the version.
                        /// </summary>
                        public static readonly string[] PrereleaseIdentifiers = new string[] {{{prereleaseIdentifiers}}};
                
                        /// <summary>
                        /// The metadata part of the version. This is the value after the <c>+</c> character of the version.
                        /// </summary>
                        public static readonly string Metadata = "{{metadata}}";

                        /// <summary>
                        /// The metadata part of the version separated by dot <c>.</c>. This is the value after the <c>+</c> character of the version.
                        /// </summary>
                        public static readonly string[] MetadataIdentifiers = new string[] {{{metadataIdentifiers}}};
                
                        /// <summary>
                        /// The full version of the generated version without the <c>v</c> character.
                        /// </summary>
                        public const string Full = "{{versionTxt}}";
                    }
                }
                """;

        context.AddSource(sourceName, SourceText.From(source, Encoding.UTF8));
    }
}
