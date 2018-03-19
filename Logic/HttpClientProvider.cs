using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Web.Providers
{
    public static class HttpClientProvider
    {
        private static HttpClient _httpClient;
        public static HttpClient GetClient()
        {
            return (_httpClient != null) ? _httpClient : _httpClient = new HttpClient(); 
        }
    }
}