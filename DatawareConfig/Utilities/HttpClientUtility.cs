using Consiss.ConfigDataWare.CrossCutting.Configurations;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Consiss.ConfigDataWare.CrossCutting.Utilities
{
    public static class HttpClientUtility
    {
        public static async Task<(T, HttpResponseMessage)> PostAsync<T>(string url, object data, string? token = null) where T : class, new()
        {
            try
            {
                var retryPolity = GetRetryPolity();
                return await retryPolity.ExecuteAsync(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromMinutes(30);
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    string content = JsonConvert.SerializeObject(data);
                    var buffer = Encoding.UTF8.GetBytes(content);
                    var byteContent = new ByteArrayContent(buffer);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.PostAsync(url, byteContent).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return (JsonConvert.DeserializeObject<T>(result), response);
                        default:
                            return (new T(), response);
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    //ErrorLogHelper.AddExcFileTxt(ex, url + " -> POST - " + $"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}");
                    throw new Exception($"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //ErrorLogHelper.AddExcFileTxt(ex, url + " -> POST");
                //if (ex.InnerException != null) ErrorLogHelper.AddExcFileTxt(ex.InnerException, url + " -> POST");
                throw ex;
            }
        }

        public static async Task<(T, HttpResponseMessage)> PostAsyncAcendes<T>(string url, object data, string? token = null) where T : class, new()
        {
            try
            {
                var retryPolity = GetRetryPolity();
                return await retryPolity.ExecuteAsync(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromMinutes(30);
                    //if (!string.IsNullOrEmpty(token))
                    //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    string content = JsonConvert.SerializeObject(data);
                    var buffer = Encoding.UTF8.GetBytes(content);
                    var byteContent = new ByteArrayContent(buffer);
                    byteContent.Headers.Add("X-Openerp-Session-Id", token);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.PostAsync(url, byteContent).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return (JsonConvert.DeserializeObject<T>(result), response);
                        case HttpStatusCode.BadRequest:
                            return (JsonConvert.DeserializeObject<T>(result), response);
                        default:
                            return (new T(), response);
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    //ErrorLogHelper.AddExcFileTxt(ex, url + " -> POST - " + $"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}");
                    throw new Exception($"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                //ErrorLogHelper.AddExcFileTxt(ex, url + " -> POST");
                //if (ex.InnerException != null) ErrorLogHelper.AddExcFileTxt(ex.InnerException, url + " -> POST");
                throw ex;
            }
        }

        public static async Task<(T, HttpResponseMessage)> PutAsync<T>(string url, object data, string token = null) where T : class, new()
        {
            try
            {
                var retryPolity = GetRetryPolity();
                return await retryPolity.ExecuteAsync(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(75);
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    string content = JsonConvert.SerializeObject(data);
                    var buffer = Encoding.UTF8.GetBytes(content);
                    var byteContent = new ByteArrayContent(buffer);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.PutAsync(url, byteContent).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return (JsonConvert.DeserializeObject<T>(result), response);
                        default:
                            //ErrorLogHelper.AddExcFileTxt(new Exception(result), url + " -> PUT -> " + content);
                            return (new T(), response);
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    throw new Exception($"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}", ex);
                throw;
            }
            catch (Exception ex)
            {
                //ErrorLogHelper.AddExcFileTxt(ex, url + " -> PUT");
                //if (ex.InnerException != null) ErrorLogHelper.AddExcFileTxt(ex.InnerException, url + " -> POST");
                throw ex;
            }
        }
        public static async Task<(T, HttpResponseMessage)> GetAsync<T>(string url, string token = null) where T : new() //class, new()
        {
            try
            {
                var retryPolity = GetRetryPolity();
                return await retryPolity.ExecuteAsync(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromHours(2);
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.GetAsync(url).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return (JsonConvert.DeserializeObject<T>(result), response);
                        default:
                            //ErrorLogHelper.AddExcFileTxt(new Exception(result), url + " -> GET<T>");
                            return (new T(), response);
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    throw new Exception($"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}", ex);
                throw;
            }
            catch (Exception ex)
            {
                //ErrorLogHelper.AddExcFileTxt(ex, url + " -> GET<T>");
                //if (ex.InnerException != null) ErrorLogHelper.AddExcFileTxt(ex.InnerException, url + " -> POST");
                throw ex;
            }
        }
        public static async Task<(string, HttpResponseMessage)> GetAsync(string url, string token = null)
        {
            try
            {
                var retryPolity = GetRetryPolity();
                return await retryPolity.ExecuteAsync(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(75);
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.GetAsync(url).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return (result, response);
                        default:
                            //ErrorLogHelper.AddExcFileTxt(new Exception(result), url + " -> GET<string>");
                            return (string.Empty, response);
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    throw new Exception($"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}", ex);
                throw;
            }
            catch (Exception ex)
            {
                //ErrorLogHelper.AddExcFileTxt(ex, url + " -> GET<string>");
                //if (ex.InnerException != null) ErrorLogHelper.AddExcFileTxt(ex.InnerException, url + " -> POST");
                throw ex;
            }
        }
        public static async Task<(int, HttpResponseMessage)> DeleteAsync(string url, string token = null)
        {
            try
            {
                var retryPolity = GetRetryPolity();

                return await retryPolity.ExecuteAsync(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(75);
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.DeleteAsync(url).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return (JsonConvert.DeserializeObject<int>(result), response);
                        default:
                            //ErrorLogHelper.AddExcFileTxt(new Exception(result), url + " -> DELETE");
                            return (int.MinValue, response);
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    throw new Exception($"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}", ex);
                throw;
            }
            catch (Exception ex)
            {
                //ErrorLogHelper.AddExcFileTxt(ex, url + " -> DELETE");
                //if (ex.InnerException != null) ErrorLogHelper.AddExcFileTxt(ex.InnerException, url + " -> POST");
                throw ex;
            }
        }

        public static async Task<(T, HttpResponseMessage)> DeleteAsyncObject<T>(string url, string token = null) where T : class, new()
        {
            try
            {
                var retryPolity = GetRetryPolity();

                return await retryPolity.ExecuteAsync(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(75);
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await client.DeleteAsync(url).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return (JsonConvert.DeserializeObject<T>(result), response);
                            case HttpStatusCode.BadRequest:
                            return (JsonConvert.DeserializeObject<T>(result), response);
                        default:
                            //ErrorLogHelper.AddExcFileTxt(new Exception(result), url + " -> DELETE");
                            return (new T(), response);
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    throw new Exception($"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}", ex);
                throw;
            }
            catch (Exception ex)
            {
                //ErrorLogHelper.AddExcFileTxt(ex, url + " -> DELETE");
                //if (ex.InnerException != null) ErrorLogHelper.AddExcFileTxt(ex.InnerException, url + " -> POST");
                throw ex;
            }
        }

        public static async Task<(string, HttpResponseMessage)> PostAsyncString<T>(string url, object data, string? token = null, string? typeApplication = null) where T : class, new()
        {
            try
            {

                if (!string.IsNullOrEmpty(typeApplication))
                {
                    typeApplication = "application/json";
                }

                var retryPolity = GetRetryPolity();
                return await retryPolity.ExecuteAsync(async () =>
                {
                    HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromMinutes(30);
                    if (!string.IsNullOrEmpty(token))
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    string content = JsonConvert.SerializeObject(data);
                    var buffer = Encoding.UTF8.GetBytes(content);
                    var byteContent = new ByteArrayContent(buffer);
                    byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.PostAsync(url, byteContent).ConfigureAwait(false);
                    string result = await response.Content.ReadAsStringAsync();
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return ((string)(result), response);
                        default:
                            return ((string)("NODATA"), response);
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                    throw new Exception($"response :{new StreamReader(ex.Response.GetResponseStream()).ReadToEnd()}", ex);
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static AsyncRetryPolicy GetRetryPolity()
        {
            try
            {
                var pollyConfiguration = new PollyConfiguration();
                var maxTrys = pollyConfiguration.MaxTrys;
                var timeToWait = TimeSpan.FromSeconds(pollyConfiguration.TimeDelay);
                var retryPolity = Policy.Handle<Exception>().WaitAndRetryAsync(
                    maxTrys,
                    i => timeToWait
                );
                return retryPolity;
            }
            catch (Exception ex)
            {
                //ErrorLogHelper.AddExcFileTxt(ex, "method: GetRetryPolity()");
                throw;
            }
        }
    }
}
