
namespace app.motionalarm.streaming {

	using System.IO;
	using System.Security.AccessControl;

	static public class FileManipulator {

		/// <summary>
		/// Copies the file from one path to another.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="newPath"></param>
		public static void copyFile(string path, string newPath) {
			byte[] data = fileToRaw(path);
			saveFromRaw(data, newPath);
		}
		
		/// <summary>
		/// Deletes the file.
		/// </summary>
		/// <param name="path"></param>
		public static void deleteFile(string path) {
			File.Delete(path);
		}

		/// <summary>
		/// Recursively copyies all contents from one directory to a new directory.
		/// The new directory must already exist, and must have permissions to delete
		/// a file if it already exists before being copied to.
		/// </summary>
		/// <param name="currentDirectory"></param>
		/// <param name="newDirectory"></param>
		public static void copyDirectory(string currentDirectory, string newDirectory) {
			DirectoryInfo source = new DirectoryInfo(currentDirectory);
			DirectoryInfo target = new DirectoryInfo(newDirectory);
			foreach (DirectoryInfo dir in source.GetDirectories()) {
				copyDirectory(dir.FullName, target.CreateSubdirectory(dir.Name).FullName);
			}
			foreach (FileInfo fileInfo in source.GetFiles()) {
				string newFile = target.FullName + "\\" + fileInfo.Name;
				if (File.Exists(newFile)) {
					File.Delete(newFile); // assuming have permissions
				}
				fileInfo.CopyTo(newFile);
			}
		}

		/// <summary>
		/// Converts a file to a byte array.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static byte[] fileToRaw(string path) {
			return File.ReadAllBytes(path);
		}

		/// <summary>
		/// Sets the file's readonly attribute.
		/// </summary>
		/// <param name="path"></param>
		public static void setReadonly(string path) {
			File.SetAttributes(path, System.IO.FileAttributes.ReadOnly);
		}

		/// <summary>
		/// Adds an access rule to a file.  
		/// </summary>
		/// <param name="path">Full path to existing file.</param>
		/// <param name="account">(EX: @"Domain\AccountName")</param>
		/// <param name="rights">The rights to assign</param>
		/// <param name="controlType">Allow or Deny them this right.</param>
		public static void addFileSecurity(string path, string account, FileSystemRights rights, AccessControlType controlType) {
			FileSecurity fileSecurity = File.GetAccessControl(path);
			fileSecurity.AddAccessRule(new FileSystemAccessRule(account, rights, controlType));			
			File.SetAccessControl(path, fileSecurity);
		}

		/// <summary>
		/// Removes an existing access rule to a file.
		/// </summary>
		/// <param name="path">Full path to existing file.</param>
		/// <param name="account">(EX: @"Domain\AccountName")</param>
		/// <param name="rights">The rights to remove.</param>
		/// <param name="controlType">Allow or Deny, I think this doesn't matter.</param>
		public static void removeFileSecurity(string path, string account, FileSystemRights rights, AccessControlType controlType) {
			FileSecurity fileSecurity = File.GetAccessControl(path);
			fileSecurity.RemoveAccessRule(new FileSystemAccessRule(account, rights, controlType));
			File.SetAccessControl(path, fileSecurity);
		}

		/// <summary>
		/// Saves a file from a raw byte array.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="savePath"></param>
		/// <param name="errorIfExists"></param>
		public static void saveFromRaw(byte[] data, string savePath, bool errorIfExists = false) {
			FileStream writer = File.Open(
				savePath, 
				errorIfExists ? System.IO.FileMode.CreateNew : System.IO.FileMode.Create
			);
			BinaryWriter binWriter = new BinaryWriter(writer);
			binWriter.Write(data);
			binWriter.Close();
			binWriter.Dispose();
			writer.Close();
			writer.Dispose();
		}
		
	}

}
