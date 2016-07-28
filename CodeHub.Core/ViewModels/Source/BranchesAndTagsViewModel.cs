using ReactiveUI;

namespace CodeHub.Core.ViewModels.Source
{
    public class BranchesAndTagsViewModel : BaseViewModel
    {
        public BranchesViewModel Branches { get; }

        public TagsViewModel Tags { get; }

        private bool _showBranches;
        public bool ShowBranches
        {
            get { return _showBranches; }
            set { this.RaiseAndSetIfChanged(ref _showBranches, value); }
        }

        public BranchesAndTagsViewModel(string username, string repository, bool showBranches = true)
        {
            Branches = BranchesViewModel.ForSource(username, repository);
            Tags = new TagsViewModel(username, repository);
            ShowBranches = showBranches;

            Title = "Branches and Tags";
        }
    }
}

