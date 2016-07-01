using GitHubSharp.Models;

namespace CodeHub.Core.Messages
{
    public class GistAddMessage
    {
        public GistModel Gist { get; }

        public GistAddMessage(GistModel gist) 
        {
            Gist = gist;
        }
    }
}

