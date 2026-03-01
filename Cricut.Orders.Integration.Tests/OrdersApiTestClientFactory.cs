using Cricut.Orders.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Cricut.Orders.Integration.Tests
{
    public static class OrdersApiTestClientFactory
    {
        private static WebApplicationFactory<Startup> _webAppFactory = new WebApplicationFactory<Startup>();
        public static HttpClient CreateTestClient() => _webAppFactory.CreateClient();
    }
}
