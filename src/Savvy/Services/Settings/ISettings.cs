using System;

namespace Savvy.Services.Settings
{
    public interface ISettings
    {
        string DropboxClientId { get; }
        string DropboxClientSecret { get; }
        string DropboxRedirectUrl { get; }
    }
}