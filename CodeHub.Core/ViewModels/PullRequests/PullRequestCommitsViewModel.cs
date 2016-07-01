using GitHubSharp.Models;
using GitHubSharp;
using System.Collections.Generic;
using CodeHub.Core.ViewModels.Changesets;

namespace CodeHub.Core.ViewModels.PullRequests
{
    public class PullRequestCommitsViewModel : CommitsViewModel
    {
        public long PullRequestId { get; }

        public PullRequestCommitsViewModel(string username, string repository, long pullRequestId)
            : base(username, repository)
        {
            PullRequestId = pullRequestId;
        }

        protected override GitHubRequest<List<CommitModel>> GetRequest()
        {
            return this.GetApplication().Client.Users[Username].Repositories[Repository].PullRequests[PullRequestId].GetCommits();
        }
    }
}

