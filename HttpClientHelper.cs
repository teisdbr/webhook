using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TeUtil.Extensions;
using WebhookProcessor.Constants;


namespace WebhookProcessor
{
    public class HttpClientHelper<T>
    {
        /// <summary>
        /// This will setup the HttpClient with the provided Authorization Options. baseurl is Optional
        /// </summary>
        public HttpClientHelper(T authorizationInfo, HttpClient client)
        {
            RequestMessage = new HttpRequestMessage();

            RequestMessage.Headers.ExpectContinue = false;

            Client = client;
           
            //Client.DefaultRequestHeaders.Accept.Clear();
            //Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // Set Auth Options
            if (authorizationInfo is AuthorizationInfo)
            {
                SetAuthorization(authorizationInfo as AuthorizationInfo);
            }
            else if (authorizationInfo is string)
            {
                SetAuthorization(authorizationInfo as string);
            }
            else
            {
                throw new Exception($"Invalid Authorization Parameter Type: {authorizationInfo.GetType()}");
            }
        }

        public HttpClientHelper()
        {
            RequestMessage = new HttpRequestMessage();
        }


        private  HttpClient Client { get; set; }

        public HttpRequestMessage RequestMessage { get; set; }

             
        /// <summary>
        /// This method will make a Http Get Request to the given URL, and calls the response handler when response is received.
        /// </summary>
        /// <typeparam name="T">return type of the payload </typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="apiUrl"></param>
        /// <param name="queryString"></param>
        /// <param name="processResponseHandler">This will be called once the response is received</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TReturn> MakeGetRequest<TReturn, TResponse>(string apiUrl,
            string queryString,
            Func<TResponse, Task<TReturn>> processResponseHandler,
            CancellationToken cancellationToken = default(CancellationToken))
        {

            // The payload will be a string that is a queryString
            // i.e: fromDate=1/1/2020, citationId=2, citationNumber=345345 
            if (!queryString.IsNullBlankOrEmpty())
            {
                if(apiUrl.Contains("?"))
                    apiUrl = String.Concat(apiUrl, "&", queryString);
                else
                {
                    apiUrl = String.Concat(apiUrl, "?", queryString);
                }
            }
                

            var result = default(TResponse);

            RequestMessage.RequestUri = new Uri(apiUrl);
            RequestMessage.Method = HttpMethod.Get;

            var response = await Client.SendAsync(RequestMessage, cancellationToken).ConfigureAwait(false);
            
            if (response.IsSuccessStatusCode)
            {
                //TODO: Get rid of the Deserializer in the line 84 and let the Delegate take the raw response and Process.
                var responseContent = await response.Content.ReadAsStringAsync();
              
                result = JsonConvert.DeserializeObject<TResponse>(responseContent);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                response.Content?.Dispose();
                throw new HttpRequestException($"{response.StatusCode}:{content} , URL: {apiUrl} , VERB: {RequestMessage.Method}");

            }

            return await processResponseHandler(result);
        }


        /// <summary>
        /// This method makes the HTTP get request to the provided URL.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="apiUrl"></param>
        /// <param name="queryString"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<TResponse> MakeGetRequest<TResponse>(string apiUrl,
            string queryString,
            CancellationToken cancellationToken = default(CancellationToken))
        {

            // The payload will be a string that is a queryString
            // i.e: fromDate=1/1/2020, citationId=2, citationNumber=345345 
            apiUrl = BuildQueryParam(queryString , apiUrl);


            var result = default(TResponse);

            RequestMessage.RequestUri = new Uri(apiUrl);
            RequestMessage.Method = HttpMethod.Get;

            var response = await Client.SendAsync(RequestMessage, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                await response.Content.ReadAsStringAsync().ContinueWith(
                      x => { result = JsonConvert.DeserializeObject<TResponse>(x?.Result); }, cancellationToken);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                response.Content?.Dispose();
                throw new HttpRequestException($"{response.StatusCode}:{content} , URL: {apiUrl} , VERB: {RequestMessage.Method}");
            }

            return result;
        }






        /// <summary>
        /// This Method makes a JSON Http Post Request to the provided URL and 
        /// return the response. To process the response, a processResponseHandler should be passed
        /// If failed HttpRequestException is thrown
        /// </summary>
        /// <typeparam name="TRequestBody"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="apiUrl"></param>
        /// <param name="payload"></param>
        /// <param name="processResponseHandler"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        /// <param name="cancellationToken"></param>
        public async Task<TResponse> MakeJsonPostRequest<TRequestBody, TResponse>(string apiUrl,
            TRequestBody payload, Func<HttpResponseMessage, Task<TResponse>> processResponseHandler = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var jsonString = JsonConvert.SerializeObject(payload);

            var buffer = System.Text.Encoding.UTF8.GetBytes(jsonString);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                // set the Request url and Method
                RequestMessage.RequestUri = new Uri(apiUrl);
                RequestMessage.Method = HttpMethod.Post;
                RequestMessage.Content = byteContent;

                var response = await Client.SendAsync(RequestMessage, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                response.Content?.Dispose();
                throw new HttpRequestException($"{response.StatusCode}:{content} , URL: {apiUrl} , VERB: {RequestMessage.Method}");

            }
            else if (processResponseHandler != null)
            {
                return await processResponseHandler(response);
            }

            return default(TResponse);
        }

        /// <summary>
        /// This method makes a Http PUT request, to the provided URL. Returns True if successful.
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="apiUrl"></param>
        /// <param name="getPayload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> MakePutRequest<TIn>(string apiUrl,
            TIn payload, CancellationToken cancellationToken = default(CancellationToken))
        {

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            RequestMessage.RequestUri = new Uri(apiUrl);
            RequestMessage.Method = HttpMethod.Put;
            RequestMessage.Content = byteContent;

            var response = await Client.SendAsync(RequestMessage, cancellationToken).ConfigureAwait(false);
           

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                response.Content?.Dispose();
                throw new HttpRequestException($"{response.StatusCode}:{content} , URL: {apiUrl} , VERB: {RequestMessage.Method}");

            }

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// This method makes a Http PUT request, to the provided URL.
        /// </summary>
        /// <typeparam name="T">Type of the payload</typeparam>
        /// <param name="apiUrl"></param>
        /// <param name="getPayload"></param>
        /// <param name="cancellationToken"></param>
        /// /// <param name="TResponse"></param>
        /// <returns></returns>
        public async Task<TResponse> MakePutRequest<Tin, TResponse>(string apiUrl,
            Func<string, string, Tin> getPayload, CancellationToken cancellationToken = default(CancellationToken))
        {

            var buffer = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(getPayload(apiUrl, "Put")));
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var result = default(TResponse);

            RequestMessage.RequestUri = new Uri(apiUrl);
            RequestMessage.Method = HttpMethod.Put;
            RequestMessage.Content = byteContent;

            var response = await Client.SendAsync(RequestMessage, cancellationToken).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                await response.Content.ReadAsStringAsync().ContinueWith(
                    x => { result = JsonConvert.DeserializeObject<TResponse>(x?.Result); }, cancellationToken);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                response.Content?.Dispose();
                throw new HttpRequestException($"{response.StatusCode}:{content} , URL: {apiUrl} , VERB: {RequestMessage.Method}");
                
            }

            return result;
        }



        // TODO: Create a Delete Request
        /// <summary>
        /// This method makes a Http Delete request, to the provided URL.
        /// </summary>
        /// <typeparam name="T">Type of the payload</typeparam>
        /// <param name="apiUrl"></param>
        /// <param name="getPayload"></param>
        /// <param name="cancellationToken"></param>
        /// /// <param name="TResponse"></param>
        /// <returns></returns>
        public async Task<Boolean> MakeDeleteRequest(string apiUrl,
            string queryParams, CancellationToken cancellationToken = default(CancellationToken))
        {

            // The payload will be a string that is a queryString
            // i.e: fromDate=1/1/2020, citationId=2, citationNumber=345345 
            apiUrl = BuildQueryParam(queryParams as string, apiUrl);

            RequestMessage.RequestUri = new Uri(apiUrl);
            RequestMessage.Method = HttpMethod.Delete;
         
            var response = await Client.SendAsync(RequestMessage, cancellationToken).ConfigureAwait(false);


            if (response.IsSuccessStatusCode) return response.IsSuccessStatusCode;

            var content = await response.Content.ReadAsStringAsync();
            response.Content?.Dispose();
            throw new HttpRequestException($"{response.StatusCode}:{content} , URL: {apiUrl} , VERB: {RequestMessage.Method}");
            
        }

        /// <summary>
        /// Used to setup  authentication headers for the request
        /// </summary>
        /// <param name="authorizationInfo">The Authorization header information.
        private void SetAuthorization(AuthorizationInfo authorizationInfo)
        {

            //Client.DefaultRequestHeaders.Accept.Clear();
            //Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            RequestMessage.Headers.Add("Accept", "application/json");

            switch (authorizationInfo.AuthType)
            {
                case AuthorizationType.NoAuth:
                    // 
                    break;
                case AuthorizationType.Basic:
                    var authenticationString = $"{authorizationInfo.UserName}:{authorizationInfo.Password}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));
                    RequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                    break;
                case AuthorizationType.Bearer:
                    RequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationInfo.AuthToken);
                    break;
                case AuthorizationType.XAPIKey:
                    //TODO: Check for AttachAPIKeyOptions and add APIKey to the Url if needed.
                    RequestMessage.Headers.Add(authorizationInfo.key, authorizationInfo.Value);
                    break;
                default:
                    break;
            }
        }

        private void SetAuthorization(string authorizationHeader)
        {

            //Client.DefaultRequestHeaders.Accept.Clear();
            //Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            RequestMessage.Headers.Add("Accept", "application/json");

            var authKeyValuePair = authorizationHeader.Split(':');

            var authType = Enum.Parse(typeof(AuthorizationType), authKeyValuePair[0]);
            var authValue = authKeyValuePair[1];

            switch (authType)
            {
                case AuthorizationType.NoAuth:
                    break;
                case AuthorizationType.Basic:
                    RequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                    break;
                case AuthorizationType.Bearer:
                    RequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authValue);
                    break;
                case AuthorizationType.XAPIKey:
                    //TODO: Check for AttachAPIKeyOptions and add APIKey to the Url if needed.
                    RequestMessage.Headers.Add("X-Api-Key", authValue);
                    break;
                default:
                    break;
            }
        }

        private string BuildQueryParam( string queryString , string apiUrl)
        {
            if (!queryString.IsNullBlankOrEmpty())
            {
                if (apiUrl.Contains("?"))
                    apiUrl = String.Concat(apiUrl, "&", queryString);
                else
                {
                    apiUrl = String.Concat(apiUrl, "?", queryString);
                }
            }

            return apiUrl;
        }

   

    }
}
