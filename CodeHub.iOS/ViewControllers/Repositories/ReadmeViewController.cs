using CodeHub.Core.ViewModels.Repositories;
using UIKit;
using WebKit;
using System;
using CodeHub.Services;
using System.Reactive.Linq;
using CodeHub.WebViews;
using CodeHub.ViewControllers;
using ReactiveUI;

namespace CodeHub.Views.Repositories
{
    public class ReadmeViewController : WebViewController<ReadmeViewModel>
    {
        private readonly UIBarButtonItem _actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action);

        public ReadmeViewController()
        {
            NavigationItem.RightBarButtonItem = _actionButton;

            OnActivation(d =>
            {
                _actionButton
                    .GetClickedObservable()
                    .Subscribe(ShareButtonPress)
                    .AddTo(d);
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.WhenAnyValue(x => x.ViewModel.ContentText)
                .IsNotNull()
                .Select(x => new DescriptionModel(x, (int)UIFont.PreferredSubheadline.PointSize))
                .Select(x => new MarkdownView { Model = x }.GenerateString())
                .Subscribe(LoadContent);

            ViewModel.LoadCommand.Execute(false);
        }

        public override bool ShouldStartLoad(WKWebView webView, WKNavigationAction navigationAction)
        {
            if (!navigationAction.Request.Url.AbsoluteString.StartsWith("file://", StringComparison.Ordinal))
            {
                var vc = new WebBrowserViewController(navigationAction.Request.Url.AbsoluteString);
                NavigationController.PushViewController(vc, true);
                return false;
            }

            return base.ShouldStartLoad(webView, navigationAction);
        }

        private void ShareButtonPress(UIBarButtonItem o)
        {
            var sheet = new UIActionSheet();
            var shareButton = sheet.AddButton("Share");
            var showButton = sheet.AddButton("Show in GitHub");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;

            sheet.Dismissed += (sender, e) =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    if (e.ButtonIndex == showButton)
                    {
                        var vc = new WebBrowserViewController(ViewModel.HtmlUrl);
                        NavigationController.PushViewController(vc, true);
                    }
                    else if (e.ButtonIndex == shareButton)
                        AlertDialogService.ShareUrl(ViewModel.HtmlUrl, o);
                });

                sheet.Dispose();
            };

            sheet.ShowFrom(_actionButton, true);
        }
    }
}

