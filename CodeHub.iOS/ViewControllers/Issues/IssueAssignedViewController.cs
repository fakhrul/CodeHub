using System;
using System.Linq;
using CodeHub.DialogElements;
using CodeHub.ViewControllers;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.Utilities;
using CodeHub.Core.Utilities;
using ReactiveUI;

namespace CodeHub.Views.Issues
{
    public class IssueAssignedViewController : ViewModelCollectionDrivenDialogViewController<IssueAssignedViewModel>
    {
        public IssueAssignedViewController()
        {
            EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Person.ToEmptyListImage(), "There are no assignees."));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            BindCollection(ViewModel.Users, x =>
            {
                var avatar = new GitHubAvatar(x.AvatarUrl);
                var el = new UserElement(x.Login, string.Empty, string.Empty, avatar);
                el.Clicked.Subscribe(_ => {
                    if (ViewModel.SelectedUser != null && string.Equals(ViewModel.SelectedUser.Login, x.Login))
                        ViewModel.SelectedUser = null;
                    else
                        ViewModel.SelectedUser = x;
                });

                if (ViewModel.SelectedUser != null && string.Equals(ViewModel.SelectedUser.Login, x.Login, StringComparison.OrdinalIgnoreCase))
                    el.Accessory = UITableViewCellAccessory.Checkmark;
                else
                    el.Accessory = UITableViewCellAccessory.None;
                return el;
            });

            this.WhenAnyValue(x => x.ViewModel.SelectedUser).Subscribe(x =>
            {
                if (Root.Count == 0)
                    return;
                foreach (var m in Root[0].Elements.Cast<UserElement>())
                    m.Accessory = (x != null && string.Equals(ViewModel.SelectedUser.Login, m.Caption, StringComparison.OrdinalIgnoreCase)) ? 
                                     UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
            });

            this.WhenAnyValue(x => x.ViewModel.IsSaving).SubscribeStatus("Saving...");
        }
    }
}

