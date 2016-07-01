using System;
using CodeHub.Core.ViewModels.Gists;
using UIKit;

namespace CodeHub.ViewControllers.Gists
{
    public class UserGistsViewController : GistsViewController<UserGistsViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var button = new UIBarButtonItem(UIBarButtonSystemItem.Add);

            if (ViewModel.IsMine)
                NavigationItem.RightBarButtonItem = button;

            OnActivation(d => button.GetClickedObservable().Subscribe(_ => GoToCreateGist()).AddTo(d));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (ViewModel != null) Title = ViewModel.Title;
        }

        private void GoToCreateGist()
        {
            GistCreateViewController.Show(this);
        }
    }
}

