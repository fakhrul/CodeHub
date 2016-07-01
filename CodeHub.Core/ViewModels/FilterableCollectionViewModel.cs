using CodeHub.Core.Services;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels
{
    public class FilterableCollectionViewModel<T, TF> : CollectionViewModel<T>, IFilterableViewModel<TF> where TF : FilterModel<TF>, new()
    {
        protected TF _filter;
        private readonly string _filterKey;

        public TF Filter
        {
            get { return _filter; }
            set { this.RaiseAndSetIfChanged(ref _filter, value); }
        }

        public FilterableCollectionViewModel(string filterKey)
        {
            _filterKey = filterKey;
            var accounts = Locator.Current.GetService<IAccountsService>();
            _filter = accounts.ActiveAccount.Filters.GetFilter<TF>(_filterKey);
        }

        public void ApplyFilter(TF filter, bool saveAsDefault = false)
        {
            Filter = filter;
            if (saveAsDefault)
            {
                var accounts = Locator.Current.GetService<IAccountsService>();
                accounts.ActiveAccount.Filters.AddFilter(_filterKey, filter);
            }
        }
    }
}

