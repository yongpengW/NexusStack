using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace NexusStack.Infrastructure.Client
{
    /// <summary>
    /// HTTP 请求客户端服务
    /// 使用 IHttpClientFactory 和异步方法
    /// </summary>
    public class HttpClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpClientService>? _logger;

        public HttpClientService(IHttpClientFactory httpClientFactory, ILogger<HttpClientService>? logger = null)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger;
        }

        /// <summary>
        /// GET 请求（异步）
        /// </summary>
        public async Task<string?> GetAsync(string url, List<KeyValuePair<string, string>>? dataParameter = null, string authorization = "", CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                ConfigureDefaultHeaders(httpClient, authorization);

                if (dataParameter != null && dataParameter.Count > 0)
                {
                    var queryString = string.Join("&", dataParameter.ConvertAll(x => $"{x.Key}={x.Value}"));
                    url += (url.Contains("?") ? "&" : "?") + queryString;
                }

                var response = await httpClient.GetAsync(url, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync(cancellationToken);
                }

                _logger?.LogWarning("GET request failed. URL: {Url}, StatusCode: {StatusCode}", url, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GET request exception. URL: {Url}", url);
                return null;
            }
        }

        /// <summary>
        /// POST 请求（异步）
        /// </summary>
        public async Task<string?> PostAsync(string url, object dataJson, string authorization = "", CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                ConfigureDefaultHeaders(httpClient, authorization);

                var data = JsonConvert.SerializeObject(dataJson);
                var httpContent = new StringContent(data, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, httpContent, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync(cancellationToken);
                }

                _logger?.LogWarning("POST request failed. URL: {Url}, StatusCode: {StatusCode}", url, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "POST request exception. URL: {Url}", url);
                throw;
            }
        }

        /// <summary>
        /// Form 表单请求（异步）
        /// </summary>
        public async Task<string?> FormAsync(string url, List<KeyValuePair<string, string>> dataArray, string authorization, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                
                if (!string.IsNullOrWhiteSpace(authorization))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                }

                // MultipartFormDataContent 会自动设置正确的 Content-Type
                var httpContent = new MultipartFormDataContent();
                foreach (var data in dataArray)
                {
                    httpContent.Add(new StringContent(data.Value), data.Key);
                }

                var response = await httpClient.PostAsync(url, httpContent, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync(cancellationToken);
                }

                _logger?.LogWarning("Form request failed. URL: {Url}, StatusCode: {StatusCode}", url, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Form request exception. URL: {Url}", url);
                return null;
            }
        }

        /// <summary>
        /// POST 表单数据（异步）
        /// </summary>
        public async Task<string> PostFormUrlEncodedAsync(string url, Dictionary<string, string> param, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var content = new FormUrlEncodedContent(param);
                
                var response = await httpClient.PostAsync(url, content, cancellationToken);
                return await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PostFormUrlEncoded request exception. URL: {Url}", url);
                return string.Empty;
            }
        }

        /// <summary>
        /// PUT 请求（异步）
        /// </summary>
        public async Task<string?> PutAsync(string url, object dataJson, string authorization, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                ConfigureDefaultHeaders(httpClient, authorization);

                var httpContent = new StringContent(JsonConvert.SerializeObject(dataJson), Encoding.UTF8, "application/json");
                
                var response = await httpClient.PutAsync(url, httpContent, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync(cancellationToken);
                }

                _logger?.LogWarning("PUT request failed. URL: {Url}, StatusCode: {StatusCode}", url, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "PUT request exception. URL: {Url}", url);
                return null;
            }
        }

        /// <summary>
        /// DELETE 请求（异步）
        /// </summary>
        public async Task<string?> DeleteAsync(string url, List<KeyValuePair<string, string>>? dataParameter, string authorization, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                ConfigureDefaultHeaders(httpClient, authorization);

                if (dataParameter != null && dataParameter.Count > 0)
                {
                    var queryString = string.Join("&", dataParameter.ConvertAll(x => $"{x.Key}={x.Value}"));
                    url += (url.Contains("?") ? "&" : "?") + queryString;
                }

                var response = await httpClient.DeleteAsync(url, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync(cancellationToken);
                }

                _logger?.LogWarning("DELETE request failed. URL: {Url}, StatusCode: {StatusCode}", url, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "DELETE request exception. URL: {Url}", url);
                return null;
            }
        }

        /// <summary>
        /// 发送钉钉消息（异步）
        /// </summary>
        public async Task<string?> PostDingtalkMsgAsync(string url, string msg, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            var dingtalkObj = new
            {
                msgtype = "text",
                text = new
                {
                    content = msg
                }
            };
            
            return await PostAsync(url, dingtalkObj, string.Empty, cancellationToken);
        }

        /// <summary>
        /// 发送企业微信消息（异步）
        /// </summary>
        public async Task<string?> PostQyWinXinMsgAsync(string url, string msg, string mentionedMobile = "", CancellationToken cancellationToken = default)
        {
            var msgObj = new
            {
                msgtype = "text",
                text = new
                {
                    content = msg,
                    mentioned_mobile_list = mentionedMobile.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                }
            };
            
            return await PostAsync(url, msgObj, string.Empty, cancellationToken);
        }

        /// <summary>
        /// 配置默认请求头
        /// </summary>
        private void ConfigureDefaultHeaders(HttpClient httpClient, string authorization)
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
            }
        }
    }
}
