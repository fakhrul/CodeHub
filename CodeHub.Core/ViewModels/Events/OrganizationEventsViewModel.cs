using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Events
{
    public class OrganizationEventsViewModel : BaseEventsViewModel
    {
        public string OrganizationName { get; }

        public string Username { get; }

        public OrganizationEventsViewModel(string username, string organizationName)
        {
            Username = username;
            OrganizationName = organizationName;
        }

        protected override GitHubRequest<List<EventModel>> CreateRequest(int page, int perPage)
        {
            return this.GetApplication().Client.Users[Username].GetOrganizationEvents(OrganizationName, page, perPage);
        }
    }
}
