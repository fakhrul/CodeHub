using System;
using UIKit;
using CodeHub.Core.ViewModels;
using Foundation;
using CodeHub.Services;
using ReactiveUI;

namespace CodeHub.ViewControllers.Source
{
    public abstract class FileSourceViewController<TViewModel> : WebViewController<TViewModel>
        where TViewModel : FileSourceViewModel
    {
        private readonly UIBarButtonItem _actionButton;
        private bool _loaded;

        protected FileSourceViewController()
        {
            _actionButton = NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Action) { Enabled = false };

            OnActivation(d =>
            {
                this.WhenAnyObservable(x => x.ViewModel.LoadCommand.IsExecuting)
                    .Subscribe(x => _actionButton.Enabled = !x).AddTo(d);
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            _actionButton.Clicked += ShowExtraMenu;

            //Stupid but I can't put this in the ViewDidLoad...
            if (!_loaded)
            {
                ViewModel.LoadCommand.Execute(null);
                _loaded = true;
            }

            Title = ViewModel.Title;
        }

        public override void ViewDidDisappear(bool animated)
        {
            _actionButton.Clicked -= ShowExtraMenu;
            base.ViewDidDisappear(animated);
        }

        protected virtual UIActionSheet CreateActionSheet(string title)
        {
            var sheet = new UIActionSheet();
            sheet.Dismissed += (sender, e) => sheet.Dispose();
            return sheet;
        }

        private void ShowExtraMenu(object o, EventArgs arg)
        {
            var sheet = CreateActionSheet(Title);
            var vm = ViewModel;
            var openButton = !string.IsNullOrEmpty(ViewModel.FilePath) ? sheet.AddButton("Open In") : -1;
            var shareButton = !string.IsNullOrEmpty(ViewModel.HtmlUrl) ? sheet.AddButton("Share") : -1;
            var showButton = ViewModel.GoToHtmlUrlCommand.CanExecute(null) ? sheet.AddButton("Show in GitHub") : -1;
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.Dismissed += (s, e) => BeginInvokeOnMainThread(() => {
                try
                {
                    if (e.ButtonIndex == openButton)
                    {
                        var ctrl = new UIDocumentInteractionController();
                        ctrl.Url = NSUrl.FromFilename(ViewModel.FilePath);
                        ctrl.PresentOpenInMenu(NavigationItem.RightBarButtonItem, true);
                    }
                    else if (e.ButtonIndex == shareButton)
                    {
                        AlertDialogService.ShareUrl(ViewModel?.HtmlUrl, o as UIBarButtonItem);
                    }
                    else if (e.ButtonIndex == showButton)
                    {
                        vm.GoToHtmlUrlCommand.Execute(null);
                    }
                }
                catch
                {
                }
            });

            sheet.ShowFrom(NavigationItem.RightBarButtonItem, true);
        }
    }
}

