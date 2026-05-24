using System;
using System.Windows.Forms;
using System.IO;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void prepare_AllDokuStempeln()
        {
            dokuFolderPath = AllDokuForm.Controls["AllDoku_FolderPath"].Text;
            if(!System.IO.Directory.Exists(dokuFolderPath))
            {
                throw new System.Exception(string.Format("Error on FrontEndProgram.run(): could not find this Doku-FolderPath: {0}", dokuFolderPath));
            }
            string[] subDirectories = System.IO.Directory.GetDirectories(dokuFolderPath,"1_Upload NWL_Rev.*");
            if(subDirectories.Length == 0)
            {
                throw new System.Exception(string.Format("Error on FrontEndProgram.run(): could not find any UploadFolderPath like {0}\\1_Upload NWL_Rev.*!", dokuFolderPath));
            }
            if(subDirectories.Length > 1)
            {
                throw new System.Exception("Error in FrontEndProgram.run(): Found more than 1 UploadFolder in dokuFolder!");
            }
            uploadFolderPath = subDirectories[0];
            getDokuID();
            getDokuRev();
            if(dictDocs["Abfalldatenblatt"].box.Checked)
                getADBRev();
        }

        private void getDokuID()
        {
            string pattern = "^.*_(?<ID>[^\\\\]+-\\d{4}-\\d+)_[^\\\\]*$";
            System.Text.RegularExpressions.Regex regDokuID = new System.Text.RegularExpressions.Regex(pattern);
            var matches = regDokuID.Matches(dokuFolderPath);
            if(matches.Count == 0)
            {
                throw new System.Exception("Error in FrontEndProgram.getDokuID(): No dokuID-match found in dokuFolderPath!");
            }
            if(matches.Count > 1)
            {
                throw new System.Exception("Error in FrontEndProgram.getDokuID(): More than 1 dokuID-matches found in dokuFolderPath!");
            }
            dokuID = matches[0].Groups["ID"].Value;
        }

        private void getDokuRev()
        {
            string pattern = "Rev.(?<rev>\\d+)$";
            System.Text.RegularExpressions.Regex regDokuID = new System.Text.RegularExpressions.Regex(pattern);
            var matches = regDokuID.Matches(uploadFolderPath);
            if(matches.Count == 0)
            {
                System.Console.WriteLine("Warning in FrontEndProgram.getDokuRev(): No dokuRev-match found in uploadFolderPath! -> using dokuRev = 0");
                dokuRev = "0";
                return;
            }
            if(matches.Count > 1)
            {
                throw new System.Exception("Error in FrontEndProgram.getDokuRev(): More than 1 dokuRev-matches found in uploadFolderPath!");
            }
            dokuRev = matches[0].Groups["rev"].Value;
        }

        private bool getADBRev()
        {
            DocData docData = dictDocs["Abfalldatenblatt"];
            string[] ADBFilePaths = Directory.GetFiles(Path.Combine(uploadFolderPath, docData.folderName), docData.fileApproxName, SearchOption.TopDirectoryOnly);
            if(ADBFilePaths.Length != 1)
                return false;
            
            string pattern = "Rev.(?<rev>\\d+)$";
            string ADBFileName = Path.GetFileNameWithoutExtension(ADBFilePaths[0]);
            System.Text.RegularExpressions.Regex regRev = new System.Text.RegularExpressions.Regex(pattern);
            var matches = regRev.Matches(ADBFileName);
            if(matches.Count != 1)
                return false;

            ADBRev = matches[0].Groups["rev"].Value;
            return true;
        }

        private void allDokuStempeln()
        {
            prepare_AllDokuStempeln();

            System.Collections.Generic.List<string> lstMissedFiles = new System.Collections.Generic.List<string>();
            bool overrideFiles = ((CheckBox)AllDokuForm.Controls["Override_Files"]).Checked;
            foreach(string docName in dictDocs.Keys)
            {
                bool isChecked = dictDocs[docName].box.Checked;
                if(!isChecked)
                    continue;
                System.Console.WriteLine(string.Format("Now Stempeln: {0}", docName));
                string subFolderName = dictDocs[docName].folderName;
                string fileNameWildcard = dictDocs[docName].fileApproxName;
                string[] filePaths = System.IO.Directory.GetFiles(System.IO.Path.Combine(uploadFolderPath, subFolderName) , fileNameWildcard, System.IO.SearchOption.TopDirectoryOnly);
                if(filePaths.Length == 0)
                {
                    Console.WriteLine(string.Format("Warning in AllDokuStempeln(): Could not find any File like {0}", fileNameWildcard));
                    lstMissedFiles.Add(docName);
                    continue;
                }
                if(filePaths.Length > 1)
                {
                    Console.WriteLine("Warning in AllDokuStempeln(): More than one File found!");
                }
                string dirGestempelt = Path.Combine(uploadFolderPath, subFolderName, "gestempelt");
                foreach(string filePath in filePaths)
                {
                    string outPath;
                    if(overrideFiles)
                    {
                       outPath = filePath;
                    }
                    else
                    {
                        if(!Directory.Exists(dirGestempelt))
                            Directory.CreateDirectory(dirGestempelt);
                        outPath = Path.Combine(dirGestempelt, Path.GetFileName(filePath));
                    }
                    if(!stempelDokuDoc(docName, filePath, outPath))
                        lstMissedFiles.Add(docName);
                }
            }
            if(lstMissedFiles.Count > 0)
            {
                System.Console.WriteLine(string.Format("{0} requested Files could not get gestempelt:", lstMissedFiles.Count));
                foreach(string docName in lstMissedFiles)
                    Console.WriteLine(docName);
            }
            else
            {
                Console.WriteLine("Success! All requested Files are gestempelt.");
            }
        }

        private bool stempelDokuDoc(string docName, string filePath, string outPath)
        {
            DocData docData = dictDocs[docName];
            string header = insertPlaceHolders(docData.header);
            string footer = insertPlaceHolders(docData.footer);
            try
            {
                doc.setFilePath(filePath);
                doc.setOutputPath(outPath);
                doc.stempeln(header, footer, docData.headerXPos, docData.footerXPos, "/Helvetica", 12, docData.textColor, docData.scaleFactor, 
                            docData.vertPagesQuantifier, docData.horiPagesQuantifier, docData.vertPagesNumbers, docData.horiPagesNumbers);
                doc.write();
            }
            catch(Exception exc)
            {
                string message = string.Format("Die Datei {0} kann leider nicht gestempelt werden!\nEs gibt folgenden Fehler bei der PDF-Behandlung:\n\n{1}", 
                                            Path.GetFileName(filePath), exc.Message);
                MessageBox.Show(message, "Fehler", MessageBoxButtons.OK);
                return false;
            }
            return true;
        }
    }
}