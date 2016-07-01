using System.Collections.Generic;
using System.Windows.Input;
using CodeHub.Core.Data;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.Events;
using CodeHub.Core.ViewModels.Gists;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.Core.ViewModels.Repositories;
using CodeHub.Core.ViewModels.Users;
using System.Linq;
using CodeHub.Core.Utils;
using CodeHub.Core.Messages;
using CodeHub.Core.ViewModels.Notifications;
using GitHubSharp.Models;
using ReactiveUI;
using System;
using Splat;
using System.Reactive;
using System.Threading.Tasks;
using System.Reactive.Linq;
using CodeHub.Core.ViewModels.Organizations;

namespace CodeHub.Core.ViewModels.App
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly IApplicationService _application;
        private readonly IFeaturesService _featuresService;
        private readonly IDisposable _notificationCountToken;

        private int _notifications;
        public int Notifications
        {
            get { return _notifications; }
            set { this.RaiseAndSetIfChanged(ref _notifications, value); }
        }

        private List<BasicUserModel> _organizations;
        public List<BasicUserModel> Organizations
        {
            get { return _organizations; }
            set { this.RaiseAndSetIfChanged(ref _organizations, value); }
        }
        
        public GitHubAccount Account => _application.Account;

        public bool ShouldShowUpgrades => !_featuresService.IsProEnabled;

        public IReactiveCommand<Unit> LoadCommand { get; }
        
        public MenuViewModel(IApplicationService application, IFeaturesService featuresService)
        {
            _application = GetService<IApplicationService>();
            _featuresService = GetService<IFeaturesService>();
            _notificationCountToken = Messenger.Subscribe<NotificationCountMessage>(OnNotificationCountMessage);

            LoadCommand = ReactiveCommand.CreateAsyncTask(t =>
            {
                var notificationRequest = _application.Client.Notifications.GetAll();
                var req1 = _application.Client.ExecuteAsync(notificationRequest)
                            .OnSuccess(x => Notifications = x.Data.Count);

                var organizationsRequest = _application.Client.AuthenticatedUser.GetOrganizations();
                var req2 = _application.Client.ExecuteAsync(organizationsRequest)
                            .OnSuccess(x => Organizations = x.Data.ToList());

                return Task.WhenAll(req1, req2);
            });

            GoToProfileCommand
                .Select(_ => new UserViewModel(_application.Account.Username))
                .Subscribe(NavigateTo);

            GoToNotificationsCommand
                .Select(_ => new NotificationsViewModel())
                .Subscribe(NavigateTo);

            GoToMyIssuesCommand
                .Select(_ => new MyIssuesViewModel())
                .Subscribe(NavigateTo);

            GoToMyEvents
                .Select(_ => new UserEventsViewModel(_application.Account.Username))
                .Subscribe(NavigateTo);

            GoToMyGistsCommand
                .Select(_ => new UserGistsViewModel(_application.Account.Username))
                .Subscribe(NavigateTo);

            GoToStarredGistsCommand
                .Select(_ => new StarredGistsViewModel())
                .Subscribe(NavigateTo);

            GoToPublicGistsCommand
                .Select(_ => new PublicGistsViewModel())
                .Subscribe(NavigateTo);

            GoToStarredRepositoriesCommand
                .Select(_ => new RepositoriesStarredViewModel())
                .Subscribe(NavigateTo);

            GoToOwnedRepositoriesCommand
                .Select(_ => new UserRepositoriesViewModel(Account.Username))
                .Subscribe(NavigateTo);

            GoToExploreRepositoriesCommand
                .Select(_ => new RepositoriesExploreViewModel())
                .Subscribe(NavigateTo);

            GoToTrendingRepositoriesCommand
                .Select(_ => new RepositoriesTrendingViewModel())
                .Subscribe(NavigateTo);

            GoToOrganizationsCommand
                .Select(_ => new OrganizationsViewModel(Account.Username))
                .Subscribe(NavigateTo);

            GoToNewsCommand
                .Select(_ => new NewsViewModel())
                .Subscribe(NavigateTo);

            GoToDefaultTopView.Subscribe(_ =>
            {
                var startupViewName = Accounts.ActiveAccount.DefaultStartupView;
                if (!string.IsNullOrEmpty(startupViewName))
                {
                    var props = from p in GetType().GetProperties()
                                let attr = p.GetCustomAttributes(typeof(PotentialStartupViewAttribute), true)
                                where attr.Length == 1
                                select new { Property = p, Attribute = attr[0] as PotentialStartupViewAttribute };

                    foreach (var p in props)
                    {
                        if (string.Equals(startupViewName, p.Attribute.Name))
                        {
                            (p.Property.GetValue(this) as IReactiveCommand)?.ExecuteIfCan();
                        }
                    }
                }

                //Oh no... Look for the last resort DefaultStartupViewAttribute
                var deprop = (from p in GetType().GetProperties()
                              let attr = p.GetCustomAttributes(typeof(DefaultStartupViewAttribute), true)
                              where attr.Length == 1
                              select new { Property = p, Attribute = attr[0] as DefaultStartupViewAttribute }).FirstOrDefault();

                //That shouldn't happen...
                if (deprop == null)
                    return;
                
                var val = deprop.Property.GetValue(this);
                (val as IReactiveCommand)?.ExecuteIfCan();
            });
        }

        private void OnNotificationCountMessage(NotificationCountMessage msg)
        {
            Notifications = msg.Count;
        }

        public IReactiveCommand<object> GoToAccountsCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Profile")]
        public IReactiveCommand<object> GoToProfileCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Notifications")]
        public IReactiveCommand<object> GoToNotificationsCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("My Issues")]
        public IReactiveCommand<object> GoToMyIssuesCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("My Events")]
        public IReactiveCommand<object> GoToMyEvents { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("My Gists")]
        public IReactiveCommand<object> GoToMyGistsCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Starred Gists")]
        public IReactiveCommand<object> GoToStarredGistsCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Public Gists")]
        public IReactiveCommand<object> GoToPublicGistsCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Starred Repositories")]
        public IReactiveCommand<object> GoToStarredRepositoriesCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Owned Repositories")]
        public IReactiveCommand<object> GoToOwnedRepositoriesCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Explore Repositories")]
        public IReactiveCommand<object> GoToExploreRepositoriesCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Trending Repositories")]
        public IReactiveCommand<object> GoToTrendingRepositoriesCommand { get; } = ReactiveCommand.Create();

        [PotentialStartupViewAttribute("Organizations")]
        public IReactiveCommand<object> GoToOrganizationsCommand { get; } = ReactiveCommand.Create();

        [DefaultStartupViewAttribute]
        [PotentialStartupViewAttribute("News")]
        public IReactiveCommand<object> GoToNewsCommand { get; } = ReactiveCommand.Create();

        private static IAccountsService Accounts => Locator.Current.GetService<IAccountsService>();

        public IReactiveCommand<object> GoToDefaultTopView { get; } = ReactiveCommand.Create();

        public void DeletePinnedRepository(PinnedRepository repository)
        {
            if (repository == null) return;
            Accounts.ActiveAccount.PinnnedRepositories.RemovePinnedRepository(repository.Id);
        }

        public IEnumerable<PinnedRepository> PinnedRepositories
        {
            get { return Accounts.ActiveAccount.PinnnedRepositories; }
        }

//
//        private async Task PromptForPushNotifications()
//        {
//            // Push notifications are not enabled for enterprise
//            if (Account.IsEnterprise)
//                return;
//
//            try
//            {
//                var features = Mvx.Resolve<IFeaturesService>();
//                var alertDialog = Mvx.Resolve<IAlertDialogService>();
//                var push = Mvx.Resolve<IPushNotificationsService>();
//                var 
//                // Check for push notifications
//                if (Account.IsPushNotificationsEnabled == null && features.IsPushNotificationsActivated)
//                {
//                    var result = await alertDialog.PromptYesNo("Push Notifications", "Would you like to enable push notifications for this account?");
//                    if (result)
//                        Task.Run(() => push.Register()).ToBackground();
//                    Account.IsPushNotificationsEnabled = result;
//                    Accounts.Update(Account);
//                }
//                else if (Account.IsPushNotificationsEnabled.HasValue && Account.IsPushNotificationsEnabled.Value)
//                {
//                    Task.Run(() => push.Register()).ToBackground();
//                }
//            }
//            catch (Exception e)
//            {
//                _alertDialogService.Alert("Error", e.Message);
//            }
//        }
    }
}
