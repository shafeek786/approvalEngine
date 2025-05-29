using ApprovalEngine.Domain.Interfaces; // For IInitialAssignmentRuleEngine and IReassignmentRuleEngine
using ApprovalEngine.Application;       // For ApprovalWorkflowService
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection; // New: For reflection capabilities
using System.IO;         // New: For path manipulation

namespace ClientApplication
{
    public class RuleEngineLoader
    {
        public static IServiceProvider LoadRuleEngines()
        {
            var services = new ServiceCollection();

            // Register the ApprovalWorkflowService itself
            services.AddTransient<ApprovalWorkflowService>();

            // --- NEW: Dynamic Loading of Rule Engines from Assembly ---
            string rulesAssemblyName = "ApprovalEngine.RuleSet.dll";
            string baseDirectory = AppContext.BaseDirectory; 
            string rulesAssemblyPath = Path.Combine(baseDirectory, rulesAssemblyName);

            Console.WriteLine($"Attempting to load rules from: {rulesAssemblyPath}");

            if (!File.Exists(rulesAssemblyPath))
            {
                Console.WriteLine($"Error: Rules assembly '{rulesAssemblyPath}' not found.");
                // As a fallback or error handling: consider registering default rules here if the DLL is optional.
                // For this example, we'll proceed, but it might fail if the DLL isn't there.
                // In a real app, you might throw or log and exit.
            }

            try
            {
                // Load the assembly dynamically
                Assembly rulesAssembly = Assembly.LoadFrom(rulesAssemblyPath);
                Console.WriteLine($"Successfully loaded assembly: {rulesAssembly.FullName}");

                // Find and register IInitialAssignmentRuleEngine implementations
                foreach (var type in rulesAssembly.GetTypes())
                {
                    if (typeof(IInitialAssignmentRuleEngine).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        services.AddTransient(typeof(IInitialAssignmentRuleEngine), type);
                        Console.WriteLine($"Registered Initial Assignment Rule Engine: {type.FullName}");
                    }
                }

                // Find and register IReassignmentRuleEngine implementations
                foreach (var type in rulesAssembly.GetTypes())
                {
                    if (typeof(IReassignmentRuleEngine).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        services.AddTransient(typeof(IReassignmentRuleEngine), type);
                        Console.WriteLine($"Registered Reassignment Rule Engine: {type.FullName}");
                    }
                }
            }
            catch (FileNotFoundException fnfEx)
            {
                Console.WriteLine($"CRITICAL ERROR: Rules assembly '{rulesAssemblyName}' not found at '{rulesAssemblyPath}'. Please ensure it's copied to the output directory. Exception: {fnfEx.Message}");
            }
            catch (BadImageFormatException bifEx)
            {
                Console.WriteLine($"CRITICAL ERROR: Rules assembly '{rulesAssemblyName}' is not a valid .NET assembly. Exception: {bifEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while loading rules assembly: {ex.Message}");
            }

            return services.BuildServiceProvider();
        }
    }
}