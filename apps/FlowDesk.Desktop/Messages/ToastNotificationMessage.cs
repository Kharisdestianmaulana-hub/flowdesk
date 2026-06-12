using CommunityToolkit.Mvvm.Messaging.Messages;

namespace FlowDesk.Desktop.Messages;

public class ToastNotificationMessage : ValueChangedMessage<string>
{
    public ToastNotificationMessage(string message) : base(message)
    {
    }
}
