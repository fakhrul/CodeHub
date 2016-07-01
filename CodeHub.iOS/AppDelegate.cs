// --------------------------------------------------------------------------------------------------------------------
// <summary>
//    Defines the AppDelegate type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System;
using Foundation;
using UIKit;
using CodeHub.Core.Utils;
using CodeHub.Core.Services;
using System.Threading.Tasks;
using System.Linq;
using ObjCRuntime;
using System.Net.Http;
using CodeHub.Services;
using ReactiveUI;
using CodeHub.Core.Messages;
using CodeHub.XCallback;
using System.Reactive.Linq;
using Splat;
using CodeHub.ViewControllers.Source;
using CodeHub.ViewControllers.Issues;
using CodeHub.Views.PullRequests;
using CodeHub.Views.Repositories;
using CodeHub.Core;

namespace CodeHub
{
    /// <summary>
    /// The UIApplicationDelegate for the application. This class is responsible for launching the 
    /// User Interface of the application, as well as listening (and optionally responding) to 
    /// application events from iOS.
    /// </summary>
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public string DeviceToken;

        public override UIWindow Window { get; set; }

        /// <summary>
        /// This is the main entry point of the application.
        /// </summary>
        /// <param name="args">The args.</param>
        public static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }

        /// <summary>
        /// Finished the launching.
        /// </summary>
        /// <param name="app">The app.</param>
        /// <param name="options">The options.</param>
        /// <returns>True or false.</returns>
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // Register all services for our application
            Application.Initialize();
            ServiceRegistration.Register();

            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            // Initialize the error service!
            var errorService = Locator.Current.GetService<IErrorService>();
            errorService.Init();

            var culture = new System.Globalization.CultureInfo("en");
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Setup theme
            UIApplication.SharedApplication.SetStatusBarStyle(UIStatusBarStyle.LightContent, true);
            Theme.Setup();

            var features = Locator.Current.GetService<IFeaturesService>();
            var defaultValueService = Locator.Current.GetService<IDefaultValueService>();
            var purchaseService = Locator.Current.GetService<IInAppPurchaseService>();
            purchaseService.ThrownExceptions.Subscribe(ex => {
                AlertDialogService.ShowAlert("Error Purchasing", ex.Message);
                errorService.Log(ex);
            });

            #if DEBUG
            features.ActivateProDirect();
            #endif 

//            options = new NSDictionary (UIApplication.LaunchOptionsRemoteNotificationKey, 
//                new NSDictionary ("r", "octokit/octokit.net", "i", "739", "u", "thedillonb"));
//
            if (options != null)
            {
                if (options.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey)) 
                {
                    var remoteNotification = options[UIApplication.LaunchOptionsRemoteNotificationKey] as NSDictionary;
                    if(remoteNotification != null) {
                        HandleNotification(remoteNotification, true);
                    }
                }
            }

            // Set the client constructor
            GitHubSharp.Client.ClientConstructor = () => new HttpClient(new CustomHttpMessageHandler());

            bool hasSeenWelcome;
            if (!defaultValueService.TryGet("HAS_SEEN_WELCOME_INTRO", out hasSeenWelcome) || !hasSeenWelcome)
            {
                defaultValueService.Set("HAS_SEEN_WELCOME_INTRO", true);
                var welcomeViewController = new CodeHub.ViewControllers.Walkthrough.WelcomePageViewController();
                welcomeViewController.WantsToDimiss += GoToStartupView;
                TransitionToViewController(welcomeViewController);
            }
            else
            {
                GoToStartupView();
            }

            Window.MakeKeyAndVisible();

            // Notifications don't work on teh simulator so don't bother
            if (Runtime.Arch != Arch.SIMULATOR && features.IsProEnabled)
                RegisterUserForNotifications();

            return true;
        }

        public void RegisterUserForNotifications()
        {
            var notificationTypes = UIUserNotificationSettings.GetSettingsForTypes (UIUserNotificationType.Alert | UIUserNotificationType.Sound, null);
            UIApplication.SharedApplication.RegisterUserNotificationSettings(notificationTypes);
        }

        private void GoToStartupView()
        {
            var startup = new CodeHub.ViewControllers.Application.StartupViewController();
            TransitionToViewController(startup);
            MessageBus.Current.Listen<LogoutMessage>()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => startup.DismissViewController(true, null));
        }

        private void TransitionToViewController(UIViewController viewController)
        {
            UIView.Transition(Window, 0.35, UIViewAnimationOptions.TransitionCrossDissolve, () => 
                Window.RootViewController = viewController, null);
        }

        class CustomHttpMessageHandler : DelegatingHandler
        {
            public CustomHttpMessageHandler()
                : base(new HttpClientHandler())
            {
            }

            protected override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                if (!string.Equals(request.Method.ToString(), "get", StringComparison.OrdinalIgnoreCase))
                    NSUrlCache.SharedCache.RemoveAllCachedResponses();
                return base.SendAsync(request, cancellationToken);
            }
        }

        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
        {
            if (application.ApplicationState == UIApplicationState.Active)
                return;
            HandleNotification(userInfo, false);
        }

        private void HandleNotification(NSDictionary data, bool fromBootup)
        {
            try
            {
                var appService = Locator.Current.GetService<IApplicationService>();
                var repoId = RepositoryIdentifier.FromFullName(data["r"].ToString());
                var parameters = new Dictionary<string, string>() {{"Username", repoId?.Owner}, {"Repository", repoId?.Name}};

                UIViewController view;
                if (data.ContainsKey(new NSString("c")))
                {
                    view = new ChangesetViewController(repoId?.Owner, repoId?.Name, data["c"].ToString(), true);
                }
                else if (data.ContainsKey(new NSString("i")))
                {
                    var id = long.Parse(data["i"].ToString());
                    view = new IssueViewController(repoId?.Owner, repoId?.Name, id);
                }
                else if (data.ContainsKey(new NSString("p")))
                {
                    var id = long.Parse(data["p"].ToString());
                    view = new PullRequestViewController(repoId?.Owner, repoId?.Name, id);
                }
                else
                {
                    view = new RepositoryViewController(repoId?.Owner, repoId?.Name);
                }

                var username = data["u"].ToString();

                if (appService.Account == null || !appService.Account.Username.Equals(username))
                {
                    var user = appService.Accounts.FirstOrDefault(x => x.Username.Equals(username));
                    if (user != null)
                    {
                        appService.DeactivateUser();
                        appService.Accounts.SetDefault(user);
                    }
                }

                //appService.SetUserActivationAction(() => nav.NavigateTo(view));

                if (appService.Account == null && !fromBootup)
                {
                    //TODO: FIX THIS
                    //var startupViewModelRequest = MvxViewModelRequest<Core.ViewModels.App.StartupViewModel>.GetDefaultRequest();
                    //viewDispatcher.ShowViewModel(startupViewModelRequest);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Handle Notifications issue: " + e);
            }
        }

        public override void DidRegisterUserNotificationSettings (UIApplication application, UIUserNotificationSettings notificationSettings)
        {
            application.RegisterForRemoteNotifications ();
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            DeviceToken = deviceToken.Description.Trim('<', '>').Replace(" ", "");

            var app = Locator.Current.GetService<IApplicationService>();
            if (app.Account != null && !app.Account.IsPushNotificationsEnabled.HasValue)
            {
                Locator.Current.GetService<IPushNotificationsService>().Register().ToBackground();
                app.Account.IsPushNotificationsEnabled = true;
                app.Accounts.Update(app.Account);
            }
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            AlertDialogService.ShowAlert("Error Registering for Notifications", error.LocalizedDescription);
        }

        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            var uri = new Uri(url.AbsoluteString);

            if (uri.Host == "x-callback-url")
            {
                XCallbackProvider.Handle(new XCallbackQuery(url.AbsoluteString));
                return true;
            }
            else
            {
                var path = url.AbsoluteString.Replace("codehub://", "");
                var queryMarker = path.IndexOf("?", StringComparison.Ordinal);
                if (queryMarker > 0)
                    path = path.Substring(0, queryMarker);

                if (!path.EndsWith("/", StringComparison.Ordinal))
                    path += "/";
//                var first = path.Substring(0, path.IndexOf("/", StringComparison.Ordinal));
                return UrlRouteProvider.Handle(path);
            }
        }
    }
}