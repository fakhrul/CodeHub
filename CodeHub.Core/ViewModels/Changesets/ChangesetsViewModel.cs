using GitHubSharp;
using System.Collections.Generic;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Changesets
{
    public class ChangesetsViewModel : CommitsViewModel
    {
        public string Branch { get; }

        public ChangesetsViewModel(string username, string repository, string branch)
            : base(username, repository)
        {
            Branch = branch;
        }

        protected override GitHubRequest<List<CommitModel>> GetRequest()
        {
            return this.GetApplication().Client.Users[Username].Repositories[Repository].Commits.GetAll(Branch);
        }
    }
}

