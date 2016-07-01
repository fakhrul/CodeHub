using System;
using System.Threading.Tasks;
using CodeHub.Core.Filters;
using GitHubSharp.Models;
using CodeHub.Core.Messages;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;

namespace CodeHub.Core.ViewModels.Issues
{
    public class IssuesViewModel : BaseIssuesViewModel<IssuesFilterModel>
    {
        private IDisposable _addToken, _editToken;

        public string Username { get; private set; }

        public string Repository { get; private set; }

        public IReactiveCommand<object> GoToNewIssueCommand { get; } = ReactiveCommand.Create();

        public IssuesViewModel(string username, string repository)
        {
            Username = username;
            Repository = repository;

            GoToNewIssueCommand.Subscribe(_ => NavigateTo(new IssueAddViewModel(Username, Repository)));

            _issues = new FilterableCollectionViewModel<IssueModel, IssuesFilterModel>("IssuesViewModel:" + Username + "/" + Repository);
            _issues.GroupingFunction = Group;
            _issues.WhenAnyValue(x => x.Filter).Skip(1).InvokeCommand(LoadCommand);

            _addToken = Messenger.Subscribe<IssueAddMessage>(x =>
            {
                if (x.Issue == null || !DoesIssueBelong(x.Issue))
                    return;
                Issues.Items.Insert(0, x.Issue);
            });

            _editToken = Messenger.Subscribe<IssueEditMessage>(x =>
            {
                if (x.Issue == null || !DoesIssueBelong(x.Issue))
                    return;
                
                var item = Issues.Items.FirstOrDefault(y => y.Number == x.Issue.Number);
                if (item == null)
                    return;

                var index = Issues.Items.IndexOf(item);

                using (Issues.DeferRefresh())
                {
                    Issues.Items.RemoveAt(index);
                    Issues.Items.Insert(index, x.Issue);
                }
            });
        }

        protected override Task Load()
        {
            string direction = _issues.Filter.Ascending ? "asc" : "desc";
            string state = _issues.Filter.Open ? "open" : "closed";
            string sort = _issues.Filter.SortType == IssuesFilterModel.Sort.None ? null : _issues.Filter.SortType.ToString().ToLower();
            string labels = string.IsNullOrEmpty(_issues.Filter.Labels) ? null : _issues.Filter.Labels;
            string assignee = string.IsNullOrEmpty(_issues.Filter.Assignee) ? null : _issues.Filter.Assignee;
            string creator = string.IsNullOrEmpty(_issues.Filter.Creator) ? null : _issues.Filter.Creator;
            string mentioned = string.IsNullOrEmpty(_issues.Filter.Mentioned) ? null : _issues.Filter.Mentioned;
            string milestone = _issues.Filter.Milestone == null ? null : _issues.Filter.Milestone.Value;

            var request = this.GetApplication().Client.Users[Username].Repositories[Repository].Issues.GetAll(sort: sort, labels: labels, state: state, direction: direction, 
                                                                                          assignee: assignee, creator: creator, mentioned: mentioned, milestone: milestone);
            return Issues.SimpleCollectionLoad(request);
        }

        public void CreateIssue(IssueModel issue)
        {
            if (!DoesIssueBelong(issue))
                return;
            Issues.Items.Add(issue);
        }

        private bool DoesIssueBelong(IssueModel model)
        {
            if (Issues.Filter == null)
                return true;
            if (Issues.Filter.Open != model.State.Equals("open"))
                return false;
            return true;
        }

        public class NavObject
        {
            public string Username { get; set; }
            public string Repository { get; set; }
        }
    }
}

