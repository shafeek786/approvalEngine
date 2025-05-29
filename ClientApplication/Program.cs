using ApprovalEngine.Domain;
using ApprovalEngine.Application;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace ClientApplication
{
    internal class Program
    {
        private static Dictionary<string, RoleId> AllUsers = Enum.GetNames(typeof(RoleNames))
            .ToDictionary(name => name, name => new RoleId(name));


        private const string UniversalCategory = "UniversalApprovalRequest";
        private static ApprovalWorkflowService? _approvalService;
        private static List<ApprovalRequest> _allRequests = new();

        static void Main(string[] args)
        {
            Console.WriteLine("== Welcome to the Interactive Approval Engine ==");

            var serviceProvider = RuleEngineLoader.LoadRuleEngines();
            _approvalService = serviceProvider.GetRequiredService<ApprovalWorkflowService>();

            while (true)
            {
                Console.WriteLine("\nEnter your username to log in (or type 'exit'):");
                var input = Console.ReadLine()?.Trim();

                if (input?.ToLower() == "exit") break;

                if (input == null || !AllUsers.ContainsKey(input))
                {
                    Console.WriteLine("Invalid user. Try again.");
                    continue;
                }

                var user = AllUsers[input];

                if (input.StartsWith("approver", StringComparison.OrdinalIgnoreCase) || input == "SeniorAdminApprover")
                {
                    ApproverMenu(user);
                }
                else
                {
                    RequesterMenu(user);
                }
            }

            Console.WriteLine("Goodbye!");
        }

        static void RequesterMenu(RoleId user)
        {
            while (true)
            {
                Console.WriteLine($"\n--- Requester Menu ({user.Value}) ---");
                Console.WriteLine("1. Submit new request");
                Console.WriteLine("2. View my requests");
                Console.WriteLine("3. Logout");

                var input = Console.ReadLine();
                if (input == "1")
                {
                    Console.Write("Enter request title: ");
                    var title = Console.ReadLine() ?? "Untitled";
                    var payload = new { RequestName = title };
                    var payloadJson = JsonSerializer.Serialize(payload);
                    var request = _approvalService!.SubmitRequest(UniversalCategory, payloadJson, user);
                    _allRequests.Add(request);
                    Console.WriteLine($"Request submitted. Assigned to: {request.AssignedToUser}");
                }
                else if (input == "2")
                {
                    var myRequests = _allRequests.Where(r => r.RequestedBy.Equals(user)).ToList();
                    if (!myRequests.Any())
                        Console.WriteLine("No requests found.");
                    else
                        foreach (var r in myRequests)
                            PrintRequest(r);
                }
                else if (input == "3")
                {
                    break;
                }
                else Console.WriteLine("Invalid input.");
            }
        }

        static void ApproverMenu(RoleId user)
        {
            while (true)
            {
                Console.WriteLine($"\n--- Approver Menu ({user.Value}) ---");
                Console.WriteLine("1. View assigned requests");
                Console.WriteLine("2. Logout");

                var input = Console.ReadLine();
                if (input == "1")
                {
                    var assignedRequests = _allRequests.Where(r => r.IsAssignedTo(user)).ToList();

                    if (!assignedRequests.Any())
                    {
                        Console.WriteLine("No assigned requests.");
                        continue;
                    }

                    for (int i = 0; i < assignedRequests.Count; i++)
                    {
                        Console.WriteLine("-----------------------------------------------------------------------------");
                        Console.WriteLine($"{i + 1}. Request ID: {assignedRequests[i].Id}");
                        
                        Console.WriteLine("-----------------------------------------------------------------------------");
                    }

                    Console.Write("Select request number to manage: ");
                    if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= assignedRequests.Count)
                    {
                        var request = assignedRequests[index - 1];
                        ManageRequest(user, request);
                    }
                    else
                    {
                        Console.WriteLine("Invalid selection.");
                    }
                }
                else if (input == "2")
                {
                    break;
                }
                else Console.WriteLine("Invalid input.");
            }
        }

        static void ManageRequest(RoleId user, ApprovalRequest request)
        {
            while (true)
            {
                Console.WriteLine($"\n--- Managing Request {request.Id} ---");
                PrintRequest(request);
                Console.WriteLine("1. Approve");
                Console.WriteLine("2. Reject");
                Console.WriteLine("3. Reassign");
                Console.WriteLine("4. Back");

                var input = Console.ReadLine();
                try
                {
                    if (input == "1")
                    {
                        _approvalService!.ApproveRequest(request, user, "Approved via console");
                        Console.WriteLine("Request approved.");
                        break;
                    }
                    else if (input == "2")
                    {
                        Console.Write("Enter rejection reason: ");
                        var reason = Console.ReadLine() ?? "No reason";
                        _approvalService!.RejectRequest(request, user, reason);
                        Console.WriteLine("Request rejected.");
                        break;
                    }
                    else if (input == "3")
                    {
                        Console.Write("Enter new approver username: ");
                        var newApproverName = Console.ReadLine();
                        if (newApproverName != null && AllUsers.TryGetValue(newApproverName, out var newApprover))
                        {
                            _approvalService!.ReassignRequest(request, user, newApprover, "Reassigned via console");
                            Console.WriteLine($"Reassigned to {newApprover.Value}");
                            break;
                        }
                        else Console.WriteLine("Invalid approver.");
                    }
                    else if (input == "4") break;
                    else Console.WriteLine("Invalid input.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static void PrintRequest(ApprovalRequest request)
        {
            Console.WriteLine("-----------------------------------------------------------------------------------------");
            Console.WriteLine($"\nRequest ID: {request.Id}");
            Console.WriteLine($"Status: {request.CurrentStatus}");
            Console.WriteLine($"Assigned To: {request.AssignedToUser}");
            Console.WriteLine($"Requested By: {request.RequestedBy}");
            Console.WriteLine($"Payload: {request.RequestPayloadJson}"); 
            Console.WriteLine("History:");
            foreach (var e in request.History)
                Console.WriteLine($"- {e}");
            Console.WriteLine("-----------------------------------------------------------------------------------------");
        }
        
    }
}
