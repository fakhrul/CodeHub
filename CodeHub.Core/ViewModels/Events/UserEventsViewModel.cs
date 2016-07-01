using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public class UserEventsViewModel : BaseEventsViewModel
    {
        public string Username { get; }

        public UserEventsViewModel(string username)
        {
            Username = username;
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
            return this.GetApplication().Client.Users[Username].GetEvents(page, perPage);
        }
    }
}
