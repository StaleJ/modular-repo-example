using NetArchTest.Rules;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ArchTests;

public class ModuleBoundariesTests
{
    private static readonly string[] ModuleLayers = new[] { "Contracts", "Domain", "Infrastructure", "Application", "Api" };

    private static bool IsModuleAssemblyName(string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName)) return false;
        var parts = assemblyName.Split('.');
        if (parts.Length < 2) return false;
        var layer = parts[1];
        return ModuleLayers.Contains(layer);
    }

    private static Assembly[] GetSolutionAssemblies()
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyPath == null)
        {
            throw new InvalidOperationException("Unable to determine assembly path.");
        }

        // Auto-detect module assemblies by naming convention: <Module>.(Contracts|Domain|Infrastructure|Application|Api)
        return Directory.GetFiles(assemblyPath, "*.dll")
            .Where(file => IsModuleAssemblyName(Path.GetFileNameWithoutExtension(file)))
            .Select(file => Assembly.LoadFrom(file))
            .ToArray();
    }

    [Fact]
    public void Modules_Should_Not_Depend_On_Other_Modules_Impl_Assemblies()
    {
        var allAssemblies = GetSolutionAssemblies();

        // Auto-detect modules based on assembly name prefixes (e.g., "LoanApplication")
        var modules = allAssemblies
            .Select(a => a.GetName().Name.Split('.')[0])
            .Distinct()
            .ToArray();

        foreach (var module in modules)
        {
            // Get all assemblies for this module
            var moduleAssemblies = allAssemblies
                .Where(a => a.GetName().Name.StartsWith(module + "."))
                .ToArray();

            // Get non-Contracts (implementation) assemblies for this module
            var moduleImplAssemblies = moduleAssemblies
                .Where(a => !a.GetName().Name.EndsWith(".Contracts"))
                .ToArray();

            if (moduleImplAssemblies.Length == 0) continue;

            // Get forbidden dependencies: other modules' non-Contracts assembly names (used for namespace matching)
            var forbiddenNamespaces = allAssemblies
                .Where(a => !a.GetName().Name.StartsWith(module + ".") && !a.GetName().Name.EndsWith(".Contracts"))
                .Select(a => a.GetName().Name)
                .ToArray();

            if (forbiddenNamespaces.Length == 0) continue;

            // Check rule: this module's impl should not depend on forbidden namespaces
            var result = Types.InAssemblies(moduleImplAssemblies)
                .Should()
                .NotHaveDependencyOnAny(forbiddenNamespaces)
                .GetResult();

            var failing = result.FailingTypeNames ?? Array.Empty<string>();
            Assert.True(result.IsSuccessful, $"Module '{module}' violates boundaries:{Environment.NewLine}{string.Join(Environment.NewLine, failing)}");
        }
    }

    [Fact]
    public void Modules_Should_Only_Use_Allowed_Layers()
    {
        var allAssemblies = GetSolutionAssemblies();
        var modules = allAssemblies
            .Select(a => a.GetName().Name.Split('.')[0])
            .Distinct()
            .ToArray();

        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyPath == null)
        {
            throw new InvalidOperationException("Unable to determine assembly path.");
        }

        var allDllNames = Directory.GetFiles(assemblyPath, "*.dll")
            .Select(Path.GetFileNameWithoutExtension)
            .ToArray();

        var offending = allDllNames
            .Select(name => new { name, parts = name.Split('.') })
            .Where(x => x.parts.Length == 2)
            .Where(x => modules.Contains(x.parts[0]))
            .Where(x => !ModuleLayers.Contains(x.parts[1]))
            .Select(x => x.name)
            .OrderBy(n => n)
            .ToArray();

        Assert.True(offending.Length == 0,
            $"Found module assemblies with non-allowed layers. Allowed layers: {string.Join(", ", ModuleLayers)}.{Environment.NewLine}Offending assemblies: {string.Join(", ", offending)}");
    }
}