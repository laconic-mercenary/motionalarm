
namespace app.motionalarm.scanning {

    public class ThresholdBrokenEventArgs {
        public System.DateTime timeOccured { get; set; }
        public object imageFrame { get; set; }
        public float maxThreshold { get; set; }
        public float currentThreshold { get; set; }
    }

}
