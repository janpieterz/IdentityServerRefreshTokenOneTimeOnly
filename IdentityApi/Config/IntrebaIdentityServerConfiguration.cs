using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Test;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Intreba.Identity.Api.Config
{
    public static class IntrebaIdentityServerConfiguration
    {
        public static void AddIntrebaIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            var thumbprint = configuration["SigningThumbprint"];
            var certs = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (certs.Count != 1)
            {
                throw new Exception($"Found {certs.Count} matches for IntrebaIdentity signing certificate with thumbprint {thumbprint}");
            }
            string connectionString = configuration["SqlPersistence"];
            string migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services
                .AddIdentityServer()
                .AddAspNetIdentity<ApplicationUser>()
                .AddSigningCredential(certs[0])
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .Services.AddTransient<IResourceOwnerPasswordValidator, ValidateUsers>(); ;
        }

        public static void InitializeIdentityDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {

                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApiResources())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }

        public static void InitializeUserDatabase(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var userManager = serviceScope.ServiceProvider.GetService<UserManager<ApplicationUser>>();
                foreach (TestUser testUser in Config.GetUsers())
                {
                    var user = userManager.FindByNameAsync(testUser.Username).GetAwaiter().GetResult();
                    if (user == null)
                    {
                        var result = userManager.CreateAsync(new ApplicationUser
                        {
                            UserName = testUser.Username,
                        }, testUser.Password).GetAwaiter().GetResult();
                        user = userManager.FindByNameAsync(testUser.Username).GetAwaiter().GetResult();
                        userManager.AddClaimsAsync(user, testUser.Claims).GetAwaiter().GetResult();
                    }
                }
            }
        }
    }

    public class ValidateUsers : IResourceOwnerPasswordValidator
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ValidateUsers(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            ApplicationUser user = await _userManager.FindByNameAsync(context.UserName);
            if (user != null)
            {
                var signedIn = await _signInManager.CheckPasswordSignInAsync(user, context.Password, false).ConfigureAwait(false);
                if (signedIn.Succeeded)
                {
                    var claimsPrincipal = await _signInManager.CreateUserPrincipalAsync(user);

                    context.Result = new GrantValidationResult(claimsPrincipal.GetSubjectId(), OidcConstants.AuthenticationMethods.Password, claimsPrincipal.Claims);
                }
            }
            //if (context.Password == "password")
            //{
            //    profileServices.GetProfileDataAsync(new ProfileDataRequestContext(new ClaimsPrincipal(new IdentityResource(context.UserName, context.Request.ClientClaims.Select(t => t.Type)))))
            //    var user = Config.GetUsers().SingleOrDefault(x => x.Username == context.UserName);

            //    context.Result = new GrantValidationResult(new ClaimsPrincipal());
            //}
            //return Task.CompletedTask;
        }
    }
}