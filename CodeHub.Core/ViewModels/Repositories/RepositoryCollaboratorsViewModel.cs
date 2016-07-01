using System.Threading.Tasks;
using GitHubSharp.Models;

namespace CodeHub.Core.ViewModels.Repositories
{
    public class RepositoryCollaboratorsViewModel : LoadableViewModel
    {
        public CollectionViewModel<BasicUserModel> Collaborators { get; } = new CollectionViewModel<BasicUserModel>();

        public string Username { get; }

        public string Repository { get; }

        public RepositoryCollaboratorsViewModel(string username, string repository) 
        {
            Username = username;
            Repository = repository;
        }

        protected override Task Load()
        {
            return Collaborators.SimpleCollectionLoad(this.GetApplication().Client.Users[Username].Repositories[Repository].GetCollaborators());
        }
    }
}

