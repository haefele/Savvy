using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Security.Authentication.Web;
using Newtonsoft.Json.Linq;
using Savvy.Services.Settings;

namespace Savvy.Services.DropboxAuthentication
{
    public class DropboxAuthenticationService : IDropboxAuthenticationService
    {
        private readonly ISettings _settings;

        public DropboxAuthenticationService(ISettings settings)
        {
            this._settings = settings;
        }
        
        public async Task<DropboxAuth> LoginAndGetAccessCodeAsync()
        {
            var authenticateUrl = new Uri($"https://api.dropbox.com/1/oauth2/authorize?response_type=code&client_id={this._settings.DropboxClientId}&redirect_uri={Uri.EscapeDataString(this._settings.DropboxRedirectUrl)}");
            var authenticateResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, authenticateUrl, new Uri(this._settings.DropboxRedirectUrl));

            if (authenticateResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                var code = this.ExtractCode(authenticateResult.ResponseData);

                var accessTokenUrl = new Uri($"https://api.dropbox.com/1/oauth2/token?code={code}&grant_type=authorization_code&client_id={this._settings.DropboxClientId}&client_secret={this._settings.DropboxClientSecret}&redirect_uri={this._settings.DropboxRedirectUrl}");
                var accessTokenResponse = await new HttpClient().PostAsync(accessTokenUrl, null);

                if (accessTokenResponse.StatusCode == HttpStatusCode.OK)
                {
                    var content = await accessTokenResponse.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);

                    return new DropboxAuth(json.Value<string>("access_token"), json.Value<string>("uid"));
                }
            }

            return null;
        }

        private string ExtractCode(string redirectedUrl)
        {
            var url = new Uri(redirectedUrl);
            var decoder = new WwwFormUrlDecoder(url.Query);

            return decoder.GetFirstValueByName("code");
        }
    }
}