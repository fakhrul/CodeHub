using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public class RepositoryEventsViewModel : BaseEventsViewModel
    {
        public string Repository { get; }

        public string Username { get; }

        public RepositoryEventsViewModel(string username, string repository)
        {
            Username = username;
            Repository = repository;
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
            return this.GetApplication().Client.Users[Username].Repositories[Repository].GetEvents(page, perPage);
        }
    }
}