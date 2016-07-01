using System;
using CodeHub.Core.ViewModels.Accounts;
using CodeHub.Utilities;
using Foundation;
using WebKit;
using CodeHub.Core.Services;
using CodeHub.Services;
using System.Reactive.Linq;
using CodeHub.Views;
using Splat;
using ReactiveUI;

namespace CodeHub.ViewControllers.Accounts
{
    public class LoginViewController : BaseWebViewController<LoginViewModel>
    {
        private static readonly string HasSeenWelcomeKey = "HAS_SEEN_OAUTH_INFO";

        private static readonly string OAuthWelcome = 
            "In the following screen you will be prompted for your GitHub credentials. This is done through GitHub's OAuth portal, " +
            "the recommended way to authenticate.\n\nCodeHub does not save your password. Instead, only the OAuth " + 
            "token is saved on the device which you may revoke at any time.";

        public LoginViewController() 
        {
            ViewModel = new LoginViewModel();
            OnActivation(d => this.WhenAnyValue(x => x.ViewModel.IsLoggingIn).SubscribeStatus("Logging in...").AddTo(d));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            LoadRequest();

            bool hasSeenWelcome = false;
            var defaultValueService = Locator.Current.GetService<IDefaultValueService>();
            defaultValueService.TryGet(HasSeenWelcomeKey, out hasSeenWelcome);

            if (!hasSeenWelcome)
            {
                Appeared
                    .Take(1)
                    .Subscribe(_ =>
                    {
                        defaultValueService.Set(HasSeenWelcomeKey, true);
                        BlurredAlertView.Display(OAuthWelcome);
                    });
            }
        }

        public override bool ShouldStartLoad(WKWebView webView, WKNavigationAction navigationAction)
        {
            try
            {
                //We're being redirected to our redirect URL so we must have been successful
                if (navigationAction.Request.Url.Host == "dillonbuchanan.com")
                {
                    var code = navigationAction.Request.Url.Query.Split('=')[1];
                    ViewModel.Login(code).ToBackground();
                    return false;
                }
    
                if (navigationAction.Request.Url.AbsoluteString.StartsWith("https://github.com/join"))
                {
                    Locator.Current.GetService<IAlertDialogService>().Alert("Error", "Sorry, due to restrictions, creating GitHub accounts cannot be done in CodeHub.");
                    return false;
                }

                return base.ShouldStartLoad(webView, navigationAction);
            }
            catch 
            {
                Locator.Current.GetService<IAlertDialogService>().Alert("Error Logging in!", "CodeHub is unable to login you in due to an unexpected error. Please try again.");
                return false;
            }
        }

        public override void OnLoadError(NSError e)
        {
            base.OnLoadError(e);

            //Frame interrupted error
            if (e.Code == 102 || e.Code == -999) return;
            AlertDialogService.ShowAlert("Error", "Unable to communicate with GitHub. " + e.LocalizedDescription);
        }

        private void LoadRequest()
        {
            //Remove all cookies & cache
            WKWebsiteDataStore.DefaultDataStore.RemoveDataOfTypes(WKWebsiteDataStore.AllWebsiteDataTypes, NSDate.FromTimeIntervalSince1970(0), 
                () => Web.LoadRequest(new NSUrlRequest(new NSUrl(ViewModel.LoginUrl))));
        }
    }
}

