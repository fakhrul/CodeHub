using ReactiveUI;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestsViewModel : BaseViewModel
    {
        public PullRequestListViewModel OpenPullRequests { get; }

        public PullRequestListViewModel ClosedPullRequests { get; }

        private int _selectedFilter;
        public int SelectedFilter
        {
            get { return _selectedFilter; }
            set { this.RaiseAndSetIfChanged(ref _selectedFilter, value); }
        }

        public PullRequestsViewModel(string username, string repository) 
        {
            OpenPullRequests = new PullRequestListViewModel(username, repository, true);
            ClosedPullRequests = new PullRequestListViewModel(username, repository, false);

            Title = "Pull Requests";
        }
    }
}
