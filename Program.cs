using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace consoleapp2
{
    static class ProcessEx
    {
        public static string Start(string fileName, string arguments)
        {
            using (
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                }
            )
            {
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ProcessEx.Start("/usr/bin/pacman", "-Qi")
                .Split('\n')
                .Where(x => !String.IsNullOrWhiteSpace(x))
                .Where(x => !x.StartsWith("                  "))
                .Select(x => new { Key = x.Substring(0, 16).Trim(), Value = x.Substring(17).Trim() })
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 21)
                .Select(x => x.Select(y => y.Value).ToArray())
                .Select(
                    x => new
                    {
                        Name = x[0],
                        Version = x[1],
                        Description = x[2],
                        Architecture = x[3],
                        Url = x[4],
                        Licences = x[5],
                        Groups = x[6],
                        Provides = x[7],
                        DependsOn = x[8],
                        OptionalDependencies = x[9],
                        RequiredBy = x[10],
                        OptionalFor = x[11],
                        ConflictsWith = x[12],
                        Replaces = x[13],
                        InstalledSize = x[14],
                        Packager = x[15],
                        BuildDate = x[16],
                        InstallDate = x[17],
                        InstallReason = x[18],
                        InstallScript = x[19],
                        ValidatedBy = x[20],
                    }
                )
                .Select(
                    x => new
                    {
                        Name = x.Name.Value,
                        Size = SizeByUnit(x.InstalledSize.Value),
                    }
                )
                .OrderBy(x => x.Size)
                .ToList()
                .ForEach(
                    x =>
                    {
                        Console.WriteLine($"[{x.Name}][{x.Size}]");
                    }
                );
        }

        static double SizeByUnit(string value)
        {
            var units = new Dictionary<string, int>()
            {
                { "B", 1 },
                { "KiB", 1024 },
                { "MiB", 1024 * 1024 },
                { "GiB", 1024 * 1024 * 1024 },
            };

            var tokens = value.Split(' ');
            return Double.Parse(tokens[0]) * units[tokens[1].Trim()];
        }
    }
}
