using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Savvy.YnabApiFileSystem
{
    public class AccessCodeMessageHandler : DelegatingHandler
    {
        private readonly string _accessCode;

        public AccessCodeMessageHandler(string accessCode, HttpMessageHandler innerHandler)
            : base(innerHandler)
        {
            this._accessCode = accessCode;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this._accessCode);

            return base.SendAsync(request, cancellationToken);
        }
    }
}