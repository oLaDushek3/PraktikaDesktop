using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PraktikaDesktop
{
    public static class ApiRequest
    {

        public static HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();
            string baseUri = configuration.GetValue<string>("BaseUri");

            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        public static HttpResponseMessage Get(string request)
        {
            HttpClient client = CreateHttpClient();
            HttpResponseMessage response = client.GetAsync(request).Result;
            client.Dispose();

            return response;
        }

        public static HttpResponseMessage Post<T>(string request, T model)
        {
            HttpClient client = CreateHttpClient();
            HttpResponseMessage response = client.PostAsJsonAsync(request, model).Result;
            client.Dispose();

            return response;
        }

        public static HttpResponseMessage Put<T>(string request, T model)
        {
            HttpClient client = CreateHttpClient();
            HttpResponseMessage response = client.PutAsJsonAsync(request, model).Result;
            client.Dispose();

            return response;
        }

        public static HttpResponseMessage Delete(string request)
        {
            HttpClient client = CreateHttpClient();
            HttpResponseMessage response = client.DeleteAsync(request).Result;
            client.Dispose();

            return response;
        }
    }
}
