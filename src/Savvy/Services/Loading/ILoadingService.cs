using System;

namespace Savvy.Services.Loading
{
    public interface ILoadingService
    {
        IDisposable Show(string message);
    }
}