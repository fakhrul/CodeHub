using System.Net;

namespace CodeHub.Core
{
    public static class Application
    {
        public static void Initialize()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }
    }
}