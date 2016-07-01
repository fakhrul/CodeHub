using System;
using System.Linq;
using CodeHub.Core.Services;
using CodeHub.Core.Utils;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.App
{
    public class DefaultStartupViewModel : BaseViewModel
    {
        private readonly IAccountsService _accountsService;
        private readonly Type _menuViewModelType;

        public CollectionViewModel<string> StartupViews { get; } = new CollectionViewModel<string>();

        private string _selectedStartupView;
        public string SelectedStartupView
        {
            get { return _selectedStartupView; }
            set { this.RaiseAndSetIfChanged(ref _selectedStartupView, value); }
        }

        public DefaultStartupViewModel()
        {
            _accountsService = Locator.Current.GetService<IAccountsService>();
            _menuViewModelType = typeof(MenuViewModel);

            Title = "Default Startup View";

            var props = from p in _menuViewModelType.GetProperties()
                        let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true)
                        where attr.Length == 1
                        select attr[0] as PotentialStartupViewAttribute;

            SelectedStartupView = _accountsService.ActiveAccount.DefaultStartupView;
            StartupViews.Items.Reset(props.Select(x => x.Name));

            this.WhenAnyValue(x => x.SelectedStartupView).Subscribe(x =>
            {
                _accountsService.ActiveAccount.DefaultStartupView = x;
                _accountsService.Update(_accountsService.ActiveAccount);
            });
        }
    }
}

