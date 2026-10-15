using System.Reflection;
using System.Xml.Linq;
using Chinook.ModMono.Modules.Catalog;
using Chinook.ModMono.Modules.Playlists;
using Chinook.ModMono.Modules.Sales;

namespace Chinook.ModMono.Tests;

/// <summary>
/// The module boundary rule made executable: no module project references another module project.
/// Modules talk through the host or through contracts, never by reaching into each other.
/// </summary>
public class ModuleBoundaryTests
{
    private const string ModulePrefix = "Chinook.ModMono.Modules.";

    private static readonly Assembly[] Modules =
    [
        typeof(CatalogModule).Assembly,
        typeof(SalesModule).Assembly,
        typeof(PlaylistsModule).Assembly,
    ];

    [Fact]
    public void No_module_assembly_references_another_module()
    {
        foreach (var module in Modules)
        {
            var offenders = module.GetReferencedAssemblies()
                .Where(r => r.Name is not null && r.Name.StartsWith(ModulePrefix, StringComparison.Ordinal))
                .Select(r => r.Name)
                .ToList();

            Assert.True(offenders.Count == 0,
                $"{module.GetName().Name} references {string.Join(", ", offenders)}");
        }
    }

    [Fact]
    public void No_module_project_file_references_another_module()
    {
        // The compiler drops references to assemblies whose types are never used, so the
        // assembly check alone can miss a ProjectReference that is declared but idle.
        var srcDirectory = Path.Combine(FindExampleRoot(), "src");
        var moduleProjects = Directory.GetFiles(srcDirectory, ModulePrefix + "*.csproj", SearchOption.AllDirectories);

        Assert.Equal(3, moduleProjects.Length);

        foreach (var project in moduleProjects)
        {
            var offenders = XDocument.Load(project)
                .Descendants("ProjectReference")
                .Select(e => (string?)e.Attribute("Include") ?? string.Empty)
                .Where(include => Path.GetFileName(include.Replace('\\', '/')).StartsWith(ModulePrefix, StringComparison.Ordinal))
                .ToList();

            Assert.True(offenders.Count == 0,
                $"{Path.GetFileName(project)} references {string.Join(", ", offenders)}");
        }
    }

    private static string FindExampleRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("*.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException("Could not find the example's .sln above " + AppContext.BaseDirectory);
    }
}
