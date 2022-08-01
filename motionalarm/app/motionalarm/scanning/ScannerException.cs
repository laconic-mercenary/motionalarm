
namespace app.motionalarm.scanning {

    public class ScannerException : System.Exception {

        public ScannerException(Scanner scanner)
            : base("A scanner error occured.") {
        }

        public ScannerException(Scanner scanner, string message)
            : base(message) {
        }

    }

}
