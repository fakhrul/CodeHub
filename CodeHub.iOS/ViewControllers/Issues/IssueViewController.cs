using System;
using CodeHub.Core.ViewModels.Issues;
using UIKit;
using CodeHub.Utilities;
using CodeHub.DialogElements;
using System.Linq;
using System.Collections.Generic;
using Humanizer;
using CodeHub.ViewControllers.Repositories;
using CodeHub.Services;
using System.Reactive.Linq;
using ReactiveUI;
using CodeHub.WebViews;
using System.Threading.Tasks;

namespace CodeHub.ViewControllers.Issues
{
    public class IssueViewController : PrettyDialogViewController<IssueViewModel>
    {
        private readonly HtmlElement _descriptionElement = new HtmlElement("description");
        private readonly HtmlElement _commentsElement = new HtmlElement("comments");
        private StringElement _milestoneElement;
        private StringElement _assigneeElement;
        private StringElement _labelsElement;
        private StringElement _addCommentElement;
        private SplitButtonElement _split = new SplitButtonElement();
        private SplitButtonElement.Button _splitButton1;
        private SplitButtonElement.Button _splitButton2;

        public IssueViewController()
        {
        }

        public IssueViewController(string username, string repository, long id)
        {
            ViewModel = new IssueViewModel(username, repository, id);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _splitButton1 = _split.AddButton("Comments", "-");
            _splitButton2 = _split.AddButton("Participants", "-");

            HeaderView.SetImage(null, Images.Avatar);
            HeaderView.Text = Title;

            Appeared.Take(1)
                .Select(_ => Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Switch()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Select(_ => ViewModel.WhenAnyValue(x => x.IsClosed).Where(x => x.HasValue).Select(x => x.Value))
                .Switch()
                .Subscribe(x => 
                {
                    HeaderView.SubImageView.TintColor = x ? UIColor.FromRGB(0xbd, 0x2c, 0) : UIColor.FromRGB(0x6c, 0xc6, 0x44);
                    HeaderView.SetSubImage((x ? Octicon.IssueClosed :Octicon.IssueOpened).ToImage());
                });

            _milestoneElement = new StringElement("Milestone", "No Milestone", UITableViewCellStyle.Value1) {Image = Octicon.Milestone.ToImage() };
            _assigneeElement = new StringElement("Assigned", "Unassigned", UITableViewCellStyle.Value1) {Image = Octicon.Person.ToImage() };
            _labelsElement = new StringElement("Labels", "None", UITableViewCellStyle.Value1) {Image = Octicon.Tag.ToImage() };
            _addCommentElement = new StringElement("Add Comment") { Image = Octicon.Pencil.ToImage() };

            var actionButton = new UIBarButtonItem(UIBarButtonSystemItem.Action) { Enabled = false };
            NavigationItem.RightBarButtonItem = actionButton;

            ViewModel.WhenAnyValue(x => x.IsModifying).SubscribeStatus("Loading...");

            ViewModel.WhenAnyValue(x => x.Issue).IsNotNull().Subscribe(x =>
            {
                _assigneeElement.Value = x.Assignee != null ? x.Assignee.Login : "Unassigned";
                _milestoneElement.Value = x.Milestone != null ? x.Milestone.Title : "No Milestone";
                _labelsElement.Value = x.Labels.Count == 0 ? "None" : string.Join(", ", x.Labels.Select(i => i.Name));

                var model = new DescriptionModel(ViewModel.MarkdownDescription, (int)UIFont.PreferredSubheadline.PointSize, true);
                var markdown = new MarkdownView { Model = model };
                var html = markdown.GenerateString();
                _descriptionElement.SetValue(string.IsNullOrEmpty(ViewModel.MarkdownDescription) ? null : html);

                HeaderView.Text = x.Title;
                HeaderView.SubText = "Updated " + x.UpdatedAt.Humanize();
                HeaderView.SetImage(x.User?.AvatarUrl, Images.Avatar);
                RefreshHeaderView();

                Render();
            });

            ViewModel.BindCollection(x => x.Comments).Subscribe(_ => RenderComments());
            ViewModel.BindCollection(x => x.Events).Subscribe(_ => RenderComments());
            ViewModel.WhenAnyValue(x => x.ShouldShowPro).Subscribe(x => {
                if (x) this.ShowPrivateView(); 
            });

            OnActivation(d =>
            {
                _milestoneElement.Clicked.InvokeCommand(ViewModel.GoToMilestoneCommand).AddTo(d);
                _assigneeElement.Clicked.InvokeCommand(ViewModel.GoToAssigneeCommand).AddTo(d);
                _labelsElement.Clicked.InvokeCommand(ViewModel.GoToLabelsCommand).AddTo(d);
                _addCommentElement.Clicked.Subscribe(_ => AddCommentTapped()).AddTo(d);
                actionButton.GetClickedObservable().Subscribe(ShowExtraMenu).AddTo(d);
                HeaderView.Clicked.InvokeCommand(ViewModel.GoToOwner).AddTo(d);

                _commentsElement.UrlRequested
                    .Merge(_descriptionElement.UrlRequested)
                    .Select(x => new WebBrowserViewController(x))
                    .Subscribe(x => NavigationController.PushViewController(x, true))
                    .AddTo(d);

                ViewModel.WhenAnyValue(x => x.IsCollaborator).Subscribe(x => {
                    foreach (var i in new [] { _assigneeElement, _milestoneElement, _labelsElement })
                        i.Accessory = x ? UITableViewCellAccessory.DisclosureIndicator : UITableViewCellAccessory.None;
                }).AddTo(d);

                ViewModel.LoadCommand.IsExecuting.Subscribe(x => actionButton.Enabled = !x).AddTo(d);
            });
        }

        private static string CreateEventBody(string eventType, string commitId)
        {
            commitId = commitId ?? string.Empty;
            var smallCommit = commitId;
            if (string.IsNullOrEmpty(smallCommit))
                smallCommit = "Unknown";
            else if (smallCommit.Length > 7)
                smallCommit = commitId.Substring(0, 7);

            if (eventType == "closed")
                return "<p><span class=\"label label-danger\">Closed</span> this issue.</p>";
            if (eventType == "reopened")
                return "<p><span class=\"label label-success\">Reopened</span> this issue.</p>";
            if (eventType == "merged")
                return "<p><span class=\"label label-info\">Merged</span> commit " + smallCommit + "</p>";
            if (eventType == "referenced")
                return "<p><span class=\"label label-default\">Referenced</span> commit " + smallCommit + "</p>";
            return string.Empty;
        }

        public void RenderComments()
        {
            var comments = ViewModel.Comments
                .Select(x => new Comment(x.User.AvatarUrl, x.User.Login, ViewModel.ConvertToMarkdown(x.Body), x.CreatedAt))
                .Concat(ViewModel.Events.Select(x => new Comment(x.Actor.AvatarUrl, x.Actor.Login, CreateEventBody(x.Event, x.CommitId), x.CreatedAt)))
                .Where(x => !string.IsNullOrEmpty(x.Body))
                .OrderBy(x => x.Date)
                .ToList();
            var commentModel = new CommentModel(comments, (int)UIFont.PreferredSubheadline.PointSize);
            var razorView = new CommentsView { Model = commentModel };
            var html = razorView.GenerateString();

            InvokeOnMainThread(() => {
                _commentsElement.SetValue(!comments.Any() ? null : html);
                Render();
            });
        }

        protected virtual void Render()
        {
            //Wait for the issue to load
            if (ViewModel.Issue == null)
                return;

            var participants = ViewModel.Events.Select(x => x.Actor.Login).Distinct().Count();

            _splitButton1.Text = ViewModel.Issue.Comments.ToString();
            _splitButton2.Text = participants.ToString();

            ICollection<Section> sections = new LinkedList<Section>();
            sections.Add(new Section { _split });

            var secDetails = new Section();
            if (_descriptionElement.HasValue)
                secDetails.Add(_descriptionElement);


            secDetails.Add(_assigneeElement);
            secDetails.Add(_milestoneElement);
            secDetails.Add(_labelsElement);
            sections.Add(secDetails);

            var commentsSection = new Section();
            if (_commentsElement.HasValue)
                commentsSection.Add(_commentsElement);
            commentsSection.Add(_addCommentElement);
            sections.Add(commentsSection);

            Root.Reset(sections);
        }

        void AddCommentTapped()
        {
            var composer = new MarkdownComposerViewController();
            composer.NewComment(this, async (text) => {

                var hud = this.CreateHud();
                hud.Show("Posting Comment...");
                if (await ViewModel.AddComment(text))
                    composer.CloseComposer();
                hud.Hide();
                composer.EnableSendButton = true;
            });
        }

        public override UIView InputAccessoryView
        {
            get
            {
                var u = new UIView(new CoreGraphics.CGRect(0, 0, 320f, 27)) { BackgroundColor = UIColor.White };
                return u;
            }
        }

        private void ShowExtraMenu(UIBarButtonItem item)
        {
            if (ViewModel.Issue == null)
                return;

            var sheet = new UIActionSheet();
            var editButton = ViewModel.IsCollaborator ? sheet.AddButton("Edit") : -1;
            var openButton = ViewModel.IsCollaborator ? sheet.AddButton(ViewModel.Issue.State == "open" ? "Close" : "Open") : -1;
            var commentButton = sheet.AddButton("Comment");
            var shareButton = sheet.AddButton("Share");
            var showButton = sheet.AddButton("Show in GitHub");
            var cancelButton = sheet.AddButton("Cancel");
            sheet.CancelButtonIndex = cancelButton;
            sheet.Dismissed += (s, e) =>
            {
                BeginInvokeOnMainThread(() =>
                {
                    if (e.ButtonIndex == editButton)
                        ViewModel.GoToEditCommand.Execute(null);
                    else if (e.ButtonIndex == openButton)
                        ViewModel.ToggleStateCommand.Execute(null);
                    else if (e.ButtonIndex == shareButton)
                        AlertDialogService.ShareUrl(ViewModel.Issue?.HtmlUrl, NavigationItem.RightBarButtonItem);
                    else if (e.ButtonIndex == showButton)
                    {
                        var view = new WebBrowserViewController(ViewModel?.Issue?.HtmlUrl);
                        NavigationController.PushViewController(view, true);
                    }
                    else if (e.ButtonIndex == commentButton)
                        AddCommentTapped();
                });

                sheet.Dispose();
            };

            sheet.ShowFrom(item, true);
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            Task.WhenAll(_descriptionElement.ForceResize(), _commentsElement.ForceResize())
                .ToBackground(() => Root.Reload(new [] { _commentsElement, _descriptionElement }));
        }
    }
}

