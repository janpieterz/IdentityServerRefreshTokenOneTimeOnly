using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;

namespace Mobile
{
    class Program
    {
        static void Main(string[] args) => new Program().Run().GetAwaiter().GetResult();
        private static string _authority = "http://localhost:63115";
        private static string _api = "http://localhost:64665";
        private string _aliceRefreshToken = "7fb83d90a4ff23d13b745d54e370b934a44eee30df63f78d5fe53b98ba7395b3";
        private async Task Run()
        {
            Console.WriteLine("Hello World!");

            while (true)
            {
                try
                {
                    Console.WriteLine("Do you want to login or register?");

                    var action = Console.ReadLine();

                    if (action == "login")
                    {
                        await Login().ConfigureAwait(false);
                        
                    }
                    else if (action == "register")
                    {
                        await Register().ConfigureAwait(false);
                    }
                    else
                    {
                        Console.WriteLine($"We don't understand action '{action}'");
                    }

                    //var question = await CreateLoginSessionForUser(username);

                    //Console.WriteLine($"Please enter your {question}:");
                    //var password = Console.ReadLine();

                    //await LogInAndWriteUserClaims(username, password);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            }

            Console.ReadLine();
        }

        private async Task Register()
        {
            var tokenClient = new TokenClient($"{_authority}/connect/token", _clientId, "secret");
            var clientToken = await tokenClient.RequestClientCredentialsAsync("intreba.arke.api");
            if (clientToken.IsError)
            {
                Console.WriteLine(clientToken.ErrorDescription);
                return;
            }
            HttpClient client = new HttpClient();
            client.SetBearerToken(clientToken.AccessToken);

            Console.WriteLine("What's your phone number?");
            var phoneNumber = Console.ReadLine();

            var getCodeResult = await GetCode(client, phoneNumber);
            if (!getCodeResult)
            {
                return;
            }
            Console.WriteLine("What code did you receive?");
            var code = Console.ReadLine();
            var codeVerified = await VerifyCode(client, phoneNumber, code);
            if (!codeVerified)
            {
                return;
            }
            Console.WriteLine("What's your email address?");
            var emailAddress = Console.ReadLine();
            Console.WriteLine("What's your password? Make it 8 characters or more.");
            var password = Console.ReadLine();
            
            await client.PostAsync($"{_api}/api/users", new StringContent(JsonConvert.SerializeObject(new
            {
                phoneNumber = phoneNumber,
                emailAddress = emailAddress,
                password = password
            }), Encoding.UTF8, "application/json")).ConfigureAwait(false);
        }

        private async Task<Boolean> VerifyCode(HttpClient client, string phoneNumber, string code)
        {
            var result = await client.GetAsync($"{_api}/api/users/phone/{phoneNumber}/verification/{code}").ConfigureAwait(false);
            if (result.IsSuccessStatusCode)
            {
                return true;
            } else 
            {
                Console.WriteLine(await result.Content.ReadAsStringAsync());
                return false;
            }
        }

        private async Task<Boolean> GetCode(HttpClient client, string phoneNumber)
        {
            var result = await client.PostAsync($"{_api}/api/users/phone/{phoneNumber}/verification", new StringContent(""));
            if (result.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                Console.WriteLine(await result.Content.ReadAsStringAsync());
                return false;
            }
        }

        private static string _clientId = "intreba.arke.mobileapp";
        private async Task Login()
        {
            Console.WriteLine("Type email to log in with (alice@arke.io, bob@arke.io or other) Alice can have a refresh token");
            var username = Console.ReadLine();
            TokenResponse tokenResponse;
            var tokenClient = new TokenClient($"{_authority}/connect/token", _clientId, "secret");
            if (username == "alice@arke.io" && _aliceRefreshToken != string.Empty)
            {
                tokenResponse = await tokenClient.RequestRefreshTokenAsync(_aliceRefreshToken).ConfigureAwait(false);
                Console.WriteLine($"New refresh token: {tokenResponse.RefreshToken}");
            }
            else
            {
                Console.WriteLine($"Type the password for {username}");
                var password = Console.ReadLine();
                tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(username, password, "intreba.arke.api offline_access");
            }
            
            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.ErrorDescription);
            }
            await PrintAccountsAndSites(username, tokenResponse).ConfigureAwait(false);
        }

        private async Task PrintAccountsAndSites(string username, TokenResponse response)
        {
            Console.WriteLine("Refresh token:");
            Console.WriteLine(response.RefreshToken);
            HttpClient client = new HttpClient();
            client.SetBearerToken(response.AccessToken);
            var items = string.Empty;

            var accounts = await client.GetAsync($"{_api}/api/users/{username}/accounts").ConfigureAwait(false);
            items = accounts.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var accountsTyped = JsonConvert.DeserializeObject<List<Guid>>(items);
            foreach (Guid guid in accountsTyped)
            {
                Console.WriteLine($"Account {guid}");
            }

            var sites = await client.GetAsync($"{_api}/api/users/{username}/sites").ConfigureAwait(false);
            items = sites.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var sitesTyped = JsonConvert.DeserializeObject<List<Guid>>(items);
            foreach (Guid guid in sitesTyped)
            {
                Console.WriteLine($"Site {guid}");
            }
            Console.WriteLine();
        }
    }
}
