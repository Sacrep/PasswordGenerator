using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace PasswordGenerator
{
    public static class GeneratePasswordFunction
    {
        [FunctionName("GeneratePassword")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "{account}/{length}")] HttpRequest req,
            ILogger log,
            string account,
            int length = 12)
        {
            log.LogInformation($"GeneratePassword called to generate password for {account} account");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string master = data?.master;

            if (master == null)
            {
                return new BadRequestObjectResult("No master password found");
            }

            return new OkObjectResult(GeneratePassword(master, account, length));
        }


        private static string GeneratePassword(string master, string accountName, int length)
        {
            // Using the accountName as a seed, 
            var result = new StringBuilder();
            const string pool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%&?_-+";

            // Convert master string to number to use as seed
            int seed = 0;
            foreach (char c in accountName)
            {
                seed += c;
            }
            var random = new Random(seed);

            for (int i = 0; i < length; i++)
            {
                char c = master[i % master.Length];
                result.Append(pool[random.Next(100) * c % pool.Length]);
            }

            return result.ToString();
        }
    }
}
