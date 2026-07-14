namespace StreamChatInator
{
    public static class Util
    {
        public static async Task RetryAsync(Func<bool>method,int tries, int delay=500, bool useExponentialBackoff=false, int maxDelay = 5000)
        {
            while(tries-- > 0)
            {
                if (method())
                {
                    return;
                }
                await Task.Delay(delay);
                if (useExponentialBackoff)
                {
                    delay *= 2;
                    if(delay > maxDelay)
                    {
                        delay = maxDelay;
                        useExponentialBackoff = false;
                    }
                }
            }
            throw new TimeoutException("Failed calling method even with retries");
        }
    }
}
