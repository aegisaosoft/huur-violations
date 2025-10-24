/*
 *
 * Copyright (c) 2025 Alexander Orlov.
 * 34 Middletown Ave Atlantic Highlands NJ 07716
 *
 * THIS SOFTWARE IS THE CONFIDENTIAL AND PROPRIETARY INFORMATION OF
 * Alexander Orlov. ("CONFIDENTIAL INFORMATION"). YOU SHALL NOT DISCLOSE
 * SUCH CONFIDENTIAL INFORMATION AND SHALL USE IT ONLY IN ACCORDANCE
 * WITH THE TERMS OF THE LICENSE AGREEMENT YOU ENTERED INTO WITH
 * Alexander Orlov.
 *
 * Author: Alexander Orlov
 *
 */

using System.Net;

namespace HuurVioaltionsAPI.Abstr
{
    public class AHttpFinder
    {
        protected readonly HttpClient _httpClient;
        public AHttpFinder()
        {
            var cookieJar = new CookieContainer();
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieJar,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };

            _httpClient = new HttpClient(handler);

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36"
            );
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/plain, */*");
            _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            _httpClient.DefaultRequestHeaders.Referrer = new Uri("https://site.metropolis.io/");
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://site.metropolis.io");
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

    }
}
