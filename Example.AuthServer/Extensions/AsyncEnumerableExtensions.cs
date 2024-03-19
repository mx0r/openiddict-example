namespace Example.AuthServer.Extensions;

public static class AsyncEnumerableExtensions
{
    public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return LocalExecuteAsync();

        // local function to execute the async operation
        
        async Task<List<T>> LocalExecuteAsync()
        {
            var list = new List<T>();
            await foreach (var element in source)
            {
                list.Add(element);
            }

            return list;
        }
    }
}
