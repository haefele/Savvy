using System;
using System.Threading.Tasks;

namespace Savvy.Services.DropboxAuthentication
{
    public interface IDropboxAuthenticationService
    {
        Task<DropboxAuth> LoginAndGetAccessCodeAsync();
    }
}