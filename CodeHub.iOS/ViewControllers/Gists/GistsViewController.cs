using CodeHub.Core.ViewModels.Gists;
using UIKit;
using System;
using CodeHub.Views;
using CodeHub.TableViewSources;
using ReactiveUI;

namespace CodeHub.ViewControllers.Gists
{
    public abstract class GistsViewController<TViewModel> : BaseTableViewController<TViewModel, GistItemViewModel>
        where TViewModel : GistsViewModel
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new GistTableViewSource(TableView, ViewModel.Items);
            TableView.Source = source;
            source.RequestMore.InvokeCommand(ViewModel.LoadMoreCommand);
            TableView.EmptyView = new Lazy<UIView>(() =>
                new EmptyListView(Octicon.Gist.ToEmptyListImage(), "There are no gists."));
        }
    }
}

