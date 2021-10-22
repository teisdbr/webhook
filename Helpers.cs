using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebhookProcessor
{
   public static class   Helpers
    {

        public static async Task<T> RetryOnFail<T>(Func<Task<T>> func)
        {
            int retryAttemptsLeft = 3;

            while (true)
            {
                try
                {
                    return await func();
                }
                catch (Exception ex)
                {
                    retryAttemptsLeft--;
                    if (retryAttemptsLeft == 0)
                        throw ex;
                }
            }
        }


       
    }
}
