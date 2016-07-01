using CodeHub.Core.Services;
using System;
using System.Threading.Tasks;
using ReactiveUI;
using Splat;

namespace CodeHub.Core.ViewModels.App
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly IFeaturesService _featuresService;

        public IReactiveCommand<object> ShowUpgradeCommand { get; } = ReactiveCommand.Create();

        public SettingsViewModel()
        {
            _featuresService = Locator.Current.GetService<IFeaturesService>();
        }

        public string DefaultStartupViewName
        {
            get { return this.GetApplication().Account.DefaultStartupView; }
        }

        public bool ShouldShowUpgrades
        {
            get { return _featuresService.IsProEnabled; }
        }

        private bool _isSaving;
        public bool IsSaving
        {
            get { return _isSaving; }
            private set { this.RaiseAndSetIfChanged(ref _isSaving, value); }
        }

        public bool PushNotificationsEnabled
        {
            get 
            { 
                return this.GetApplication().Account.IsPushNotificationsEnabled.HasValue && this.GetApplication().Account.IsPushNotificationsEnabled.Value; 
            }
            set 
            { 
                if (_featuresService.IsProEnabled)
                {
                    RegisterPushNotifications(value).ToBackground();
                }
                else
                {
                    GetService<IAlertDialogService>()
                        .PromptYesNo("Requires Activation", "Push notifications require activation. Would you like to go there now to activate push notifications?")
                        .ContinueWith(t =>
                        {
                            if (t.Status == TaskStatus.RanToCompletion && t.Result)
                                ShowUpgradeCommand.ExecuteIfCan();
                        });
                    this.RaisePropertyChanged(nameof(PushNotificationsEnabled));
                }
            }
        }

        private async Task RegisterPushNotifications(bool enabled)
        {
            var notificationService = GetService<IPushNotificationsService>();

            try
            {
                IsSaving = true;
                if (enabled)
                {
                    await notificationService.Register();
                }
                else
                {
                    await notificationService.Deregister();
                }

                this.GetApplication().Account.IsPushNotificationsEnabled = enabled;
                this.GetApplication().Accounts.Update(this.GetApplication().Account);
            }
            catch (Exception e)
            {
                GetService<IAlertDialogService>()
                    .Alert("Unable to register for push notifications!", e.Message)
                    .ToBackground();
            }
            finally
            {
                this.RaisePropertyChanged(nameof(PushNotificationsEnabled));
                IsSaving = false;
            }
        }
    }
}
