using Microsoft.Extensions.Hosting; // For BackgroundService
using Microsoft.Extensions.DependencyInjection; // For IServiceScopeFactory and GetRequiredService
using System;
using System.Threading;
using System.Threading.Tasks;
using Auto_Insurance_Management_System.Services; // Assuming IPolicyService is in this namespace
using System.Collections.Generic; // Potentially needed if List is explicitly used

namespace Auto_Insurance_Management_System.BackgroundServices
{
    // Make sure this class inherits from BackgroundService
    public class PolicyLifecycleBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        // Constructor to inject dependencies
        public PolicyLifecycleBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        // The core logic of the background service
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Create a new scope for each execution to ensure services are correctly resolved
                // and to avoid capturing scoped services for the lifetime of the singleton background service.
                using (var scope = _scopeFactory.CreateScope())
                {
                    // Resolve IPolicyService from the current scope
                    var policyService = scope.ServiceProvider.GetRequiredService<IPolicyService>();

                    // Fetch all active policies to process their lifecycle
                    // The GetAllPoliciesAsync method needs to be able to fetch policies
                    // based on criteria relevant for lifecycle checks (e.g., all active policies).
                    var allPolicies = await policyService.GetAllPoliciesAsync(null, "ACTIVE"); //

                    // Iterate through each policy and process its lifecycle
                    foreach (var policy in allPolicies) //
                    {
                        // Call the method that contains the policy lifecycle logic.
                        // This method will check if a policy needs to be expired, renewed, etc.
                        await policyService.ProcessPolicylifecycleAsync(policy.PolicyId); //
                    }
                }

                // Wait for a specified duration before the next execution.
                // Adjust this delay based on how frequently you need to run the lifecycle check.
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken); //
            }
        }

        // You might also implement StartAsync and StopAsync if you need custom startup/shutdown logic,
        // but for simple background tasks, ExecuteAsync is often sufficient.
        // public override Task StartAsync(CancellationToken cancellationToken) { ... }
        // public override Task StopAsync(CancellationToken cancellationToken) { ... }
    }
}