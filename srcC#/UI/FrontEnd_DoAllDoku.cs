using System;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void allDokuStempeln()
        {
            AllDokuForm_readFields();
//---------------
            if(!prepare_AllDokuStempeln())
                return;
//--------------
            System.Collections.Generic.List<string> lstMissedFiles = new System.Collections.Generic.List<string>();
            foreach(string docName in dictDocs.Keys)
            {
                DocData docdata = dictDocs[docName];
                if(!docdata.selected)
                    continue;
                Console.WriteLine(string.Format("Now Stempeln: {0}", docName));
                string subFolderName = docdata.folderName;
                string fileNameWildcard = docdata.fileApproxName;
                string[] filePaths = Directory.GetFiles(Path.Combine(uploadFolderPath, subFolderName) , fileNameWildcard, SearchOption.TopDirectoryOnly);
                if(filePaths.Length == 0)
                {
                    //Maybe here should be a Message Box, not sure if that would be reasonable.
                    Console.WriteLine(string.Format("Warning in allDokuStempeln(): Could not find any File like {0}", fileNameWildcard));
                    lstMissedFiles.Add(docName);
                    continue;
                }
                if(filePaths.Length > 1)
                {
                    string multipleFiles = "";
                    foreach(string f in filePaths)
                        multipleFiles += Path.GetFileName(f) + "\n";
                    MessageBox.Show(string.Format("Für den Dokumententyp '{0}' wurden mehrere pdf-Dateien gefunden.\nAlle Dateien werden nun gestempelt.\n{1}", docName, multipleFiles),
                                    "Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Console.WriteLine("Warning in allDokuStempeln(): More than one File found for " + docName + "! All files will be processed!");
                }
                string dirGestempelt = Path.Combine(uploadFolderPath, subFolderName, "gestempelt");
                foreach(string filePath in filePaths)
                {
                    string outPath;
                    if(overrideDokuFiles)
                    {
                       outPath = filePath;
                    }
                    else
                    {
                        if(!Directory.Exists(dirGestempelt))
                            Directory.CreateDirectory(dirGestempelt);
                        outPath = Path.Combine(dirGestempelt, Path.GetFileName(filePath));
                    }
                    if(!stempelDokuDoc(docdata, filePath, outPath))
                        lstMissedFiles.Add(docName);
                }
            }
            if(lstMissedFiles.Count > 0)
            {
                string missedFiles = "";
                foreach(string docName in lstMissedFiles)
                    missedFiles += docName + "\n";
                MessageBox.Show(string.Format("{0} der angeforderten Dokumente konnten nicht gestempelt werden!\n{1}", lstMissedFiles.Count, missedFiles),
                                    "Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Console.WriteLine(string.Format("{0} requested Files could not get gestempelt:\n{1}", lstMissedFiles.Count, missedFiles));
            }
            else
            {
                MessageBox.Show("Alle angeforderten Dateien wurden gestempelt.", "Fertig", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Console.WriteLine("Success! All requested Files are gestempelt.");
            }
        }

        private bool prepare_AllDokuStempeln()
        {
            if(!Directory.Exists(dokuFolderPath))
            {
                MessageBox.Show(string.Format("Der angegebene Dokuordner konnte nicht gefunden werden!\n" +
                                            "Es scheint, im Pfad '{0}' existiert kein Ordner!", dokuFolderPath),
                                            "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(string.Format("Error in prepare_AllDokuStempeln(): could not find Doku-FolderPath '{0}'", dokuFolderPath));
                return false;
            }
            string[] subDirectories = Directory.GetDirectories(dokuFolderPath, uploadFolderApproxName);
            if(subDirectories.Length == 0)
            {
                MessageBox.Show(string.Format("Uploadordner konnte nicht gefunden werden!\n" +
                                            "Es scheint, es existiert kein Ordner mit Wildcard-Pfad '{0}'", Path.Combine(dokuFolderPath,uploadFolderApproxName)),
                                            "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine(string.Format("Error in prepare_AllDokuStempeln(): could not find Upload-FolderPath '{0}'", Path.Combine(dokuFolderPath,uploadFolderApproxName)));
                return false;
            }
            if(subDirectories.Length > 1)
            {
                MessageBox.Show(string.Format("Im Dokuordner wurden mehrere Ordner gefunden, die auf die Form des Uploadordners '{0}' passen!\n" + 
                                "Der Uploadordner muss eindeutig sein!", uploadFolderApproxName),
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Error in FrontEndProgram.run(): Found more than 1 UploadFolder in dokuFolder!");
                return false;
            }
            uploadFolderPath = subDirectories[0];
            if(!getDokuID())
                return false;
            if(!getDokuRev())
                return false;
            if(dictDocs["Abfalldatenblatt"].selected)
            {
                if(!getADBRev())
                    return false;
            } 
            return true;
        }

        private bool getDokuID()
        {
            string pattern = "^.*_(?<ID>[^\\\\]+-\\d{4}-\\d+)_[^\\\\]*$";
            System.Text.RegularExpressions.Regex regDokuID = new System.Text.RegularExpressions.Regex(pattern);
            var matches = regDokuID.Matches(dokuFolderPath);
            if(matches.Count == 0)
            {
                MessageBox.Show("Keine Doku-ID konnte aus dem Namen des DokuOrdenrs abgeleitet werden!\n" + 
                                "Es wird erwartet, dass die Doku-ID im Ordnernamen gemäß dem Schema '{...}_{Doku-ID}-{Zahl}-{Zahl}_{...}' vorliegt!",
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Error in FrontEndProgram.getDokuID(): No dokuID-match found in dokuFolderPath!");
                return false;
            }
            if(matches.Count > 1)
            {
                MessageBox.Show("Die Doku-ID konnte nicht eindeutig aus dem Namen des DokuOrdenrs abgeleitet werden!\n" + 
                                "Es wurden mehrere mögliche Doku-IDs gefunden!\n" +
                                "Es wird erwartet, dass die Doku-ID im Ordnernamen eindeutig gemäß dem Schema '{...}_{Doku-ID}-{Zahl}-{Zahl}_{...}ENDE' vorliegt!",
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Error in FrontEndProgram.getDokuID(): More than 1 dokuID-matches found in dokuFolderPath!");
                return false;
            }
            dokuID = matches[0].Groups["ID"].Value;
            return true;
        }

        private bool getDokuRev()
        {
            string pattern = "Rev.(?<rev>\\d+)$";
            System.Text.RegularExpressions.Regex regDokuID = new System.Text.RegularExpressions.Regex(pattern);
            var matches = regDokuID.Matches(uploadFolderPath);
            if(matches.Count == 0)
            {
                MessageBox.Show("Keine Doku-Reviosionsnummer konnte aus dem Namen des Uploadordners abgeleitet werden!\n" + 
                                "Es wird erwartet, dass die Doku-Revisionsnummer im Ordnernamen gemäß dem Schema '{...}Rev.{Doku-Rev}ENDE' vorliegt!",
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Error in FrontEndProgram.getDokuRev(): No dokuRev-match found in uploadFolderPath!");
                return false;
            }
            if(matches.Count > 1)
            {
                MessageBox.Show("Die Doku-Revisionsnummer konnte nicht eindeutig aus dem Namen des Uploadordners abgeleitet werden!\n" + 
                                "Es wurden mehrere mögliche Doku-Revisionsnummern gefunden!\n" +
                                "Es wird erwartet, dass die Doku-Rev im Ordnernamen eindeutig gemäß dem Schema '{...}Rev.{Doku-Rev}ENDE' vorliegt!",
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Error in FrontEndProgram.getDokuRev(): More than 1 dokuRev-matches found in uploadFolderPath!");
                return false;
            }
            dokuRev = matches[0].Groups["rev"].Value;
            return true;
        }

        private bool getADBRev()
        {
            DocData docData = dictDocs["Abfalldatenblatt"];
            string[] ADBFilePaths = Directory.GetFiles(Path.Combine(uploadFolderPath, docData.folderName), docData.fileApproxName, SearchOption.TopDirectoryOnly);
            if(ADBFilePaths.Length != 1)
            {
                MessageBox.Show(string.Format("Die ADB-pdf-Datei konnte nicht eindeutig gefunden werden!\n" + 
                                "Es wurden mehrere Dateien gemäß dem Schema {0} gefunden!\n" + 
                                "Die ADB-Datei muss eindeutig sein, um die ADB-Revisionsnummer abzuleiten!", Path.Combine(docData.folderName, docData.fileApproxName)),
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Error in getABDRev(): More than 1 ADB-Files found!");
                return false;
            }
            string pattern = "Rev.(?<rev>\\d+)$";
            string ADBFileName = Path.GetFileNameWithoutExtension(ADBFilePaths[0]);
            System.Text.RegularExpressions.Regex regRev = new System.Text.RegularExpressions.Regex(pattern);
            var matches = regRev.Matches(ADBFileName);
            if(matches.Count == 0)
            {
                MessageBox.Show("Keine ADB-Reviosionsnummer konnte aus dem Namen der ADB-Datei abgeleitet werden!\n" + 
                                "Es wird erwartet, dass die ADB-Revisionsnummer im Dateinamen gemäß dem Schema '{...}Rev.{ADB-Rev}ENDE' vorliegt!",
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Error in FrontEndProgram.getADBRev(): No ADBRev-match found in file name!");
                return false;
            }
            if(matches.Count > 1)
            {
                MessageBox.Show("Die ADB-Revisionsnummer konnte nicht eindeutig aus dem Namen der ADB-Datei abgeleitet werden!\n" + 
                                "Es wurden mehrere mögliche ADB-Revisionsnummern gefunden!\n" +
                                "Es wird erwartet, dass die ADB-Rev im Dateinamen eindeutig gemäß dem Schema '{...}Rev.{ADB-Rev}ENDE' vorliegt!",
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Error in FrontEndProgram.getADBRev(): More than 1 ADBRev-matches found in file name!");
                return false;
            }
            ADBRev = matches[0].Groups["rev"].Value;
            return true;
        }

        private bool stempelDokuDoc(DocData docData, string filePath, string outPath)
        {
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
                MessageBox.Show(string.Format("Die Datei {0} kann leider nicht gestempelt werden!\nEs gibt folgenden Fehler bei der Behandlung:\n\n{1}",Path.GetFileName(filePath), exc.Message),
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private string insertPlaceHolders(string txt)
        {
            string newTxt = txt;
            if(dokuID != null)
                newTxt = Regex.Replace(newTxt, dokuID_placeholder, dokuID);
            if(dokuRev != null)
                newTxt = Regex.Replace(newTxt, DokuRev_placeholder, dokuRev);
            if(ADBRev != null)
                newTxt = Regex.Replace(newTxt, ADBRev_placeholder, ADBRev);
            return newTxt;
        }
    }
}