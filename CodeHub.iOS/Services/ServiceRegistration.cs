using System;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using CodeHub.ViewControllers.Application;
using Splat;

namespace CodeHub.Services
{
    public static class ServiceRegistration
    {
        public static void Register()
        {
            var viewModelViewService = new ViewModelViewService();
            viewModelViewService.RegisterViewModels(typeof(StartupViewModel).Assembly);
            viewModelViewService.RegisterViewModels(typeof(StartupViewController).Assembly);

            Register<IViewModelViewService>(_ => viewModelViewService);
            Register<IJsonSerializationService>(_ => new JsonSerializationService());
            Register<IMarkdownService>(_ => new MarkdownService());
            Register<IApplicationService>(x => new ApplicationService(x.GetService<IAccountsService>()));
            Register<IAccountsService>(x => new AccountsService(x.GetService<IDefaultValueService>(), x.GetService<IAccountPreferencesService>()));
            Register<ILoginService>(x => new LoginService(x.GetService<IAccountsService>()));

            Register<IAccountPreferencesService>(_ => new AccountPreferencesService());
            Register<IAlertDialogService>(_ => new AlertDialogService());
            Register<IDefaultValueService>(_ => new DefaultValueService());
            Register<IErrorService>(x => new ErrorService(x.GetService<IAnalyticsService>()));
            Register<IFeaturesService>(x => new FeaturesService(x.GetService<IDefaultValueService>(), x.GetService<IInAppPurchaseService>()));
            Register<IInAppPurchaseService>(x => new InAppPurchaseService(x.GetService<IDefaultValueService>()));
            Register<IPushNotificationsService>(_ => new PushNotificationsService());
        }

        private static void Register<T>(Func<IDependencyResolver, T> creator)
        {
            var locator = Locator.Current;
            var cvt = new Func<object>(() => creator(locator));
            Locator.CurrentMutable.RegisterLazySingleton(cvt, typeof(T));
        }
    }
}

