using System.Reflection;
using System.Xml.Linq;
using Chinook.Clean.Domain;

namespace Chinook.Clean.Tests;

/// <summary>
/// The dependency rule made executable: Domain depends on nothing else in this solution.
/// </summary>
public class DependencyRuleTests
{
    private static readonly Assembly DomainAssembly = typeof(Artist).Assembly;

    [Fact]
    public void Domain_assembly_references_no_other_Chinook_assembly()
    {
        var chinookReferences = DomainAssembly
            .GetReferencedAssemblies()
            .Where(a => a.Name is not null && a.Name.StartsWith("Chinook.", StringComparison.Ordinal))
            .Select(a => a.Name)
            .ToList();

        Assert.Empty(chinookReferences);
    }

    [Fact]
    public void Domain_project_file_has_no_project_or_package_references()
    {
        // The compiler drops references that no code uses, so the assembly check alone would pass
        // even if someone added a ProjectReference that is not used yet. The project file catches that.
        var csproj = FindDomainProjectFile();
        var document = XDocument.Load(csproj);

        var references = document.Descendants()
            .Where(e => e.Name.LocalName is "ProjectReference" or "PackageReference" or "FrameworkReference")
            .Select(e => e.Attribute("Include")?.Value)
            .ToList();

        Assert.Empty(references);
    }

    private static string FindDomainProjectFile()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "src", "Chinook.Clean.Domain", "Chinook.Clean.Domain.csproj");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find src/Chinook.Clean.Domain/Chinook.Clean.Domain.csproj above the test output folder.");
    }
}
