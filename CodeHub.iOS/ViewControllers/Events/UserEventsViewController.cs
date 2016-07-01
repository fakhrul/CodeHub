using CodeHub.Core.ViewModels.Events;

namespace CodeHub.ViewControllers.Events
{
    public class UserEventsViewController : BaseEventsViewController<UserEventsViewModel>
    {
        public UserEventsViewController()
        {
        }

        public UserEventsViewController(string username)
        {
            ViewModel = new UserEventsViewModel(username);
        }
    }
}