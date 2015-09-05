namespace Savvy.Services.DropboxAuthentication
{
    public class DropboxAuth
    {
        public DropboxAuth(string accessCode, string userId)
        {
            this.AccessCode = accessCode;
            this.UserId = userId;
        }

        public string AccessCode { get; }
        public string UserId { get; } 
    }
}