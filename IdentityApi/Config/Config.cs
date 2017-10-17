using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace Intreba.Identity.Api.Config
{
    public class Config
    {
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Phone(),
                new IdentityResources.Email(),
                new IdentityResource("name", new List<string>()
                {
                    JwtClaimTypes.Name
                }),
                new IdentityResource("role", new List<string>()
                {
                    JwtClaimTypes.Role
                }),
                new IdentityResource("account", new List<string>()
                {
                    "account"
                }),
                new IdentityResource("site", new List<string>()
                {
                    "site"
                }),
                new IdentityResource("product", new List<string>()
                {
                    "product"
                })
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("intreba.arke.api", "Intreba Arke API")
                {
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    UserClaims =
                    {
                        JwtClaimTypes.Name,
                        JwtClaimTypes.Email,
                        JwtClaimTypes.PhoneNumber,
                        JwtClaimTypes.Role,
                        "account",
                        "site",
                        "product"
                    }
                },
                new ApiResource("intreba.identity.api", "Intreba Identity API")
                {
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    UserClaims =
                    {
                        JwtClaimTypes.Name,
                        JwtClaimTypes.Email,
                        JwtClaimTypes.PhoneNumber,
                        JwtClaimTypes.Role,
                        "account",
                        "site",
                        "product"
                    }
                }
            };
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            // client credentials client
            return new List<Client>
            {

                // resource owner password grant client
                new Client
                {
                    ClientId = "intreba.arke.mobileapp",
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        "account",
                        "site",
                        "product",
                        "intreba.arke.api"
                    },
                    AbsoluteRefreshTokenLifetime = int.MaxValue, // 100 years
                    AccessTokenLifetime = 3600, //1hour
                    AlwaysSendClientClaims = true,
                    IdentityTokenLifetime = 300, //5 minutes
                    UpdateAccessTokenClaimsOnRefresh = true,
                    SlidingRefreshTokenLifetime = 1296000, //15 days
                    RefreshTokenExpiration = TokenExpiration.Absolute,
                    RefreshTokenUsage = TokenUsage.OneTimeOnly
                },
                new Client
                {
                    ClientId = "intreba.arke.api",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes =
                    {
                        "account",
                        "site",
                        "intreba.arke.api",
                        "product",
                        "intreba.identity.api"
                    },
                },
                //new Client
                //{
                //    ClientId = "client",
                //    AllowedGrantTypes = GrantTypes.ClientCredentials,

                //    ClientSecrets =
                //    {
                //        new Secret("secret".Sha256())
                //    },
                //    AllowedScopes = { "api1" }
                //},

                //// OpenID Connect hybrid flow and client credentials client (MVC)
                //new Client
                //{
                //    ClientId = "mvc",
                //    ClientName = "MVC Client",
                //    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

                //    ClientSecrets =
                //    {
                //        new Secret("secret".Sha256())
                //    },

                //    RedirectUris = { "http://localhost:5002/signin-oidc" },
                //    PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

                //    AllowedScopes =
                //    {
                //        IdentityServerConstants.StandardScopes.OpenId,
                //        IdentityServerConstants.StandardScopes.Profile,
                //        "api1"
                //    },
                //    AllowOfflineAccess = true
                //},

                //// JavaScript Client
                //new Client
                //{
                //    ClientId = "js",
                //    ClientName = "JavaScript Client",
                //    AllowedGrantTypes = GrantTypes.Implicit,
                //    AllowAccessTokensViaBrowser = true,

                //    RedirectUris = { "http://localhost:5003/callback.html" },
                //    PostLogoutRedirectUris = { "http://localhost:5003/index.html" },
                //    AllowedCorsOrigins = { "http://localhost:5003" },

                //    AllowedScopes =
                //    {
                //        IdentityServerConstants.StandardScopes.OpenId,
                //        IdentityServerConstants.StandardScopes.Profile,
                //        "api1"
                //    },
                //}
            };
        }

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice@arke.io",
                    Password = "password",
                    
                    Claims = new List<Claim>
                    {
                        new Claim("name", "Alice"),
                        new Claim("phone", "31650623486"),
                        new Claim("role", "host"),
                        new Claim("account", "C64692A5-DC51-47BB-9FF7-B511484C944B"),
                        new Claim("account", "7997F10D-1898-45C7-9F27-1480463CF357"),
                        new Claim("site", "9CE3190D-BBA2-4094-9749-BE6729368C35"),
                        new Claim("site", "99F3AF98-024D-45A7-BD10-E1A3D6FE56B0"),
                        new Claim("product", "arke"),
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob@arke.io",
                    Password = "password",
                    Claims = new List<Claim>
                    {
                        new Claim("name", "Bob"),
                        new Claim("phone", "31650623486"),
                        new Claim("role", "host"),
                        new Claim("role", "operator"),
                        new Claim("account", "C64692A5-DC51-47BB-9FF7-B511484C944B"),
                        new Claim("site", "9CE3190D-BBA2-4094-9749-BE6729368C35"),
                        new Claim("product", "arke"),
                        new Claim("product", "enm"),
                    }
                }
            };
        }
    }
}