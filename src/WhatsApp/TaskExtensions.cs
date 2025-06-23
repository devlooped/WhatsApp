using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

static class TaskExtensions
{
    public static ConfiguredTaskAwaitable Ignore(this Task task)
        => task.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

    public static async Task Ignore<T>(this Task<T> task)
    {
        if (!task.IsCompleted || task.IsFaulted)
        {
            try
            {
                await task.ConfigureAwait(ConfigureAwaitOptions.None);
            }
            catch (Exception)
            {
            }
        }
    }
}