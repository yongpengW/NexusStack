using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace NexusStack.Infrastructure.Client
{
    public class HttpRequestClient
    {
        public static string? Get(string url, List<KeyValuePair<string, string>> dataParameter = null, string authorization = "")
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())//http对象
                {
                    //表头参数
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (dataParameter != null && dataParameter.Count > 0)
                    {
                        url += (url.Contains("?") ? "&" : "?") + string.Join("&", dataParameter.ConvertAll<string>(x => { return x.Key + "=" + x.Value; }));
                    }

                    //jwt
                    if (!string.IsNullOrWhiteSpace(authorization))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                    }
                    //请求
                    HttpResponseMessage response = httpClient.GetAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Task<string> result = response.Content.ReadAsStringAsync();
                        if (result != null)
                        {
                            return result.Result;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string? Post(string url, object dataJson, string authorization = "")
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())//http对象
                {
                    //表头参数
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //jwt
                    if (!string.IsNullOrWhiteSpace(authorization))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                    }
                    //转为链接需要的格式
                    var data = JsonConvert.SerializeObject(dataJson);
                    HttpContent httpContent = new StringContent(data, Encoding.UTF8, "application/json");
                    //请求
                    HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Task<string> result = response.Content.ReadAsStringAsync();
                        if (result != null)
                        {
                            return result.Result;
                        }
                    }
                    return null;
                }
            }
            catch
            {
                throw;
            }
        }

        public static string Form(string url, List<KeyValuePair<string, string>> dataArray, string authorization)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())//http对象
                {
                    //表头参数
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
                    //jwt
                    if (!string.IsNullOrWhiteSpace(authorization))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                    }
                    //转为链接需要的格式
                    MultipartFormDataContent httpContent = new MultipartFormDataContent(string.Format("--{0}", DateTime.Now.Ticks.ToString("x")));
                    dataArray.ForEach(data =>
                    {
                        httpContent.Add(new StringContent(data.Value), data.Key);
                    });
                    //请求
                    HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Task<string> result = response.Content.ReadAsStringAsync();
                        if (result != null)
                        {
                            return result.Result;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 使用第三方平台查询实时物流信息
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string PostQueryExpressTrack(string url, Dictionary<string, string> param)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var multipartFormDataContent = new FormUrlEncodedContent(param))
                    {
                        var result = client.PostAsync(url, multipartFormDataContent).Result.Content.ReadAsStringAsync().Result;
                        return result;
                    }
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static string Put(string url, object dataJson, string authorization)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())//http对象
                {
                    //表头参数
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //jwt
                    if (!string.IsNullOrWhiteSpace(authorization))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                    }
                    //转为链接需要的格式
                    HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(dataJson), Encoding.UTF8, "application/json");
                    //请求
                    HttpResponseMessage response = httpClient.PutAsync(url, httpContent).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Task<string> result = response.Content.ReadAsStringAsync();
                        if (result != null)
                        {
                            return result.Result;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string Delete(string url, List<KeyValuePair<string, string>> dataParameter, string authorization)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())//http对象
                {
                    //表头参数
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    if (dataParameter != null && dataParameter.Count > 0)
                    {
                        url += (url.Contains("?") ? "&" : "?") + string.Join("&", dataParameter.ConvertAll<string>(x => { return x.Key + "=" + x.Value; }));
                    }

                    //jwt
                    if (!string.IsNullOrWhiteSpace(authorization))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authorization);
                    }
                    //请求
                    HttpResponseMessage response = httpClient.DeleteAsync(url).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Task<string> result = response.Content.ReadAsStringAsync();
                        if (result != null)
                        {
                            return result.Result;
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string PostDingtalkMsg(string url, string msg)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var dingtalkObj = new
                {
                    msgtype = "text",
                    text = new
                    {
                        content = msg.ToString()
                    }
                };
                return Post(url, dingtalkObj);
            }
            else
            {
                return string.Empty;
            }
        }

        public static string? PostQyWinXinMsg(string url, string msg, string mentionedMobile = "")
        {
            var msgObj = new
            {
                msgtype = "text",
                text = new
                {
                    content = msg.ToString(),
                    mentioned_mobile_list = mentionedMobile.Split(',').ToList()
                }
            };
            return Post(url, msgObj);
        }
    }
}
