using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using WebApiClientCore.Attributes;
using WebApiClientCore.Exceptions;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace BankClient
{
    public class WebapiClientExample : Example
    {
        public override string DisplayName => "WebapiClient";

        private IBank bankClient;

        public WebapiClientExample(IBank bank)
        {
            bankClient = bank;
        }

        public interface IBank
        {
            [HttpGet("/accounts/{accountId}")]
            Task<Account> GetUser(string accountId, CancellationToken cancellationToken = default);

            [HttpPost("/deposit")]
            Task<Account> Deposit([JsonContent] Transaction transaction, CancellationToken cancellationToken = default);

            [HttpPost("/withdraw")]
            Task<HttpResponseMessage> Withdraw([JsonContent] Transaction transaction, CancellationToken cancellationToken = default);
        }

        public override async Task RunAsync(CancellationToken cancellationToken)
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
 

            // Scenario 1: Check if the account already exists.
            Account? account = null;
            try
            {
                account = await bankClient.GetUser("17", cancellationToken);
            }
            catch (ApiException ex)  
            {
                // Account does not exist.
            }

            Console.WriteLine($"Scenario 1: account '17' {(account is null ? "does not exist" : "already exists")}");

            // Scenario 2: Deposit some money
            var transaction = new Transaction()
            {
                Amount = 100m,
                Id = "17",
            };

            // read updated balance
            account = await bankClient.Deposit(transaction, cancellationToken);
            Console.WriteLine($"Scenario 2: account '17' has '{account?.Balance}' money");

            // Scenario 3: Handle a validation error without exceptions
            transaction = new Transaction()
            {
                Amount = 1_000_000m,
                Id = "17",
            };
            
            var response = await bankClient.Withdraw(transaction, cancellationToken);
            if (response.StatusCode != HttpStatusCode.BadRequest)
            {
                // We don't actually expect this example to succeed - we expect a 400
                Console.WriteLine("Something went wrong :(");
                return;
            }

            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: cancellationToken);
            Console.WriteLine($"Scenario 3: got the following errors:");
            foreach (var kvp in problem!.Errors)
            {
                Console.WriteLine($"{kvp.Key}: {string.Join(", ", kvp.Value)}");
            }
        }
    }
}