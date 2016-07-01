using CodeHub.Core.ViewModels.Users;
using UIKit;
using System;
using CodeHub.DialogElements;
using ReactiveUI;

namespace CodeHub.ViewControllers.Users
{
    public class UserViewController : PrettyDialogViewController<UserViewModel>
    {
        private readonly Lazy<UIBarButtonItem> _actionButton;

        public UserViewController()
        {
            _actionButton = new Lazy<UIBarButtonItem>(() => new UIBarButtonItem(UIBarButtonSystemItem.Action, (s, e) => ShowExtraMenu()));
        }
            
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = ViewModel.Username;

            var split = new SplitButtonElement();
            var followers = split.AddButton("Followers", "-");
            var following = split.AddButton("Following", "-");

            var events = new StringElement("Events", Octicon.Rss.ToImage());
            var organizations = new StringElement("Organizations", Octicon.Organization.ToImage());
            var repos = new StringElement("Repositories", Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", Octicon.Gist.ToImage());
            Root.Add(new [] { new Section { split }, new Section { events, organizations, repos, gists } });

            OnActivation(d =>
            {
                followers.Clicked.InvokeCommand(ViewModel.GoToFollowersCommand).AddTo(d);
                following.Clicked.InvokeCommand(ViewModel.GoToFollowingCommand).AddTo(d);
                events.Clicked.InvokeCommand(ViewModel.GoToEventsCommand).AddTo(d);
                organizations.Clicked.InvokeCommand(ViewModel.GoToOrganizationsCommand).AddTo(d);
                repos.Clicked.InvokeCommand(ViewModel.GoToRepositoriesCommand).AddTo(d);
                gists.Clicked.InvokeCommand(ViewModel.GoToGistsCommand).AddTo(d);

                this.WhenAnyValue(x => x.ViewModel.User).Subscribe(x =>
                {
                    followers.Text = x?.Followers.ToString() ?? "-";
                    following.Text = x?.Following.ToString() ?? "-";
                    HeaderView.SubText = string.IsNullOrWhiteSpace(x?.Name) ? null : x.Name;
                    HeaderView.SetImage(x?.AvatarUrl, Images.Avatar);
                    RefreshHeaderView();
                }).AddTo(d);
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            if (!ViewModel.IsLoggedInUser)
                NavigationItem.RightBarButtonItem = _actionButton.Value;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            NavigationItem.RightBarButtonItem = null;
        }

        private void ShowExtraMenu()
        {
            var sheet = new UIActionSheet();
            var followButton = sheet.AddButton(ViewModel.IsFollowing ? "Unfollow" : "Follow");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.Dismissed += (s, e) => {
                BeginInvokeOnMainThread(() =>
                {
                    if (e.ButtonIndex == followButton)
                    {
                        ViewModel.ToggleFollowingCommand.Execute(null);
                    }
                });

                sheet.Dispose();
            };

            sheet.ShowInView(this.View);
        }
    }
}

