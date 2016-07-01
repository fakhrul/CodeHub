using System;
using CodeHub.ViewControllers;
using UIKit;
using CodeHub.Utilities;
using CodeHub.Core.ViewModels.Repositories;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.TableViewSources;

namespace CodeHub.Views.Repositories
{
    public class RepositoriesExploreViewController : BaseTableViewController<RepositoriesExploreViewModel, RepositoryItemViewModel>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TableView.Source = new RepositoryTableViewSource(TableView, ViewModel.Items);
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Repo.ToEmptyListImage(), "There are no repositories."));

            OnActivation(d =>
            {
                ViewModel
                    .SearchCommand
                    .IsExecuting
                    .SubscribeStatus("Searching...")
                    .AddTo(d);
                
                SearchBar
                    .GetSearchObservable()
                    .Do(_ => SearchBar.ResignFirstResponder())
                    .InvokeCommand(ViewModel.SearchCommand)
                    .AddTo(d);
            });
        }
    }
}

