using System;
using System.Threading.Tasks;
using ReactiveUI;
using CodeHub.Core.Messages;
using CodeHub.Core.Services;

namespace CodeHub.Core.ViewModels.Accounts
{
    public class LoginViewModel : BaseViewModel
    {
        public static readonly string RedirectUri = "http://dillonbuchanan.com/";
        private readonly ILoginService _loginFactory;

        private bool _isLoggingIn;
        public bool IsLoggingIn
        {
            get { return _isLoggingIn; }
            set { this.RaiseAndSetIfChanged(ref _isLoggingIn, value); }
        }

        public string LoginUrl
        {
            get
            {
                var web = WebDomain.TrimEnd('/');
                return string.Format(
                    web + "/login/oauth/authorize?client_id={0}&redirect_uri={1}&scope={2}", 
                    Secrets.GithubOAuthId, 
                    Uri.EscapeDataString(LoginViewModel.RedirectUri),
                    Uri.EscapeDataString("user:follow,repo,notifications,gist,read:org"));
            }
        }

        public string WebDomain { get; set; }

        public LoginViewModel()
        {
            _loginFactory = GetService<ILoginService>();
            Title = "Login";

            WebDomain = GitHubSharp.Client.AccessTokenUri;
        }

        public async Task Login(string code)
        {
            LoginData loginData = null;

            try
            {
                IsLoggingIn = true;
                loginData = await _loginFactory.LoginWithToken(Secrets.GithubOAuthId, Secrets.GithubOAuthSecret, 
                    code, RedirectUri, WebDomain, GitHubSharp.Client.DefaultApi);
            }
            catch (Exception e)
            {
                DisplayAlert(e.Message);
                return;
            }
            finally
            {
                IsLoggingIn = false;
            }

            this.GetApplication().ActivateUser(loginData.Account, loginData.Client);
            MessageBus.Current.SendMessage(new LogoutMessage());
        }
    }
}

