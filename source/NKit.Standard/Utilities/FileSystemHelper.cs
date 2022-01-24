namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.IO.Compression;

    #endregion //Using Directives

    public partial class FileSystemHelper
    {
        #region Methods

        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION = 33;

        public static bool IsFileLocked(Exception exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION;
        }

        public static void ValidateFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(string.Format("Could not find {0}.", filePath));
            }
        }

        public static void ValidateFileExists(string directoryPath, string fileName)
        {
            ValidateFileExists(Path.Combine(directoryPath, fileName));
        }

        public static void ValidateDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(string.Format("Could not find {0}.", directoryPath));
            }
        }

        public static byte[] GetFileBytes(string filePath)
        {
            ValidateFileExists(filePath);
            using (FileStream fs = File.OpenRead(filePath))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.SetLength(fs.Length);
                    fs.Read(ms.GetBuffer(), 0, (int)fs.Length);
                    return ms.ToArray();
                }
            }
        }

        public static void WriteXmlFile(string outputFilePath, string xmlContents, bool indent, string indentChars)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xmlContents);
            using (XmlWriter writer = XmlWriter.Create(outputFilePath, new XmlWriterSettings() { Indent = indent, IndentChars = indentChars }))
            {
                document.Save(writer);
                writer.Close();
            }
        }

        public static void CompressFileToZip(string inputFilePath, string outputFilePath)
        {
            ValidateFileExists(inputFilePath);
            string parentDirectory = Path.GetDirectoryName(inputFilePath);
            string inputFileName = Path.GetFileName(inputFilePath);
            ValidateDirectoryExists(parentDirectory);

            string zipDirectoryName = Guid.NewGuid().ToString();
            string zipDirectoryPath = Path.Combine(parentDirectory, zipDirectoryName);
            string fileToBeZippedPath = Path.Combine(zipDirectoryPath, inputFileName);
            if (!Directory.Exists(zipDirectoryPath))
            {
                Directory.CreateDirectory(zipDirectoryPath);
            }
            File.Copy(inputFilePath, fileToBeZippedPath);
            ValidateFileExists(fileToBeZippedPath);
            if (File.Exists(outputFilePath))
            {
                File.Delete(outputFilePath);
            }
            ZipFile.CreateFromDirectory(zipDirectoryPath, outputFilePath);
            Directory.Delete(zipDirectoryPath, true);
        }

        public static void DecompressFileToZip(string zipFilePath, string outputDirectory)
        {
            ZipFile.ExtractToDirectory(zipFilePath, outputDirectory);
        }


        public static bool DeleteDirectoryRecursive(DirectoryInfo baseDirectoryPath)
        {
            return DeleteDirectoryRecursive(baseDirectoryPath, false, out string exceptionMessage);
        }

        /// <summary>
        /// Deletes a directory recursively: all sub directory and all files within it.
        /// If a file within the directory or sub-directory is marked as read-only it will be edit it as not read-only and then deleted.
        /// </summary>
        public static bool DeleteDirectoryRecursive(DirectoryInfo baseDirectoryPath, bool swallowExceptions, out string exceptionMessage)
        {
            exceptionMessage = string.Empty;
            try
            {
                if (!baseDirectoryPath.Exists)
                {
                    return false;
                }
                foreach (DirectoryInfo dir in baseDirectoryPath.EnumerateDirectories())
                {
                    DeleteDirectoryRecursive(dir);
                }
                var files = baseDirectoryPath.GetFiles();
                foreach (var file in files)
                {
                    DeleteFileForce(file, swallowExceptions, out exceptionMessage);
                }
                baseDirectoryPath.Delete();
                return !baseDirectoryPath.Exists;
            }
            catch (Exception ex)
            {
                if (!swallowExceptions)
                {
                    throw ex;
                }
                exceptionMessage = ex.Message;
                return false;
            }
        }

        public static bool DeleteFileForce(FileInfo file)
        {
            return DeleteFileForce(file, false, out string exceptionMessage);
        }

        /// <summary>
        /// Deletes a file and if it's set to read-only it will edit it as not read-only and then deleted.
        /// </summary>
        public static bool DeleteFileForce(FileInfo file, bool swallowExceptions, out string exceptionMessage)
        {
            exceptionMessage = string.Empty;
            try
            {
                if (!file.Exists)
                {
                    return false;
                }
                if (file.IsReadOnly)
                {
                    file.IsReadOnly = false;
                }
                file.Delete();
                return !file.Exists;
            }
            catch (Exception ex)
            {
                if (!swallowExceptions)
                {
                    throw ex;
                }
                exceptionMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Determines if the given filepath is an absolute file path and if not it returns the absolute path relative to the current executing directory.
        /// If it is an absolute file path it it returns the file path unchanged.
        /// </summary>
        public static string GetAbsoluteFilePath(string filePath)
        {
            if (Path.IsPathRooted(filePath))
            {
                return filePath;
            }
            return Path.Combine(Information.GetExecutingDirectory(), filePath);
        }

        #endregion //Methods
    }
}