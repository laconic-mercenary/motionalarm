
namespace app.motionalarm.notification {

    public interface INotifier {
        void notify();
        void notifyAsync();
        bool isEnabled { get; set; }
    }

}
