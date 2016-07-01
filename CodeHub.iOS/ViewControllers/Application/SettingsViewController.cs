using System;
using CodeHub.Core.Services;
using CodeHub.Core.ViewModels.App;
using CodeHub.Utilities;
using UIKit;
using Foundation;
using CodeHub.DialogElements;
using Splat;
using ReactiveUI;
using System.Reactive.Linq;
using CodeHub.Views.Repositories;

namespace CodeHub.ViewControllers.Application
{
    public class SettingsViewController : ViewModelDrivenDialogViewController<SettingsViewModel>
    {
        public SettingsViewController()
        {
            ViewModel = new SettingsViewModel();
            Title = "Settings";

            OnActivation(d =>
            {
                ViewModel.WhenAnyValue(x => x.PushNotificationsEnabled).Subscribe(_ => CreateTable()).AddTo(d);
                ViewModel.WhenAnyValue(x => x.IsSaving).SubscribeStatus("Saving...").AddTo(d);
                ViewModel
                    .ShowUpgradeCommand
                    .Subscribe(_ => NavigationController.PushViewController(new UpgradeViewController(), true))
                    .AddTo(d);
                CreateTable();
            });
        }

        private void CreateTable()
        {
            var application = Locator.Current.GetService<IApplicationService>();
            var vm = ViewModel;
            var currentAccount = application.Account;
            var accountSection = new Section("Account");

            var showOrganizationsInEvents = new BooleanElement("Show Organizations in Events", currentAccount.ShowOrganizationsInEvents);
            showOrganizationsInEvents.Changed.Subscribe(x => {
                currentAccount.ShowOrganizationsInEvents = x;
                application.Accounts.Update(currentAccount);
            });

            var showOrganizations = new BooleanElement("List Organizations in Menu", currentAccount.ExpandOrganizations);
            showOrganizations.Changed.Subscribe(x => { 
                currentAccount.ExpandOrganizations = x;
                application.Accounts.Update(currentAccount);
            });

            var repoDescriptions = new BooleanElement("Show Repo Descriptions", currentAccount.ShowRepositoryDescriptionInList);
            repoDescriptions.Changed.Subscribe(x => {
                currentAccount.ShowRepositoryDescriptionInList = x;
                application.Accounts.Update(currentAccount);
            });

            var startupView = new StringElement("Startup View", vm.DefaultStartupViewName, UITableViewCellStyle.Value1)
            { 
                Accessory = UITableViewCellAccessory.DisclosureIndicator,
            };
            startupView.Clicked
                       .Select(_ => new DefaultStartupViewController())
                       .Subscribe(x => NavigationController.PushViewController(x, true));

            var pushNotifications = new BooleanElement("Push Notifications", vm.PushNotificationsEnabled);
            pushNotifications.Changed.Subscribe(e => vm.PushNotificationsEnabled = e);
            accountSection.Add(pushNotifications);
       
            var source = new StringElement("Source Code");
            source.Clicked
                  .Select(_ => new RepositoryViewController("thedillonb", "codehub"))
                  .Subscribe(x => NavigationController.PushViewController(x, true));

            var follow = new StringElement("Follow On Twitter");
            follow.Clicked.Subscribe(_ => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://twitter.com/CodeHubapp")));

            var rate = new StringElement("Rate This App");
            rate.Clicked.Subscribe(_ => UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/codehub-github-for-ios/id707173885?mt=8")));

            var aboutSection = new Section("About", "Thank you for downloading. Enjoy!") { source, follow, rate };
        
            if (vm.ShouldShowUpgrades)
            {
                var upgrades = new StringElement("Upgrades");
                upgrades.Clicked.InvokeCommand(vm.ShowUpgradeCommand);
                aboutSection.Add(upgrades);
            }

            var appVersion = new StringElement("App Version", UIApplication.SharedApplication.GetVersion())
            { 
                Accessory = UITableViewCellAccessory.None,
                SelectionStyle = UITableViewCellSelectionStyle.None
            };

            aboutSection.Add(appVersion);

            //Assign the root
            Root.Reset(accountSection, new Section("Appearance") { showOrganizationsInEvents, showOrganizations, repoDescriptions, startupView }, aboutSection);
        }
    }
}


