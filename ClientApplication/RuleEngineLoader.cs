using ApprovalEngine.Domain.Interfaces; // For IInitialAssignmentRuleEngine, IReassignmentRuleEngine, and IApprovalSequenceRuleEngine
using ApprovalEngine.Application;       // For ApprovalWorkflowService
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.IO;

namespace ClientApplication
{
    public class RuleEngineLoader
    {
        public static IServiceProvider LoadRuleEngines()
        {
            var services = new ServiceCollection();

            // Register the ApprovalWorkflowService itself
            services.AddTransient<ApprovalWorkflowService>();

            string rulesAssemblyName = "ApprovalEngine.RuleSet.dll"; // Ensure this matches your RuleSet project's output DLL name
            string baseDirectory = AppContext.BaseDirectory;
            string rulesAssemblyPath = Path.Combine(baseDirectory, rulesAssemblyName);

            Console.WriteLine($"Attempting to load rules from: {rulesAssemblyPath}");

            if (!File.Exists(rulesAssemblyPath))
            {
                Console.WriteLine($"CRITICAL ERROR: Rules assembly '{rulesAssemblyName}' not found at '{rulesAssemblyPath}'. Please ensure it's copied to the output directory.");
                // In a real app, you might throw an exception or have a fallback mechanism.
                // For this example, we'll build an empty provider if the DLL is missing,
                // which will likely cause issues later if services are expected.
                return services.BuildServiceProvider();
            }

            try
            {
                Assembly rulesAssembly = Assembly.LoadFrom(rulesAssemblyPath);
                Console.WriteLine($"Successfully loaded assembly: {rulesAssembly.FullName}");

                var ruleEngineTypes = rulesAssembly.GetTypes()
                    .Where(t => !t.IsInterface && !t.IsAbstract)
                    .ToList();

                // Register IInitialAssignmentRuleEngine implementations
                foreach (var type in ruleEngineTypes.Where(t => typeof(IInitialAssignmentRuleEngine).IsAssignableFrom(t)))
                {
                    services.AddTransient(typeof(IInitialAssignmentRuleEngine), type);
                    Console.WriteLine($"Registered Initial Assignment Rule Engine: {type.FullName}");
                }

                // Register IReassignmentRuleEngine implementations
                foreach (var type in ruleEngineTypes.Where(t => typeof(IReassignmentRuleEngine).IsAssignableFrom(t)))
                {
                    services.AddTransient(typeof(IReassignmentRuleEngine), type);
                    Console.WriteLine($"Registered Reassignment Rule Engine: {type.FullName}");
                }

                // Register IApprovalSequenceRuleEngine implementations
                foreach (var type in ruleEngineTypes.Where(t => typeof(IApprovalSequenceRuleEngine).IsAssignableFrom(t)))
                {
                    services.AddTransient(typeof(IApprovalSequenceRuleEngine), type);
                    Console.WriteLine($"Registered Approval Sequence Rule Engine: {type.FullName}");
                }
            }
            catch (FileNotFoundException fnfEx)
            {
                Console.WriteLine($"CRITICAL ERROR: Rules assembly '{rulesAssemblyName}' could not be loaded (FileNotFound). Ensure it's in the output directory. Path: '{rulesAssemblyPath}'. Exception: {fnfEx.Message}");
            }
            catch (BadImageFormatException bifEx)
            {
                Console.WriteLine($"CRITICAL ERROR: Rules assembly '{rulesAssemblyName}' is not a valid .NET assembly. Path: '{rulesAssemblyPath}'. Exception: {bifEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred while loading or registering rules from assembly '{rulesAssemblyPath}': {ex.ToString()}");
            }

            return services.BuildServiceProvider();
        }
    }
}