using ApprovalEngine.Domain;
using ApprovalEngine.Application;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;

namespace ApprovalEngine.ConsoleApp
{
    internal class Program
    {
        private const string UniversalCategory = "UniversalApprovalRequest";

        private static readonly UserId Approver1 = new UserId("approver1");
        private static readonly UserId Approver2 = new UserId("approver2");
        private static readonly UserId Approver3 = new UserId("approver3");
        private static readonly UserId Approver4 = new UserId("approver4");
        private static readonly UserId Approver5 = new UserId("approver5");
        private static readonly UserId SeniorAdminApprover = new UserId("SeniorAdminApprover");


        static void Main(string[] args)
        {
            Console.WriteLine("Starting Generic Approval Engine with Dynamic Reassignment...");

            var serviceProvider = RuleEngineLoader.LoadRuleEngines();
            var approvalService = serviceProvider.GetRequiredService<ApprovalWorkflowService>();

            Console.WriteLine("\n--- Scenario 1: Initial Assignment & Direct Reassignment (Approver1 -> Approver4) ---");
            var payload1 = new { RequestName = "Complex Project Approval", Budget = 75000.00 };
            var payloadJson1 = JsonSerializer.Serialize(payload1);
            var requester1 = new UserId("ProjectManager");

            try
            {
                Console.WriteLine($"Submitting Request: {payloadJson1}");
                var request1 = approvalService.SubmitRequest(UniversalCategory, payloadJson1, requester1);
                Console.WriteLine($"Request ID: {request1.Id}, Status: {request1.CurrentStatus}, Assigned To: {request1.AssignedToUser}");
                PrintHistory(request1);

                Console.WriteLine($"\nApprover1 ({Approver1.Value}) reassigns directly to Approver4 ({Approver4.Value}).");
                approvalService.ReassignRequest(request1, Approver1, Approver4, "Direct escalation for specific expertise.");
                Console.WriteLine($"Request ID: {request1.Id}, Status: {request1.CurrentStatus}, Assigned To: {request1.AssignedToUser}");
                PrintHistory(request1);

                Console.WriteLine($"\nApprover4 ({Approver4.Value}) approves the request.");
                approvalService.ApproveRequest(request1, Approver4, "Approved after expert review.");
                Console.WriteLine($"Request ID: {request1.Id}, Status: {request1.CurrentStatus}, Assigned To: {request1.AssignedToUser}");
                PrintHistory(request1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Scenario 1: {ex.Message}");
            }

            Console.WriteLine("\n--- Scenario 2: Invalid Reassignment Attempt ---");
            var payload2 = new { RequestName = "New Employee Onboarding", Department = "HR" };
            var payloadJson2 = JsonSerializer.Serialize(payload2);
            var requester2 = new UserId("HRCoordinator");
            var invalidApprover = new UserId("NonExistentUser"); // An approver not in our valid list

            // --- FIX START ---
            ApprovalRequest? request2 = null; // Declare request2 outside the try block
            // --- FIX END ---

            try
            {
                Console.WriteLine($"\nSubmitting Request: {payloadJson2}");
                request2 = approvalService.SubmitRequest(UniversalCategory, payloadJson2, requester2); // Assign value within try
                Console.WriteLine($"Request ID: {request2.Id}, Status: {request2.CurrentStatus}, Assigned To: {request2.AssignedToUser}");
                PrintHistory(request2);

                Console.WriteLine($"\nApprover1 ({Approver1.Value}) attempts to reassign to an invalid user ({invalidApprover.Value}).");
                approvalService.ReassignRequest(request2, Approver1, invalidApprover, "Attempting to reassign to an unknown user.");
                Console.WriteLine($"Request ID: {request2.Id}, Status: {request2.CurrentStatus}, Assigned To: {request2.AssignedToUser}"); // This line won't be reached if exception occurs
                PrintHistory(request2); // This line won't be reached if exception occurs
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught expected error: {ex.Message}");
                // Add a null check before printing history, in case submission itself failed
                if (request2 != null)
                {
                    PrintHistory(request2);
                }
                else
                {
                    Console.WriteLine("Request was null, likely failed during initial submission.");
                }
            }

            Console.WriteLine("\n--- Scenario 3: Reassign to Senior Admin ---");
            var payload3 = new { RequestName = "High Value Contract", ContractId = "C-12345" };
            var payloadJson3 = JsonSerializer.Serialize(payload3);
            var requester3 = new UserId("LegalCounsel");

            try
            {
                Console.WriteLine($"\nSubmitting Request: {payloadJson3}");
                var request3 = approvalService.SubmitRequest(UniversalCategory, payloadJson3, requester3);
                Console.WriteLine($"Request ID: {request3.Id}, Status: {request3.CurrentStatus}, Assigned To: {request3.AssignedToUser}");
                PrintHistory(request3);

                Console.WriteLine($"\nApprover1 ({Approver1.Value}) reassigns directly to SeniorAdminApprover ({SeniorAdminApprover.Value}).");
                approvalService.ReassignRequest(request3, Approver1, SeniorAdminApprover, "Escalating for executive review.");
                Console.WriteLine($"Request ID: {request3.Id}, Status: {request3.CurrentStatus}, Assigned To: {request3.AssignedToUser}");
                PrintHistory(request3);

                Console.WriteLine($"\nSeniorAdminApprover ({SeniorAdminApprover.Value}) rejects the request.");
                approvalService.RejectRequest(request3, SeniorAdminApprover, "Contract terms unacceptable.");
                Console.WriteLine($"Request ID: {request3.Id}, Status: {request3.CurrentStatus}, Assigned To: {request3.AssignedToUser}");
                PrintHistory(request3);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Scenario 3: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        static void PrintHistory(ApprovalRequest request)
        {
            Console.WriteLine("--- History ---");
            foreach (var approvalEvent in request.History)
            {
                Console.WriteLine($"- {approvalEvent}");
            }
            Console.WriteLine("---------------");
        }
    }
}