using System;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels;
using CodeHub.Utilities;
using CodeHub.Views;
using ReactiveUI;
using UIKit;

namespace CodeHub.ViewControllers
{
    public abstract class BaseTableViewController<TViewModel, TItemViewModel> : BaseViewController<TViewModel>
        where TViewModel : class, IListViewModel<TItemViewModel>
    {
        private readonly Lazy<EnhancedTableView> _tableView;
        private readonly Lazy<UISearchBar> _searchBar;

        public EnhancedTableView TableView => _tableView.Value;

        public UISearchBar SearchBar => _searchBar.Value;

        protected BaseTableViewController(UITableViewStyle style = UITableViewStyle.Plain)
        {
            _tableView = new Lazy<EnhancedTableView>(() => new EnhancedTableView(style));
            _searchBar = new Lazy<UISearchBar>(TableView.CreateSearchBar);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            this.AddTableView(TableView);

            OnActivation(disposable =>
            {
                var loadable =
                    this.WhenAnyValue(x => x.ViewModel)
                    .OfType<ILoadableViewModel>()
                    .Select(x => x.LoadCommand.IsExecuting)
                    .Switch()
                    .StartWith(false);

                var paginatable =
                    this.WhenAnyValue(x => x.ViewModel)
                    .OfType<IPaginatableViewModel>()
                    .Select(x => x.WhenAnyObservable(y => y.LoadMoreCommand.IsExecuting))
                    .Switch()
                    .StartWith(false);

                Observable.CombineLatest(loadable, paginatable, (l, p) => l || p)
                    .Subscribe(x => TableView.IsLoading = x)
                    .AddTo(disposable);

                this.WhenAnyValue(x => x.ViewModel.SearchText)
                    .Subscribe(x => SearchBar.Text = x)
                    .AddTo(disposable);

                SearchBar.GetChangedObservable()
                    .Subscribe(x => ViewModel.SearchText = x)
                    .AddTo(disposable);

                SearchBar.GetCanceledObservable()
                    .Subscribe(x => ViewModel.SearchText = string.Empty)
                    .AddTo(disposable);

                this.WhenAnyValue(x => x.ViewModel.IsEmpty)
                    .Subscribe(x => TableView.IsEmpty = x)
                    .AddTo(disposable);
            });

        }

        protected override void Dispose(bool disposing)
        {
            if (_tableView.IsValueCreated)
            {
                var tableView = _tableView.Value;
                InvokeOnMainThread(() =>
                {
                    tableView.Source?.Dispose();
                    tableView.Source = null;
                });
            }

            base.Dispose(disposing);
        }
    }
}

