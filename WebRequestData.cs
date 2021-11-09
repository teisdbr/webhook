using System;
using System.Collections.Generic;
using System.Text;

namespace WebhookProcessor
{
    public class WebRequestData<T>
    {
        public string queryParams { get; set; }

        public T payload { get; set; }

        public WebRequestData(string queryParams, T payload)
        {
            this.queryParams = queryParams;
            this.payload = payload;
        }

        public WebRequestData()
        {

        }
    }
}

