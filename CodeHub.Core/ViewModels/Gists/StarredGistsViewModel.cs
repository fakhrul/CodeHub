using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Gists
{
    public class StarredGistsViewModel : GistsViewModel
    {
        public StarredGistsViewModel()
        {
            Title = "Starred Gists";
        }

        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
            return this.GetApplication().Client.Gists.GetStarredGists();
        }
    }
}
