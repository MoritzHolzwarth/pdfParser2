//Written in C# 5
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;

namespace pdfParserByMH
{
    enum StempelOptions {DoAllDoku, DoSingelPDF, AdjustDokuParams, Null}
    public partial class FrontEndProgram
    {
        System.Windows.Forms.Form form;
        StempelOptions stempelOption = StempelOptions.Null;
        System.Windows.Forms.TabPage AllDokuForm;
        System.Collections.Generic.Dictionary<string, PathAndBox> dictDocs;
        System.Windows.Forms.TabPage SinglePDFForm;
        Form DokuParamsForm;
        pdfDocument doc;
        string dokuFolderPath;
        string uploadFolderPath;
        string dokuID;
        string dokuRev;
        string ADBRev;
        string savedContentsPath;
        System.Collections.Generic.Dictionary<string,string> dictSavedContents;
        
        public FrontEndProgram()
        {
            savedContentsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                            "pdfParser2ByMH", "FormContents.txt" );
            fillDictSavedContents();
            doc = new pdfDocument();
            form = new System.Windows.Forms.Form();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            form.Size = new System.Drawing.Size(800,950);
            makeSingelPDFForm();
            makeAllDokuForm();
            makeDokuParamsForm();
            System.Windows.Forms.TabControl tabControl = new System.Windows.Forms.TabControl();
            tabControl.Size = new System.Drawing.Size(800,900);
            tabControl.Controls.Add(SinglePDFForm);
            tabControl.Controls.Add(AllDokuForm);
            form.Controls.Add(tabControl);
            form.AutoScroll = true;
        }

        private void fillDictSavedContents()
        {
            dictSavedContents = new System.Collections.Generic.Dictionary<string,string>();
            if(!System.IO.File.Exists(savedContentsPath))
                return;
            foreach(string line in File.ReadAllLines(savedContentsPath))
            {
                int separatorIndex = line.IndexOf('=');
                if(separatorIndex < 0)
                    continue;
                string key = line.Substring(0, separatorIndex);
                string value = line.Substring(separatorIndex+1);
                dictSavedContents.Add(key, value);
            }
        }

        // private void saveFormContents()
        // {
        //     string dirName = Path.GetDirectoryName(savedFormContentsPath);
        //     if(!Directory.Exists(dirName))
        //         Directory.CreateDirectory(dirName);
        //     List<string> arrLines = new List<string>(dictSinglePDFFormContents.Count);
        //     foreach(KeyValuePair<string,string> kvp in dictSinglePDFFormContents)
        //         arrLines.Add(string.Format("{0}={1}", kvp.Key, kvp.Value));
        //     File.WriteAllLines(savedFormContentsPath, arrLines.ToArray());
        // }

        public void run()
        {
            System.Windows.Forms.DialogResult result = form.ShowDialog();
            while(result == System.Windows.Forms.DialogResult.OK)
            {
                switch (stempelOption)
                {
                    case StempelOptions.Null:
                        throw new System.Exception("Error in run(): No valid StempelOption! Must be DoAllDoku or DoSinglePDF");
                    case StempelOptions.DoAllDoku:
                        allDokuStempeln();
                        break;
                    case StempelOptions.DoSingelPDF:
                        singlePDFStempeln();
                        break;
                    case StempelOptions.AdjustDokuParams:
                        DialogResult resultFromDokuParams = DokuParamsForm.ShowDialog();
                        if(resultFromDokuParams == DialogResult.OK)
                            Console.WriteLine("OK on DokuParamsForm");
                        break;
                    default:
                        break;
                }
                result = form.ShowDialog();
            }
            // saveFormContents();
        }

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
                throw new SystemException("Error in FrontEndProgram.getDokuID(): No dokuID-match found in dokuFolderPath!");
            }
            if(matches.Count > 1)
            {
                throw new SystemException("Error in FrontEndProgram.getDokuID(): More than 1 dokuID-matches found in dokuFolderPath!");
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
                throw new SystemException("Error in FrontEndProgram.getDokuRev(): More than 1 dokuRev-matches found in uploadFolderPath!");
            }
            dokuRev = matches[0].Groups["rev"].Value;
        }

        private bool getADBRev()
        {
            PathAndBox docData = dictDocs["Abfalldatenblatt"];
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
                    System.Console.WriteLine(docName);
            }
            else
            {
                System.Console.WriteLine("Success! All requested Files are gestempelt.");
            }
        }

        private bool stempelDokuDoc(string docName, string filePath, string outPath)
        {
            PathAndBox docData = dictDocs[docName];
            string header = insertPlaceHolders(docData.header);
            string footer = insertPlaceHolders(docData.footer);
            try
            {
                doc.setFilePath(filePath);
                doc.setOutputPath(outPath);
                doc.stempeln(header, footer, docData.headerXPos, docData.footerXPos, "/Helvetica", 12, docData.textColor, docData.scaleFactor, 
                            docData.vertPagesQuatifier, docData.horiPagesQuantifier, docData.vertPagesNumbers, docData.horiPagesNumbers);
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

        private string insertPlaceHolders(string txt)
        {
            string newTxt = Regex.Replace(txt,"/DokuID",dokuID);
            newTxt = Regex.Replace(newTxt, "/Doku_Rev", dokuRev);
            newTxt = Regex.Replace(newTxt, "/ADB_Rev", ADBRev);
            return newTxt;
        }

        private void singlePDFStempeln()
        {
            string filePath = SinglePDFForm.Controls["SinglePDF_FilePath"].Text;
            if(!System.IO.File.Exists(filePath))
                throw new System.Exception(string.Format("Error in singlePDFStempeln(): File does not exist! {0}", filePath));
            if(filePath.Substring(filePath.Length-4, 4) != ".pdf")
                throw new System.Exception(string.Format("Error in singlePDFStempeln(): File is not PDF! {0}", filePath));
            // dictSinglePDFFormContents["SinglePDF_FilePath"] = filePath;
            
            bool overrideFile = ((CheckBox)SinglePDFForm.Controls["Override_File"]).Checked;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string folder = System.IO.Path.GetDirectoryName(filePath);
            string outPath = overrideFile ? filePath : System.IO.Path.Combine(folder, string.Format("{0}_gestempelt.pdf", fileName));
            
            string header = SinglePDFForm.Controls["SinglePDF_Header"].Text;
            // dictSinglePDFFormContents["SinglePDF_Header"] = header;
            header = header.Replace("\r", "");
            string footer = SinglePDFForm.Controls["SinglePDF_Footer"].Text;
            footer = footer.Replace("\r", "");

            GroupBox box_vertical = (GroupBox)SinglePDFForm.Controls["Box_VerticalPages"];
            GroupBox box_horizontal = (GroupBox)SinglePDFForm.Controls["Box_HorizontalPages"];
            GroupBox box_headerXPos = (GroupBox)SinglePDFForm.Controls["Box_HeaderXPos"];
            GroupBox box_footerXPos = (GroupBox)SinglePDFForm.Controls["Box_FooterXPos"];
            TextBox tb_scaleFactor = (TextBox)SinglePDFForm.Controls["SinglePDF_Scaling"];

            ValueTuple<PageQuantifiers, int[]> vertPageSettings = getPageQuantifierForOrientation(box_vertical);
            ValueTuple<PageQuantifiers, int[]> horiPageSettings = getPageQuantifierForOrientation(box_horizontal);
            PageXPosition headerXPos = getPageXPosition(box_headerXPos);
            PageXPosition footerXPos = getPageXPosition(box_footerXPos);

            double scaleFactor;
            if(!double.TryParse(tb_scaleFactor.Text, out scaleFactor))
                throw new System.Exception(string.Format("Error in singlePDFStempeln(): Invalid Scale Factor Format! {0}", tb_scaleFactor.Text));

            string colorHex = ((Label)SinglePDFForm.Controls["ColorName"]).Text;
            Color col = ColorTranslator.FromHtml(colorHex);
            double convert = 1.0/255;
            double[] arrColor = new double[] {convert*col.R, convert*col.G, convert*col.B};
            
            try
            {
                doc.setFilePath(filePath);
                doc.setOutputPath(outPath);
                doc.stempeln(header, footer, headerXPos, footerXPos, "/Helvetica", 12, arrColor, scaleFactor, 
                            vertPageSettings.Item1, horiPageSettings.Item1, vertPageSettings.Item2, horiPageSettings.Item2);
                doc.write();
            }
            catch(Exception exc)
            {
                string message = string.Format("Die Datei {0} kann leider nicht gestempelt werden!\nEs gibt folgenden Fehler bei der PDF-Behandlung:\n\n{1}", 
                                            Path.GetFileName(filePath), exc.Message);
                MessageBox.Show(message, "Fehler", MessageBoxButtons.OK);
            }
        }

        private ValueTuple<PageQuantifiers, int[]> getPageQuantifierForOrientation(GroupBox box)
        {
            bool allExceptArray = false;
            bool noneExceptArray = false;
            string textbox_content = "";

            foreach(var cont in box.Controls)
            {
                if(cont is RadioButton)
                {
                    RadioButton rb = (RadioButton)cont;
                    switch (rb.Name)
                    {
                        case "RB_AllExceptArray":
                            allExceptArray = rb.Checked;
                            break;
                        case "RB_NoneExceptArray":
                            noneExceptArray = rb.Checked;
                            break;
                        default:
                            throw new System.Exception(string.Format("Error in getPageQuantifierForOrientation(): Unknown RadioButton Name! {0}", rb.Name));
                    }
                }
                else if(cont is TextBox)
                {
                    textbox_content = ((TextBox)cont).Text;
                }
            }
            string[] arrTBContent = textbox_content.Replace(" ", "").Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
            int numPages = arrTBContent.Length;
            int[] arrPageNumbers = new int[numPages];
            for(int i=0; i<numPages; i++)
            {
                int pageNum;
                if(int.TryParse(arrTBContent[i], out pageNum))
                {
                    arrPageNumbers[i] = pageNum;
                }
                else
                    throw new System.Exception("Error in getPageQuantifierForOrientation(): List of Pages in Horizontal-Option not valid Format!");
            }
            if(allExceptArray)
            {
                if(numPages == 0)
                    return new ValueTuple<PageQuantifiers,int[]>(PageQuantifiers.All, null);
                else
                    return new ValueTuple<PageQuantifiers,int[]>(PageQuantifiers.AllExceptArray, arrPageNumbers);
            }
            else if(noneExceptArray)
            {
                if(numPages == 0)
                    return new ValueTuple<PageQuantifiers,int[]>(PageQuantifiers.None, null);
                else
                    return new ValueTuple<PageQuantifiers,int[]>(PageQuantifiers.NoneExceptArray, arrPageNumbers);
            }
            throw new System.Exception("Error in getPageQuantifierForOrientation(): No possible Option Selected!");
        }

        private PageXPosition getPageXPosition(GroupBox box)
        {
            foreach(var cont in box.Controls)
            {
                if(cont is RadioButton)
                {
                    RadioButton rb = (RadioButton)cont;
                    if(!rb.Checked)
                        continue;
                    switch (rb.Name)
                    {
                        case "RB_XLeft":
                            return PageXPosition.Left;
                        case "RB_XMiddle":
                            return PageXPosition.Middle;
                        case "RB_XRight":
                            return PageXPosition.Right;
                        default:
                            break;
                    }
                }
            }
            throw new System.Exception("Error in getPageXPosition(): No X-Position given!");
        }
    }
}
