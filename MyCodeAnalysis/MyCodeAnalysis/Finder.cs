using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis;

namespace MyCodeAnalysis
{
    public static class Finder
    {
        private static HashSet<string> alreadyProcessed = new HashSet<string>();
        private static HashSet<string> affectedPlaces = new HashSet<string>();

        public static void FindAffectedPlaces()
        {
            var solutionPath = @"C:\Cantaloupe\Versioned\Main\Src\All.sln";

            var workspace = MSBuildWorkspace.Create();
            var solution = workspace.OpenSolutionAsync(solutionPath).Result;

            FindCallers(
                solution,
                namespaceName: "Cantaloupe.Seed.Aeon.Website.Common.CustomerCommon",
                className: "AllowedUsersResolver",
                methodName: "ResolveAllowedUsers");

            Console.WriteLine("\nAffectedPlaces:\n");

            foreach(var affectedPlace in affectedPlaces)
                Console.WriteLine(affectedPlace);
        }

        private static void FindCallers(
            Solution solution,
            string namespaceName,
            string className,
            string methodName,
            string indent = "")
        {
            var methodSymbol = LoadMethodSymbol(solution, namespaceName, className, methodName);
            if (methodSymbol != null)
            {
                var callers = SymbolFinder.FindCallersAsync(methodSymbol, solution).Result;
                if(!callers.Any())
                {
                    affectedPlaces.Add($"{namespaceName}.{className}");
                }
                foreach (var caller in callers)
                {
                    var item = $"{namespaceName}.{caller.CallingSymbol.ContainingType.Name}.{caller.CallingSymbol.Name}";
                    if (alreadyProcessed.Contains(item))
                        continue;
                    else
                        alreadyProcessed.Add(item);

                    Console.WriteLine($"{indent}{namespaceName}.{caller.CallingSymbol.ContainingType.Name}.{caller.CallingSymbol.Name}");

                    FindCallers(
                        solution, 
                        caller.CallingSymbol.ContainingType.ContainingNamespace.ToString(), 
                        caller.CallingSymbol.ContainingType.Name, 
                        caller.CallingSymbol.Name, 
                        indent + "-");
                }
            }
        }

        private static ISymbol LoadMethodSymbol(Solution solution, string namespaceName, string className, string methodName)
        {
            foreach (var project in solution.Projects)
            {
                foreach (var document in project.Documents)
                {
                    var methodInvocation = document.GetSyntaxRootAsync().Result;
                    var members = methodInvocation.DescendantNodes().OfType<MemberDeclarationSyntax>();

                    foreach (var member in members)
                    {
                        var method = member as MethodDeclarationSyntax;
                        if (method != null)
                        {
                            if (method.Identifier.Text == methodName)
                            {
                                var classDeclarationSyntax = method.Parent as ClassDeclarationSyntax;
                                if (classDeclarationSyntax != null)
                                {
                                    if (classDeclarationSyntax.Identifier.ValueText == className)
                                    {
                                        var namespaceDeclarationSyntax = classDeclarationSyntax.Parent as NamespaceDeclarationSyntax;
                                        if (namespaceDeclarationSyntax != null)
                                        {
                                            if (namespaceDeclarationSyntax.Name.ToString() == namespaceName)
                                                return document.GetSemanticModelAsync().Result?.GetDeclaredSymbol(method);
                                        }                                       
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}