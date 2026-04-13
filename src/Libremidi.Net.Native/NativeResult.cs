namespace Libremidi.Net.Native;

using System;

internal static class NativeResult
{
    internal static void ThrowIfFailed(int result, string operation)
    {
        if (result == 0)
        {
            return;
        }

        throw new InvalidOperationException($"{operation} failed with native error code {result}.");
    }
}

