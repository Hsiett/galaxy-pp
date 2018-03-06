using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;

namespace Galaxy_Editor_2
{
    class MpqEditor
    {
        [DllImport("StormLib.dll")]
        private static extern bool SFileOpenArchive(
          byte[] szMpqName,           // Archive file name
          UInt32 dwPriority,                 // Archive priority
          UInt32 dwFlags,                    // Open flags
          ref UInt32 phMPQ                    // Pointer to result HANDLE
        );

        [DllImport("StormLib.dll")]
        private static extern bool SFileCreateFile(
          UInt32 hMpq,                      // Handle to the MPQ
          byte[] szArchivedName,            // The name under which the file will be stored
          ulong FileTime,                // Specifies the date and file time
          UInt32 dwFileSize,                 // Specifies the size of the file
          UInt32 lcLocale,                     // Specifies the file locale
          UInt32 dwFlags,                     // Specifies archive flags for the file
          ref UInt32 phFile                  // Returned file handle
        );

        [DllImport("StormLib.dll")]
        private static extern bool SFileWriteFile(
          UInt32 hFile,                     // Handle to the file
          IntPtr pvData,              // Pointer to data to be written
          UInt32 dwSize,                     // Size of the data pointed by pvData
          UInt32 dwCompression               // Specifies compression of the data block
        );

        [DllImport("StormLib.dll")]
        private static extern bool SFileFinishFile(
            UInt32 hFile // Handle to the file
        );
        [DllImport("StormLib.dll")]
        private static extern bool  SFileFlushArchive(
          UInt32 hMpq                       // Handle to an open MPQ
        );

        [DllImport("StormLib.dll")]
        private static extern bool SFileCloseArchive(
          UInt32 hMpq                       // Handle to an open MPQ
        );

        
        

        

        [DllImport("StormLib.dll")]
        private static extern UInt32 SFileFindFirstFile(
          UInt32 hMpq,                      // Archive handle
          byte[] szMask,              // Search mask
          ref SFILE_FIND_DATA lpFindFileData, // Pointer to the search result
          UInt32 szListFile           // Name of additional listfile
        );

        [DllImport("StormLib.dll")]
        private static extern bool SFileFindNextFile(
          UInt32 hFind,                     // Find handle
          ref SFILE_FIND_DATA  lpFindFileData  // Pointer to the search result
        );

        struct SFILE_FIND_DATA
        {
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;              // Name of the found file
            public IntPtr szPlainName;                      // Plain name of the found file
            public UInt32 dwHashIndex;                      // Hash table index for the file
            public UInt32 dwBlockIndex;                     // Block table index for the file
            public UInt32 dwFileSize;                       // Uncompressed size of the file, in bytes
            public UInt32 dwFileFlags;                      // MPQ file flags
            public UInt32 dwCompSize;                       // Compressed file size
            public UInt32 dwFileTimeLo;                     // Low 32-bits of the file time (0 if not present)
            public UInt32 dwFileTimeHi;                     // High 32-bits of the file time (0 if not present)
            public UInt32 lcLocale;                         // Locale version
        };

        [DllImport("StormLib.dll")]
        private static extern bool SFileRemoveFile(
          UInt32 hMpq,                      // Handle to the MPQ
          byte[] szFileName,          // The name of a file to be removed
          UInt32 dwSearchScope               // Specifies search scope for the file
        );

        [DllImport("StormLib.dll")]
        private static extern bool SFileFindClose(
          UInt32 hFind                      // Find handle
        );

        [DllImport("StormLib.dll")]
        private static extern bool SFileOpenFileEx(
          UInt32 hMpq,                      // Archive handle
          byte[] szFileName,          // Name of the file to open
          UInt32 dwSearchScope,              // Specifies the scope for the file.
          ref UInt32 phFile                   // Pointer to file handle
        );
        
        /*[DllImport("StormLib.dll")]
        private static extern bool SFileReadFile(
          UInt32 hFile,                     // File handle
          ref byte[] lpBuffer,                  // Pointer to buffer where to read the data
          UInt32 dwToRead,                   // Number of bytes to read
          ref UInt32 pdwRead,                  // Pointer to variable that receivs number of bytes read
          UInt32 lpOverlapped         // Pointer to OVERLAPPED structure
        );*/


        [DllImport("StormLib.dll")]
        private static extern unsafe int SFileReadFile(
          UInt32 hFile,                     // File handle
          void * lpBuffer,                  // Pointer to buffer where to read the data
          UInt32 dwToRead,                   // Number of bytes to read
          UInt32 * pdwRead,                  // Pointer to variable that receivs number of bytes read
          void * lpOverlapped         // Pointer to OVERLAPPED structure
        );

        [DllImport("StormLib.dll")]
        private static extern unsafe bool SFileWriteFile(
          UInt32 hFile,                     // Handle to the file
          void * pvData,              // Pointer to data to be written
          UInt32 dwSize,                     // Size of the data pointed by pvData
          UInt32 dwCompression               // Specifies compression of the data block
        );

        [DllImport("StormLib.dll")]
        private static extern bool SFileCloseFile(
          UInt32 hFile                      // File handle
        );
        
        /*[DllImport("StormLib.dll")]
        private static extern bool SFileExtractFile(
          UInt32 hMpq,                      // Handle to a file or archive
          ref byte[] szToExtract,         // Name of the file to extract
          ref byte[] szExtracted          // Name of local file
        );*/

        [DllImport("StormLib.dll")]
        private static extern unsafe bool SFileExtractFile(
          UInt32 hMpq,                      // Handle to a file or archive
          char * szToExtract,         // Name of the file to extract
          char *  szExtracted,          // Name of local file
          UInt32 searchScope
        );
        
        [DllImport("StormLib.dll")]
        private static extern bool SFileCreateArchive(
          byte[] szMpqName,           // Archive file name
          UInt32 dwFlags,                    // Additional flags to specify creation details
          UInt32 dwMaxFileCount,             // Limit for file count
          ref UInt32 phMPQ                    // Pointer to result HANDLE
        );

        /*[DllImport("StormLib.dll",
      CallingConvention = CallingConvention.StdCall,
      EntryPoint = "SFileExtractFile",
      ExactSpelling = false)]
        private static extern bool SFileExtractFile(
          UInt32 hMpq,                      // Handle to a file or archive
          IntPtr szToExtract,         // Name of the file to extract
          IntPtr szExtracted          // Name of local file
        );*/

        [DllImport("StormLib.dll")]
        private static extern int SFileSetMaxFileCount(
          UInt32 hMpq,                    // Handle to an open archive
          UInt32 dwMaxFileCount            // New limit of file count in the MPQ
        );
        //windywell
        /*
        1,delete trigger.version
        2,change the InitMap() to GalaxyPPInitMap()
        3,identify:
          <Identifier>GalaxyPPInitMap</Identifier>
          <ScriptCode>
           B
          </ScriptCode>
          <InitFunc>GalaxyPPInitMap</InitFunc>
          in the trigger file. set B=source in MapScript.galaxy.
        */
        public static bool CopyOutputToMap(FileInfo rootFile, FileInfo map, Form1 owner,bool compatible)
        {
            MakeBackups(map, owner);



            UInt32 mpqHandle = 0;
            bool success = SFileOpenArchive(ToByteArray(map.FullName), 0, 0, ref mpqHandle);
            if (!success)
            {
                MessageBox.Show("Unable to open the map.", "Error");
                return false;
            }

            //Remove all galaxy files
            List<string> filesToDelete = new List<string>();
            SFILE_FIND_DATA fileData = default(SFILE_FIND_DATA);
            string[] fileDeletePatterns = new string[] { "*.galaxy", "BankList.xml", "Triggers"};
            foreach (string pattern in fileDeletePatterns)
            {
                success = true;
                UInt32 searchHandle = SFileFindFirstFile(mpqHandle, ToByteArray(pattern), ref fileData, 0);
                while (searchHandle != 0)
                {
                    if (!success)
                    {
                        //Some stop condition here...
                        SFileFindClose(searchHandle);
                        break;
                    }
                    filesToDelete.Add(fileData.cFileName);
                    success = SFileFindNextFile(searchHandle, ref fileData);
                }
                continue;
            }

            foreach (string file in filesToDelete)
            {
                success = SFileRemoveFile(mpqHandle, ToByteArray(file), 0);
            }

            //after remove, alter files in Triggers
            //Galaxy_Editor_2.Compiler.TriggerLoader.CopyMapToTriggers();

            success = CopyFiles(owner.openProjectOutputDir, owner.openProjectOutputDir, rootFile, mpqHandle);

            SFileFlushArchive(mpqHandle);
            SFileCloseArchive(mpqHandle);
            return success;
        }
        //
        public static bool CopyOutputToMap(FileInfo rootFile, FileInfo map, Form1 owner)
        {
            /*if (true) { 
                CopyOutputToMap(rootFile, map, owner,true);
                return true;
            }*/
            //Backup map
            MakeBackups(map, owner);



            UInt32 mpqHandle = 0;
            bool success = SFileOpenArchive(ToByteArray(map.FullName), 0, 0, ref mpqHandle);
            if (!success)
            {
                MessageBox.Show("Unable to open the map.", "Error");
                return false;
            }

            //Remove all galaxy files
            List<string> filesToDelete = new List<string>();
            SFILE_FIND_DATA fileData = default(SFILE_FIND_DATA);
            string[] fileDeletePatterns = new string[] { "*.galaxy", "BankList.xml", "Triggers", "Triggers.version", "*TriggerStrings.txt"};
            foreach (string pattern in fileDeletePatterns)
            {
                success = true;
                UInt32 searchHandle = SFileFindFirstFile(mpqHandle, ToByteArray(pattern), ref fileData, 0);
                while (searchHandle != 0)
                {
                    if (!success)
                    {
                        //Some stop condition here...
                        SFileFindClose(searchHandle);
                        break;
                    }
                    filesToDelete.Add(fileData.cFileName);
                    success = SFileFindNextFile(searchHandle, ref fileData);
                }
                continue;
            }

            foreach (string file in filesToDelete)
            {
                success = SFileRemoveFile(mpqHandle, ToByteArray(file), 0);
            }

            success = CopyFiles(owner.openProjectOutputDir, owner.openProjectOutputDir, rootFile, mpqHandle);

            SFileFlushArchive(mpqHandle);
            SFileCloseArchive(mpqHandle);
            return success;
        }

        private static bool CopyFiles(DirItem item, FolderItem outputDir, FileInfo rootFile, UInt32 mpqHandle)
        {
            bool success = true;
            if (item is FolderItem)
            {
                FolderItem dir = (FolderItem)item;
                foreach (DirItem nextItem in dir.Children)
                {
                    success &= CopyFiles(nextItem, outputDir, rootFile, mpqHandle);
                }
            }
            if (item is FileItem)
            {
                FileItem file = (FileItem)item;
                string fileName = file.File.FullName.Remove(0, outputDir.Dir.FullName.Length + 1);
                if (file.File.FullName == rootFile.FullName)
                    fileName = "MapScript.galaxy";
                else if (fileName.ToLower() == "mapscript.galaxy")
                {
                    MessageBox.Show("You cannot name a file other than the root file as MapScript.galaxy", "Error");
                    return false;
                }
                List<byte> byteList = new List<byte>();
                Stream stream = file.File.OpenRead();
                int b;
                while ((b = stream.ReadByte()) != -1)
                {
                    byteList.Add((byte) b);
                }
                byte[] bytes = byteList.ToArray();
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
                // Call unmanaged code
               // byte[] bytes = ToByteArray(stream.ReadToEnd());
                /*StreamReader stream = file.OpenText();
                UTF8Encoding encoding = new System.Text.UTF8Encoding();
                byte[] bytes = encoding.GetBytes(stream.ReadToEnd());*/
                stream.Close();



                UInt32 fileHandle = 0;
                bool fSuccess = SFileCreateFile(mpqHandle, ToByteArray(fileName), (ulong)DateTime.Now.Ticks, (uint)bytes.Length, 0, 0x200, ref fileHandle);
                if (fSuccess)
                    fSuccess = SFileWriteFile(fileHandle, unmanagedPointer, (uint)bytes.Length, 0x12);
                if (fSuccess)
                    fSuccess = SFileFinishFile(fileHandle);

                if (!fSuccess)
                {
                    MessageBox.Show("Unable to copy " + fileName + " to the map.\n\nThere are most likely too many files.\nTry setting the \"Join output to one file\" flag in the options menu.", "Error");
                }

                success &= fSuccess;
                Marshal.FreeHGlobal(unmanagedPointer);

            }
            return success;
        }

        private static byte[] ToByteArray(string s)
        {
            byte[] bytes = new byte[s.Length];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)s[i];
            }
            return bytes;
        }

        public static void CopyTo(FileSystemInfo map, string path)
        {
            if (map is FileInfo)
                ((FileInfo)map).CopyTo(path, true);
            else //map is DirectoryInfo
                Form1.CopyDirectories((DirectoryInfo) map,
                                      new DirectoryInfo(path));
        }

        private static void MakeBackups(FileSystemInfo map, Form1 owner)
        {
            if (Options.Compiler.NumberOfMapBackups > 0)
            {
                List<FileSystemInfo> currentBackups = new List<FileSystemInfo>();

                // Add backup of normal sc2 map files
                foreach (string file in Directory.GetFiles(owner.openProjectDir.FullName, "*.Sc2Map"))
                {
                    currentBackups.Add(new FileInfo(file));
                }

                // Add backup directories of sc2 component style maps
                foreach (string directory in Directory.GetDirectories(owner.openProjectDir.FullName, "*.Sc2Map"))
                {
                    currentBackups.Add(new DirectoryInfo(directory));
                }

                currentBackups.Sort((a, b) => a.Name.CompareTo(b.Name));

                // Check for previous backups
                while (currentBackups.Count > 0 && currentBackups.Count >= Options.Compiler.NumberOfMapBackups)
                {
                    // There are previous backups. find the oldest one
                    FileSystemInfo oldest = currentBackups[0];

                    // Remove the backup
                    if (oldest is DirectoryInfo)
                    {
                        ((DirectoryInfo) oldest).Delete(true);
                    }
                    else
                    {
                        oldest.Delete();
                    }
                    currentBackups.Remove(oldest);
                }

                // Create backup
                string newBackupPath = 
                    owner.openProjectDir.FullName +
                    @"\Backup" +
                    (ulong)(DateTime.Now - new DateTime(0)).TotalSeconds +
                    ".SC2Map";
                CopyTo(map, newBackupPath);
            }
        }

        public static bool CopyOutputToMap(FileInfo rootFile, DirectoryInfo map, Form1 owner)
        {
            try
            {

                //Backup map
                MakeBackups(map, owner);

                //Remove all galaxy files
                DeleteFiles(map);

                CopyFiles(owner.openProjectOutputDir, owner.openProjectOutputDir, rootFile, map);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to copy map. Is any files in the target directory in use?", "Error");
                return false;
            }
            return true;
        }

        private static void DeleteFiles(DirectoryInfo dir)
        {
            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.FullName.EndsWith(".galaxy") ||
                    file.Name == "BankList.xml" ||
                    file.Name ==  "Triggers" ||
                    file.Name ==  "Triggers.version" ||
                    file.Name ==  "*TriggerStrings.txt")
                {
                    file.Delete();
                }
            }
            foreach (DirectoryInfo directory in dir.GetDirectories())
            {
                DeleteFiles(directory);
            }
        }

        private static void CopyFiles(DirItem item, FolderItem outputDir, FileInfo rootFile, DirectoryInfo targetDir)
        {
            if (item is FolderItem)
            {
                FolderItem dir = (FolderItem)item;
                DirectoryInfo childDir = dir == outputDir ? targetDir : targetDir.CreateSubdirectory(dir.Text);
                foreach (DirItem nextItem in dir.Children)
                {
                    CopyFiles(nextItem, outputDir, rootFile, childDir);
                }
            }
            if (item is FileItem)
            {
                FileItem file = (FileItem)item;
                string fileName = file.File.Name;
                if (file.File.FullName == rootFile.FullName)
                    fileName = "MapScript.galaxy";
                else if (fileName.ToLower() == "mapscript.galaxy")
                {
                    MessageBox.Show("You cannot name a file other than the root file as MapScript.galaxy", "Error");
                    return;
                }

                file.File.CopyTo(Path.Combine(targetDir.FullName, fileName), true);
            }
        }

        public static bool CopyOutputToMap(FileInfo rootFile, FileSystemInfo outputMap, Form1 owner)
        {
            if (outputMap is FileInfo)
                return CopyOutputToMap(rootFile, (FileInfo) outputMap, owner);
            return CopyOutputToMap(rootFile, (DirectoryInfo) outputMap, owner);
        }

        public static bool Copy(FileInfo source, DirectoryInfo dest)
        {
            UInt32 mpqHandle = 0;
            bool success = SFileOpenArchive(ToByteArray(source.FullName), 0, 0, ref mpqHandle);
            if (!success)
            {
                MessageBox.Show("Unable to open the source map for copying.", "Error");
                return false;
            }

            //Remove all galaxy files
            List<string> files = new List<string>();
            SFILE_FIND_DATA fileData = default(SFILE_FIND_DATA);
            
            success = true;
            UInt32 searchHandle = SFileFindFirstFile(mpqHandle, ToByteArray("*"), ref fileData, 0);
            while (searchHandle != 0)
            {
                if (!success)
                {
                    //Some stop condition here...
                    SFileFindClose(searchHandle);
                    break;
                }
                files.Add(fileData.cFileName);
                success = SFileFindNextFile(searchHandle, ref fileData);
            }


            success = true;
            string currentFile = "";
            foreach (string file in files)
            {
                currentFile = file;
                string path = Path.Combine(dest.FullName, file);
                //byte[] s1 = ToByteArray(file);
                //byte[] s2 = ToByteArray(path);
                /*unsafe
                {
                    fixed (char * s1 = file)
                    {
                        fixed (char * s2 = path)
                        {
                            success |= SFileExtractFile(mpqHandle, s1, s2, 0);
                        }
                    }
                }*/
            
                //success |= SFileExtractFile(mpqHandle, ref s1, ref s2);


                UInt32 hFile = 0;
                success |= SFileOpenFileEx(mpqHandle, ToByteArray(file), 0, ref hFile);
                if (!success) break;
                FileInfo destFile = new FileInfo(Path.Combine(dest.FullName, file));
                CreateDir(destFile.Directory);
                FileStream stream = destFile.Open(FileMode.Create, FileAccess.Write);
                unsafe
                {
                    void* buffer = Marshal.AllocHGlobal(256).ToPointer(); 
                    UInt32* bytesRead = (uint*) Marshal.AllocHGlobal(sizeof(UInt32)).ToPointer();
                    while (true)
                    {
                        success |= SFileReadFile(hFile, buffer, 256, bytesRead, null) != 0;
                        
                        if (!success) break;
                        if (*bytesRead == 0) break;
                        byte[] managedBuffer = new byte[256];
                        byte* p = (byte*) buffer;
                        for (int i = 0; i < *bytesRead; i++)
                        {
                            managedBuffer[i] = *p;
                            p++;
                        }
                        stream.Write(managedBuffer, 0, (int)*bytesRead);
                    }
                    Marshal.FreeHGlobal(new IntPtr(buffer));
                    Marshal.FreeHGlobal(new IntPtr(bytesRead));
                }
                stream.Close();
                if (!success) break;
                success |= SFileCloseFile(hFile);
            }
            SFileCloseArchive(mpqHandle);
            return success;
        }

        private static void CreateDir(DirectoryInfo dir)
        {
            if (dir.Exists)
                return;
            CreateDir(dir.Parent);
            dir.Create();
        }

        public static bool Copy(DirectoryInfo source, FileInfo dest)
        {
            if (dest.Exists) dest.Delete();
            else CreateDir(dest.Directory);
            //Create archive
            /*unsafe
            {*/
            UInt32 hMpq = 0;
            bool success = true;
            try
            {
                success &= SFileCreateArchive(ToByteArray(dest.FullName), 0x20001, 128, ref hMpq);
                    
                if (!success) return false;
                Copy(source, source, hMpq);
            }
            finally
            {//Free memory
                if (hMpq != 0)
                {
                    SFileFlushArchive(hMpq);
                    SFileCloseArchive(hMpq);
                }

            }
            return success;
        }

        private static bool Copy(DirectoryInfo root, DirectoryInfo source, UInt32 hMpq)
        {
            bool success = true;
            
            


            foreach (FileInfo file in source.GetFiles())
            {
                string fileName = file.FullName.Remove(0, root.FullName.Length + 1);

                UInt32 fileHandle = 0;
                bool fSuccess = SFileCreateFile((uint)hMpq, ToByteArray(fileName), (ulong)DateTime.Now.Ticks, (uint)file.Length, 0, 0x200, ref fileHandle);
                byte[] bytes = new byte[256];
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
                Stream stream = file.OpenRead();
                int read;
                while ((read = stream.Read(bytes, 0, bytes.Length)) > 0)
                {
                    Marshal.Copy(bytes, 0, unmanagedPointer, read);
                    fSuccess &= SFileWriteFile(fileHandle, unmanagedPointer, (uint)read, 0x12);

                }
                fSuccess &= SFileFinishFile(fileHandle);
                // Call unmanaged code
                // byte[] bytes = ToByteArray(stream.ReadToEnd());
                /*StreamReader stream = file.OpenText();
                UTF8Encoding encoding = new System.Text.UTF8Encoding();
                byte[] bytes = encoding.GetBytes(stream.ReadToEnd());*/
                stream.Close();



                
                /*if (fSuccess)
                    fSuccess = SFileWriteFile(fileHandle, unmanagedPointer, (uint)bytes.Length, 0x12);
                if (fSuccess)
                    fSuccess = SFileFinishFile(fileHandle);*/

                if (!fSuccess)
                {
                    MessageBox.Show("Unable to copy " + fileName + " to the map.\n\nThere are most likely too many files.\nTry setting the \"Join output to one file\" flag in the options menu.", "Error");
                }

                success &= fSuccess;
                Marshal.FreeHGlobal(unmanagedPointer);
            }

            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                Copy(root, dir, hMpq);
            }
            return success;
        }

        public static bool SaveGalaxyppScriptFiles(FileInfo file, DirectoryInfo srcDir, bool showErrors = true)
        {
            UInt32 hMpq = 0;
            bool success;
            try
            {
                success = SFileOpenArchive(ToByteArray(file.FullName), 0, 0, ref hMpq);
                if (!success)
                {
                    if (showErrors)
                        MessageBox.Show("Unable to save the galaxy++ files to the map. Is it open in another program?",
                                        "Error");
                    return false;
                }
                //Remove old Galaxy++\\ dir
                List<string> files = new List<string>();
                SFILE_FIND_DATA fileData = default(SFILE_FIND_DATA);
                UInt32 searchHandle = SFileFindFirstFile(hMpq, ToByteArray("*"), ref fileData, 0);
                UInt32 count = 0;
                while (searchHandle != 0)
                {
                    if (!success)
                    {
                        //Some stop condition here...
                        SFileFindClose(searchHandle);
                        break;
                    }

                    if (fileData.cFileName.StartsWith("Galaxy++\\") && (fileData.cFileName.EndsWith(".galaxy++") || fileData.cFileName.EndsWith(".Dialog")))
                        files.Add(fileData.cFileName);
                    else
                        count++;
                    success = SFileFindNextFile(searchHandle, ref fileData);
                }
                foreach (string oldFile in files)
                {
                    SFileRemoveFile(hMpq, ToByteArray(oldFile), 0);
                }

                //Count how many files we need to add
                count += CountGalaxyFiles(srcDir);

                //Extend maximum number of files in map as needed
                int err = SFileSetMaxFileCount(hMpq, count + count/2);
                

                //Copy files to the map
                Copy(hMpq, srcDir, "Galaxy++");

            }
            finally
            {//Free memory
                if (hMpq != 0)
                {
                    SFileFlushArchive(hMpq);
                    SFileCloseArchive(hMpq);
                }

            }
            return true;
        }

        private static void Copy(UInt32 hMpq, DirectoryInfo dir, string path)
        {
            foreach (FileInfo file in dir.GetFiles())
            {
                if (!file.Name.EndsWith(".galaxy++") && !file.Name.EndsWith(".Dialog"))
                    continue;
                string fileName = Path.Combine(path, file.Name);

                UInt32 fileHandle = 0;
                bool fSuccess = SFileCreateFile(hMpq, ToByteArray(fileName), (ulong)DateTime.Now.Ticks, (uint)file.Length, 0, 0, ref fileHandle);

                unsafe
                {
                    void* buffer = Marshal.AllocHGlobal(256).ToPointer();
                    byte[] managedBuffer = new byte[256];
                    Stream stream = file.OpenRead();
                    int read;
                    while ((read = stream.Read(managedBuffer, 0, managedBuffer.Length)) > 0)
                    {
                        byte* p = (byte*)buffer;
                        for (int i = 0; i < read; i++)
                        {
                            *p = managedBuffer[i];
                            p++;
                        }
                        fSuccess &= SFileWriteFile(fileHandle, buffer, (uint)read, 0x12);

                    }
                    stream.Close();
                    Marshal.FreeHGlobal(new IntPtr(buffer));
                }
            }

            foreach (DirectoryInfo childDir in dir.GetDirectories())
            {
                Copy(hMpq, childDir, Path.Combine(path, childDir.Name));
            }
        }

        private static UInt32 CountGalaxyFiles(DirectoryInfo dir)
        {
            UInt32 count = 0;
            foreach (FileInfo file in dir.GetFiles())
            {
                if (file.FullName.EndsWith(".galaxy++") || file.FullName.EndsWith(".Dialog"))
                    count++;
            }
            foreach (DirectoryInfo childDir in dir.GetDirectories())
            {
                count += CountGalaxyFiles(childDir);
            }
            return count;
        }


        public static void ExtractGalaxyppScriptFiles(FileInfo file, DirectoryInfo srcDir, bool autoOverride)
        {
            UInt32 mpqHandle = 0;
            //Opening readonly
            bool success = SFileOpenArchive(ToByteArray(file.FullName), 0, 0x100, ref mpqHandle);
            if (!success)
            {
                MessageBox.Show("Unable to open the source map for copying.", "Error");
                return;
            }

            //Find all galaxy++ and Dialog files
            List<string> files = new List<string>();
            SFILE_FIND_DATA fileData = default(SFILE_FIND_DATA);

            UInt32 searchHandle = SFileFindFirstFile(mpqHandle, ToByteArray("Galaxy++\\*.galaxy++"), ref fileData, 0);
            while (searchHandle != 0)
            {
                if (!success)
                {
                    SFileFindClose(searchHandle);
                    break;
                }
                files.Add(fileData.cFileName);
                success = SFileFindNextFile(searchHandle, ref fileData);
            }

            searchHandle = SFileFindFirstFile(mpqHandle, ToByteArray("Galaxy++\\*.Dialog"), ref fileData, 0);
            success = true;
            while (searchHandle != 0)
            {
                if (!success)
                {
                    SFileFindClose(searchHandle);
                    break;
                }
                files.Add(fileData.cFileName);
                success = SFileFindNextFile(searchHandle, ref fileData);
            }

            //Extract all found files
            foreach (string f in files)
            {
                string path = Path.Combine(srcDir.FullName, f);
                if (f.StartsWith("Galaxy++\\"))//Okay, so this should actually be true for all found files, because of the search filters
                    path = Path.Combine(srcDir.FullName, f.Remove(0, "Galaxy++\\".Length));


                UInt32 hFile = 0;
                success = SFileOpenFileEx(mpqHandle, ToByteArray(f), 0, ref hFile);
                if (!success)
                {
                    MessageBox.Show("Unable to extract " + f + " from the map.", "Error");
                    continue;
                }
                FileInfo destFile = new FileInfo(path);
                if (destFile.Exists && !autoOverride &&
                    MessageBox.Show("The file " + f + " already exists in the project folder. Do you wan't to overwrite it with the one from the map?", 
                        "Override", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    continue;
                CreateDir(destFile.Directory);
                FileStream stream = destFile.Open(FileMode.Create, FileAccess.Write);
                unsafe
                {
                    void* buffer = Marshal.AllocHGlobal(256).ToPointer();
                    UInt32* bytesRead = (uint*)Marshal.AllocHGlobal(sizeof(UInt32)).ToPointer();
                    while (true)
                    {
                        success |= SFileReadFile(hFile, buffer, 256, bytesRead, null) != 0;

                        if (!success) break;
                        if (*bytesRead == 0) break;
                        byte[] managedBuffer = new byte[256];
                        byte* p = (byte*)buffer;
                        for (int i = 0; i < *bytesRead; i++)
                        {
                            managedBuffer[i] = *p;
                            p++;
                        }
                        stream.Write(managedBuffer, 0, (int)*bytesRead);
                    }
                    Marshal.FreeHGlobal(new IntPtr(buffer));
                    Marshal.FreeHGlobal(new IntPtr(bytesRead));
                }
                stream.Close();
                if (!success) 
                {
                    MessageBox.Show("An error occured while extracting " + f + " from the map.", "Error");
                    continue;
                }
                SFileCloseFile(hFile);
            }
            SFileCloseArchive(mpqHandle);
            return;
        }

        public class MpqStreamReadonly : Stream
        {
            private const int BufferSize = 512;
            private UInt32 hMpq;
            private UInt32 hFile;
            private bool CanReadMore;
            private byte[] buffer = new byte[BufferSize];
            private int validBufferLength = BufferSize;
            private int nextBufferPos = BufferSize;
            private int currentBufferNr = 0;
            

            private bool ReadNextChunk()
            {
                if (!CanReadMore)
                    return false;
                if (validBufferLength < BufferSize)
                    return false;
                unsafe
                {
                    void* pBuffer = Marshal.AllocHGlobal(BufferSize).ToPointer();
                    UInt32* bytesRead = (uint*)Marshal.AllocHGlobal(sizeof(UInt32)).ToPointer();
                    
                    int err = SFileReadFile(hFile, pBuffer, (uint) buffer.Length, bytesRead, null);

                    if (err != 0 && err != 38)//38 == EOF
                    {
                        MessageBox.Show("Unable to read the file in an mpq.", "Error");
                        return false;
                    }
                    validBufferLength = (int) (*bytesRead);
                    byte* p = (byte*)pBuffer;
                    for (int i = 0; i < *bytesRead; i++)
                    {
                        buffer[i] = *p;
                        p++;
                    }
                    
                    Marshal.FreeHGlobal(new IntPtr(pBuffer));
                    Marshal.FreeHGlobal(new IntPtr(bytesRead));
                    currentBufferNr++; 
                    if (err == 38)
                    {
                        //The file has been read. Close it
                        SFileCloseFile(hFile);
                        SFileCloseArchive(hMpq);
                        CanReadMore = false;
                    }
                }
                return true;
            }

            public MpqStreamReadonly(FileInfo file, string fileName)
            {
                //Opening readonly
                bool success = SFileOpenArchive(ToByteArray(file.FullName), 0, 0x100, ref hMpq);
                if (!success)
                {
                    MessageBox.Show("Unable to open the map to fetch " + fileName + ".", "Error");
                    return;
                }

                List<string> files = new List<string>();
                SFILE_FIND_DATA fileData = default(SFILE_FIND_DATA);

                UInt32 searchHandle = SFileFindFirstFile(hMpq, ToByteArray(fileName), ref fileData, 0);
                while (searchHandle != 0)
                {
                    if (!success)
                    {
                        SFileFindClose(searchHandle);
                        break;
                    }
                    files.Add(fileData.cFileName);
                    success = SFileFindNextFile(searchHandle, ref fileData);
                }

                if (files.Count == 0)
                {
                    //MessageBox.Show("Unable to find the file " + fileName + " in the mpq " + file.Name + ".", "Error");
                    SFileCloseArchive(hMpq);
                    return;
                }


                string f = files[0];
                {


                    success = SFileOpenFileEx(hMpq, ToByteArray(f), 0, ref hFile);
                    if (!success)
                    {
                        MessageBox.Show("Unable to open the file " + fileName + " in the mpq " + file.Name + ".", "Error");
                        SFileCloseArchive(hMpq);
                        return;
                    }
                    
                    /*unsafe
                    {
                        void* buffer = Marshal.AllocHGlobal(256).ToPointer();
                        UInt32* bytesRead = (uint*)Marshal.AllocHGlobal(sizeof(UInt32)).ToPointer();
                        while (true)
                        {
                            success |= SFileReadFile(hFile, buffer, 256, bytesRead, null);

                            if (!success) break;
                            if (*bytesRead == 0) break;
                            byte[] managedBuffer = new byte[256];
                            byte* p = (byte*)buffer;
                            for (int i = 0; i < *bytesRead; i++)
                            {
                                managedBuffer[i] = *p;
                                p++;
                            }
                            stream.Write(managedBuffer, 0, (int)*bytesRead);
                        }
                        Marshal.FreeHGlobal(new IntPtr(buffer));
                        Marshal.FreeHGlobal(new IntPtr(bytesRead));
                    }*/
                    
                    //SFileCloseFile(hFile);
                }
                //SFileCloseArchive(mpqHandle);
                CanReadMore = true;
            }

            public override void Flush()
            {
                //Nothing to flush..
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                //Dont seek
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                //Not allowed to change the stream
                throw new NotImplementedException();
            }

            public override int Read(byte[] bufferOut, int offset, int count)
            {
                if (!CanReadMore)
                    return 0;
                int read = 0;
                for (int i = 0; i < count; i++)
                {
                    if (nextBufferPos >= buffer.Length)
                    {
                        if (!ReadNextChunk())
                            return read;
                        nextBufferPos = 0;
                    }
                    if (nextBufferPos == validBufferLength)
                        return read;
                    bufferOut[i + offset] = buffer[nextBufferPos];
                    nextBufferPos++;
                    read++;
                }
                return read;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                //Not allowed to change the stream
                throw new NotImplementedException();
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanSeek
            {
                get { return false; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override long Length
            {
                get { throw new NotImplementedException(); }
            }

            public override long Position
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        public static MpqStreamReadonly OpenFileRead(FileInfo mapFile, string file)
        {
            return new MpqStreamReadonly(mapFile, file);
        }

        public class MpqReader : IDisposable
        {
            public bool Valid { get { return hMpq != 0; } }
            private UInt32 hMpq;
            public MpqReader(string mpqPath)
            {
                //Opening readonly
                bool success = SFileOpenArchive(ToByteArray(mpqPath), 0, 0x100, ref hMpq);
                if (!success)
                {
                    hMpq = 0;
                    return;
                }
            }

            public string[] FindFiles(params string[] patterns)
            {
                List<string> filesFound = new List<string>();
                SFILE_FIND_DATA fileData = default(SFILE_FIND_DATA);
                bool success = true;
                foreach (string pattern in patterns)
                {
                    success = true;
                    UInt32 searchHandle = SFileFindFirstFile(hMpq, ToByteArray(pattern), ref fileData, 0);
                    while (searchHandle != 0)
                    {
                        if (!success)
                        {
                            //Some stop condition here...
                            SFileFindClose(searchHandle);
                            break;
                        }
                        filesFound.Add(fileData.cFileName);
                        success = SFileFindNextFile(searchHandle, ref fileData);
                    }
                }
                return filesFound.ToArray();
            }

            public byte[] ExtractFile(string file)
            {
                UInt32 hFile = 0;
                bool success = SFileOpenFileEx(hMpq, ToByteArray(file), 0, ref hFile);
                if (!success) return new byte[0];
                using (MemoryStream stream = new MemoryStream())
                {
                    unsafe
                    {
                        void* buffer = Marshal.AllocHGlobal(256).ToPointer();
                        UInt32* bytesRead = (uint*) Marshal.AllocHGlobal(sizeof (UInt32)).ToPointer();
                        while (true)
                        {
                            success |= SFileReadFile(hFile, buffer, 256, bytesRead, null) != 0;

                            if (!success) break;
                            if (*bytesRead == 0) break;
                            byte[] managedBuffer = new byte[256];
                            byte* p = (byte*) buffer;
                            for (int i = 0; i < *bytesRead; i++)
                            {
                                managedBuffer[i] = *p;
                                p++;
                            }
                            stream.Write(managedBuffer, 0, (int) *bytesRead);
                        }
                        Marshal.FreeHGlobal(new IntPtr(buffer));
                        Marshal.FreeHGlobal(new IntPtr(bytesRead));
                    }
                    if (!success) return new byte[0];
                    SFileCloseFile(hFile);
                    return stream.ToArray();
                }
            }

            public bool HasFile(string filePath)
            {
                return FindFiles(filePath).Length > 0;
            }

            public void Dispose()
            {
                SFileCloseArchive(hMpq);
            }
        }
    }
}
