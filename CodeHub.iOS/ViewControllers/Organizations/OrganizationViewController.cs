using CodeHub.Core.ViewModels.Organizations;
using UIKit;
using CoreGraphics;
using CodeHub.DialogElements;
using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace CodeHub.ViewControllers.Organizations
{
    public class OrganizationViewController : PrettyDialogViewController<OrganizationViewModel>
    {
        public OrganizationViewController()
        {
        }

        public OrganizationViewController(string organizationName)
        {
            ViewModel = new OrganizationViewModel(organizationName);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = ViewModel.Username;

            var members = new StringElement("Members", Octicon.Person.ToImage());
            var teams = new StringElement("Teams", Octicon.Organization.ToImage());
            var followers = new StringElement("Followers", Octicon.Heart.ToImage());
            var events = new StringElement("Events", Octicon.Rss.ToImage());
            var repos = new StringElement("Repositories", Octicon.Repo.ToImage());
            var gists = new StringElement("Gists", Octicon.Gist.ToImage());
            Root.Reset(new Section(new UIView(new CGRect(0, 0, 0, 20f))) { members, teams }, new Section { events, followers }, new Section { repos, gists });

            OnActivation(d =>
            {
                members.Clicked.InvokeCommand(ViewModel.GoToMembersCommand).AddTo(d);
                teams.Clicked.InvokeCommand(ViewModel.GoToTeamsCommand).AddTo(d);
                followers.Clicked.InvokeCommand(ViewModel.GoToFollowersCommand).AddTo(d);
                events.Clicked.InvokeCommand(ViewModel.GoToEventsCommand).AddTo(d);
                repos.Clicked.InvokeCommand(ViewModel.GoToRepositoriesCommand).AddTo(d);
                gists.Clicked.InvokeCommand(ViewModel.GoToGistsCommand).AddTo(d);

                this.WhenAnyValue(x => x.ViewModel.Organization)
                    .Where(x => x != null)
                    .Subscribe(x =>
                    {
                        HeaderView.SubText = string.IsNullOrWhiteSpace(x.Name) ? x.Login : x.Name;
                        HeaderView.SetImage(x.AvatarUrl, Images.Avatar);
                        RefreshHeaderView();
                    })
                    .AddTo(d);
            });
        }
    }
}

