using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebhookProcessor.Constants;

namespace WebhookProcessor
{
    /// <summary>
    /// This class has the generic Http action types.
    /// </summary>
    public static class HttpActions
    {
        // This Http Client is used to make all kind of requests
        public static HttpClient HttpClient = new HttpClient();

        #region  POST

        /// <summary>
        /// Make a HTTP Post Request and process the returned data or response if applicable with the provided method
        /// </summary>
        /// <typeparam name="TRequestBody"></typeparam>
        /// <typeparam name="TAuthInfo"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="payload"></param>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static async Task<TReturn> Post<TRequestBody, TAuthInfo, TReturn>(TRequestBody payload, string url, TAuthInfo info, Func<HttpResponseMessage, Task<TReturn>> processResponseHandler = null, bool retry = false)
        {
            Func<Task<TReturn>> function = async () =>
            {
                var client = new HttpClientHelper<TAuthInfo>(info, HttpClient);
                return await client.MakeJsonPostRequest(url, payload, processResponseHandler);
            };
            return retry ? await Helpers.RetryOnFail(function) : await function(); 
        }

        /// <summary>
        /// Make a HTTP Post Request without any authentication headers
        /// </summary>
        /// <typeparam name="TRequestBody"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="payload"></param>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static async Task<TReturn> Post<TRequestBody, TReturn>(TRequestBody payload, string url, Func<HttpResponseMessage, Task<TReturn>> processResponseHandler = null, bool retry = false)
        {
            Func<Task<TReturn>> function = async () =>
            {
                var info = new AuthorizationInfo(authType: AuthorizationType.NoAuth);
                var client = new HttpClientHelper<AuthorizationInfo>(info, HttpClient);
                return await client.MakeJsonPostRequest(url, payload, processResponseHandler);
            };

            return retry ? await Helpers.RetryOnFail(function) : await function();

        }       
        #endregion

        #region  PUT
        public static async Task<bool> Put<T, TAuthInfo>(T payload, string url, TAuthInfo info)
        {
            var client = new HttpClientHelper<TAuthInfo>(info,HttpClient);
            return await client.MakePutRequest(url, payload);
        }
        #endregion

        #region  GET
        /// <summary>
        /// Make a Get Request with provided Authentication Information
        /// </summary>
        /// <typeparam name="TResponse">Http Response</typeparam>
        /// <typeparam name="TReturn">Delegate Output</typeparam>
        /// <param name="queryString"></param>
        /// <param name="url"></param>
        /// <param name="processResponseHandler">Delegate which accepts http response</param>
        /// <returns></returns>
        public static async Task<TReturn> Get<TAuthInfo, TResponse,TReturn>(string queryString, string url, TAuthInfo info, Func<TResponse, Task<TReturn>> processResponseHandler = null, bool retry = false)
        {

            Func<Task<TReturn>> function = async () =>
            {
                var client = new HttpClientHelper<TAuthInfo>(info, HttpClient);
                return await client.MakeGetRequest(url, queryString, processResponseHandler);
            };

            return retry ? await Helpers.RetryOnFail(function) : await function();
          
        }
        
        /// <summary>
        /// Make a Get Request with the no authentication headers.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="queryString"></param>
        /// <param name="url"></param>
        /// <param name="processResponseHandler">Delegate which accepts http response</param>
        /// <returns></returns>
        public static async Task<TReturn> Get<TResponse, TReturn>(string queryString, string url, Func<TResponse, Task<TReturn>> processResponseHandler = null, bool retry = false)
        {
            Func<Task<TReturn>> function = async () =>
            {
                AuthorizationInfo info = new AuthorizationInfo(authType: AuthorizationType.NoAuth);
                var client = new HttpClientHelper<AuthorizationInfo>(info, HttpClient);
                return await client.MakeGetRequest(url, queryString, processResponseHandler);
            };

            return retry ? await Helpers.RetryOnFail(function) : await function();
           
        }

        /// <summary>
        /// Make a Get Request with provided Authentication Information
        /// </summary>
        /// <typeparam name="TAuthInfo">It can be (AuthorizationInfo or Simple String with 'Auth Type : Value' format)</typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="queryString"></param>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static async Task<TReturn> Get<TAuthInfo, TReturn>(string queryString, string url, TAuthInfo info, bool retry = false)
        {
            Func<Task<TReturn>> function = async () =>
            {
                var client = new HttpClientHelper<TAuthInfo>(info, HttpClient);
                return await client.MakeGetRequest<TReturn>(url, queryString);
            };

            return retry ? await Helpers.RetryOnFail(function) : await function();
        }

        /// <summary>
        /// Make a Get Request with the no authentication headers.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="queryString"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<TReturn> Get<TReturn>(string queryString, string url, bool retry = false)
        {
            Func<Task<TReturn>> function = async () =>
            {
                AuthorizationInfo info = new AuthorizationInfo(authType: AuthorizationType.NoAuth);
                var client = new HttpClientHelper<AuthorizationInfo>(info, HttpClient);
                return await client.MakeGetRequest<TReturn>(url, queryString);
            };

            return retry ? await Helpers.RetryOnFail(function) : await function();

            
        }
        #endregion

        #region  DELETE
        /// <summary>
        /// Make a Delete Request with provided Authentication Information
        /// </summary>
        /// <typeparam name="TAuthInfo">It can be (AuthorizationInfo or Simple String with 'Auth Type : Value' format)</typeparam>
        /// <param name="queryParams"></param>
        /// <param name="url"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static async Task<bool> Delete<TAuthInfo>(string queryParams, string url, TAuthInfo info, bool retry = false) 
        {
            Func<Task<bool>> function = async () =>
            {
                var client = new HttpClientHelper<TAuthInfo>(info, HttpClient);
                return await client.MakeDeleteRequest(url, queryParams);
            };

            return retry ? await Helpers.RetryOnFail(function) : await function();
            
        }
        /// <summary>
        /// Make a Delete Request without any authentication headers.
        /// </summary>
        /// <param name="queryParams"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<bool> Delete(string queryParams, string url, bool retry = false)
        {

            Func<Task<bool>> function = async () =>
            {

                AuthorizationInfo info = new AuthorizationInfo(authType: AuthorizationType.NoAuth);
                var client = new HttpClientHelper<AuthorizationInfo>(info, HttpClient);
                return await client.MakeDeleteRequest(url, queryParams);
            };

            return retry ? await Helpers.RetryOnFail(function) : await function();

        }
        #endregion

        // 
    }
}
