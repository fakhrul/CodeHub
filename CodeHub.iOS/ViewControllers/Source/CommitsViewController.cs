using CodeHub.Core.ViewModels.Changesets;
using CodeHub.DialogElements;
using UIKit;
using CodeHub.ViewControllers.Repositories;
using System;
using System.Reactive.Linq;
using ReactiveUI;

namespace CodeHub.ViewControllers.Source
{
    public abstract class CommitsViewController<TViewModel> : ViewModelCollectionDrivenDialogViewController<TViewModel>
        where TViewModel : CommitsViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.EstimatedRowHeight = 64f;
            TableView.RowHeight = UITableView.AutomaticDimension;

            var username = ViewModel.Username;
            var repository = ViewModel.Repository;

            BindCollection(ViewModel.Commits, x =>
            {
                return new CommitElement(x, () =>
                {
                    var view = new ChangesetViewController(username, repository, x.Sha);
                    NavigationController.PushViewController(view, true);
                });
            });

            this.WhenAnyValue(x => x.ViewModel.ShouldShowPro)
                .Where(x => x)
                .Take(1)
                .Subscribe(_ => this.ShowPrivateView());
        }
    }
}

