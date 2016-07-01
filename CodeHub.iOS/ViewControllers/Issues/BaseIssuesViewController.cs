using GitHubSharp.Models;
using CodeHub.Core.ViewModels.Issues;
using CodeHub.DialogElements;
using System;

namespace CodeHub.ViewControllers.Issues
{
    public abstract class BaseIssuesViewController<TViewModel> : ViewModelCollectionDrivenDialogViewController<TViewModel>
        where TViewModel : class, IBaseIssuesViewModel
    {
        protected BaseIssuesViewController()
        {
            Title = "Issues";
        }

        protected IssueElement CreateElement(IssueModel x)
        {
            var weakVm = new WeakReference<IBaseIssuesViewModel>(ViewModel);
            var isPullRequest = x.PullRequest != null && !(string.IsNullOrEmpty(x.PullRequest.HtmlUrl));
            var assigned = x.Assignee != null ? x.Assignee.Login : "unassigned";
            var kind = isPullRequest ? "Pull" : "Issue";
            var commentString = x.Comments == 1 ? "1 comment" : x.Comments + " comments";
            var el = new IssueElement(x.Number.ToString(), x.Title, assigned, x.State, commentString, kind, x.UpdatedAt);
            el.Tapped += () => weakVm.Get()?.GoToIssueCommand.Execute(x);
            return el;
        }
    }
}

