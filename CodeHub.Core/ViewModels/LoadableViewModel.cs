using GitHubSharp;
using System.Threading.Tasks;
using System.Net;
using System;
using ReactiveUI;
using System.Reactive;

namespace CodeHub.Core.ViewModels
{
    public interface ILoadableViewModel
    {
        IReactiveCommand<Unit> LoadCommand { get; }
    }

    public abstract class LoadableViewModel : BaseViewModel, ILoadableViewModel
    {
        public IReactiveCommand<Unit> LoadCommand { get; }

        private async Task LoadResource()
        {
            var retry = false;
            while (true)
            {
                if (retry)
                    await Task.Delay(100);

                try
                {
                    await Load();
                    return;
                }
                catch (WebException)
                {
                    if (!retry)
                        retry = true;
                    else
                        throw;
                }
            }
        }

        protected LoadableViewModel()
        {
            LoadCommand = ReactiveCommand.CreateAsyncTask(_ => HandleLoadCommand());
        }

        private async Task HandleLoadCommand()
        {
            try
            {
                await LoadResource();
            }
            catch (OperationCanceledException e)
            {
                // The operation was canceled... Don't worry
                System.Diagnostics.Debug.WriteLine("The operation was canceled: " + e.Message);
            }
            catch (System.IO.IOException)
            {
                DisplayAlert("Unable to communicate with GitHub as the transmission was interrupted! Please try again.");
            }
            catch (StatusCodeException e)
            {
                DisplayAlert(e.Message);
            }
            catch (Exception e)
            {
                DisplayAlert("The request to load this item did not complete successfuly! " + e.Message);
            }
        }

        protected abstract Task Load();
    }
}

