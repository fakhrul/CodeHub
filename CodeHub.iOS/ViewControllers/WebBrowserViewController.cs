using System;
using CodeHub.Core.ViewModels;
using CodeHub.Services;
using Foundation;
using UIKit;

namespace CodeHub.ViewControllers
{
    public class WebBrowserViewController : BaseWebViewController<WebBrowserViewModel>
    {
        public WebBrowserViewController()
        {
        }

        public WebBrowserViewController(string url)
        {
            ViewModel = new WebBrowserViewModel(url);
        }

        public static UIViewController CreateWithNavbar(string url)
        {
            var vc = new WebBrowserViewController();
            vc.ViewModel = new WebBrowserViewModel(url);
            return new UINavigationController(vc);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            try
            {
                if (!string.IsNullOrEmpty(ViewModel.Url))
                    Web.LoadRequest(new NSUrlRequest(new NSUrl(ViewModel.Url)));
            }
            catch (Exception e)
            {
                AlertDialogService.ShowAlert("Unable to process request!", e.Message);
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            GetTitle();
        }

        public override void OnLoadFinished(WebKit.WKWebView webView, WebKit.WKNavigation navigation)
        {
            base.OnLoadFinished(webView, navigation);
            GetTitle();
        }

        private void GetTitle()
        {
            Web.EvaluateJavaScript("document.title", (o, _) =>
            {
                ViewModel.Title = o as NSString;
            });
        }
    }
}
