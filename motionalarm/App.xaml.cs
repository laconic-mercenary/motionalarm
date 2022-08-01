namespace motionalarm {

    using System.Windows;

    /// <summary> Main Entry class. </summary>
    public partial class App : Application {

        /// <summary> Guaranteed to fire just before the window appears. </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e) {            
            base.OnStartup(e);

            // licensing
#if DEBUG
            // allow for full license here for debugging
#else
            // licensing stuff here
#endif

        } // OnStartup()

    } // App
}
