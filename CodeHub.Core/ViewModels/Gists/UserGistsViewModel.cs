using System.Collections.Generic;
using GitHubSharp;
using GitHubSharp.Models;
using CodeHub.Core.Messages;
using System;

namespace CodeHub.Core.ViewModels.Gists
{
    public class UserGistsViewModel : GistsViewModel
    {
        private readonly IDisposable _addToken;

        public string Username { get; }

        public bool IsMine
        {
            get { return this.GetApplication().Account.Username.Equals(Username); }
        }

        public UserGistsViewModel(string username)
        {
            Username = username ?? this.GetApplication().Account.Username;

            _addToken = Messenger.Subscribe<GistAddMessage>(x => Gists.Insert(0, x.Gist));

            //Assign some sort of title
            if (Username != null)
            {
                if (IsMine)
                    Title = "My Gists";
                else
                {
                    if (Username.EndsWith("s", System.StringComparison.Ordinal))
                        Title = Username + "' Gists";
                    else
                        Title = Username + "'s Gists";
                }
            }
            else
            {
                Title = "Gists";
            }
        }

        protected override GitHubRequest<List<GistModel>> CreateRequest()
        {
            return this.GetApplication().Client.Users[Username].Gists.GetGists();
        }
    }
}
