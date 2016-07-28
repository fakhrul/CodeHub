using CodeHub.Core.Data;
using GitHubSharp;
using System;
using Octokit;

namespace CodeHub.Core.Services
{
    public class ApplicationService : IApplicationService
    {
        public Client Client { get; private set; }
        public GitHubAccount Account { get; private set; }
        public IAccountsService Accounts { get; private set; }
        public GitHubClient GitHubClient { get; private set; }
        public Action ActivationAction { get; set; }


        public ApplicationService(IAccountsService accountsService)
        {
            Accounts = accountsService;
        }

        public void DeactivateUser()
        {
            Accounts.SetActiveAccount(null);
            Accounts.SetDefault(null);
            Account = null;
            Client = null;
            GitHubClient = null;
        }

        public void ActivateUser(GitHubAccount account, Client client)
        {
            Accounts.SetActiveAccount(account);
            Account = account;
            Client = client;

            //Set the default account
            Accounts.SetDefault(account);
        }

        public void SetUserActivationAction(Action action)
        {
            if (Account != null)
                action();
            else
                ActivationAction = action;
        }

    }
}
