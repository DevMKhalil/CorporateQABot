using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CorporateQABot.Core
{
    public static class OpenAiThrottler
    {
        private static readonly SemaphoreSlim Gate = new(1, 1);

        public static async Task<T> RunAsync<T>(Func<Task<T>> action, int maxRetries = 3)
        {
            await Gate.WaitAsync();
            try
            {
                for (int attempt = 0; ; attempt++)
                {
                    try { return await action(); }
                    catch (tryAGI.OpenAI.ApiException ex) when ((int)ex.StatusCode == 429 && attempt < maxRetries)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(21));
                    }
                }
            }
            finally { Gate.Release(); }
        }

        // يدعم IAsyncEnumerable بدون yield داخل catch
        public static async IAsyncEnumerable<T> StreamAsync<T>(
            Func<IAsyncEnumerable<T>> streamFactory,
            int maxRetries = 3)
        {
            await Gate.WaitAsync();
            try
            {
                for (int attempt = 0; ; attempt++)
                {
                    await using var e = streamFactory().GetAsyncEnumerator();
                    bool needRetry = false;

                    while (true)
                    {
                        bool moved;
                        try
                        {
                            moved = await e.MoveNextAsync();
                        }
                        catch (tryAGI.OpenAI.ApiException ex) when ((int)ex.StatusCode == 429 && attempt < maxRetries)
                        { needRetry = true; break; }
                        catch (HttpRequestException) when (attempt < maxRetries) // أحيانًا الSDK يلفّ 429 كده
                        { needRetry = true; break; }

                        if (!moved) yield break;
                        yield return e.Current;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(21)); // backoff للـFree tier
                    if (!needRetry) yield break;
                    // otherwise loop ويعيد المحاولة
                }
            }
            finally { Gate.Release(); }
        }

    }
}
