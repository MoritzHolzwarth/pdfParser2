
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;

namespace pdfParserByMH
{
    enum StempelOptions {DoAllDoku, DoSingelPDF, Null}
    public partial class FrontEndProgram
    {
        System.Windows.Forms.Form form;
        StempelOptions stempelOption = StempelOptions.Null;
        System.Windows.Forms.TabPage AllDokuForm;
        System.Collections.Generic.Dictionary<string, PathAndBox> dictDocs;
        System.Windows.Forms.TabPage SinglePDFForm;
        pdfDocument doc;
        string dokuFolderPath;
        string uploadFolderPath;
        string dokuID;
        string dokuRev;
        
        public FrontEndProgram()
        {
            doc = new pdfDocument();
            form = new System.Windows.Forms.Form();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            form.Size = new System.Drawing.Size(800,900);
            makeAllDokuForm();
            makeSingelPDFForm();
            System.Windows.Forms.TabControl tabControl = new System.Windows.Forms.TabControl();
            tabControl.Size = new System.Drawing.Size(800,900);
            tabControl.Controls.Add(AllDokuForm);
            tabControl.Controls.Add(SinglePDFForm);
            form.Controls.Add(tabControl);
        }

        public void run()
        {
            System.Windows.Forms.DialogResult result = form.ShowDialog();
            if(result != System.Windows.Forms.DialogResult.OK)
            {
                System.Console.WriteLine($"FrontEndProgram.run(): Aborted on Form-Dialogresult {result}");
                return;
            }
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
                default:
                    break;
            }
        }

        private void prepare_AllDokuStempeln()
        {
            dokuFolderPath = AllDokuForm.Controls["AllDoku_FolderPath"].Text;
            if(!System.IO.Directory.Exists(dokuFolderPath))
            {
                throw new System.Exception($"Error on FrontEndProgram.run(): could not find this Doku-FolderPath: {dokuFolderPath}");
            }
            string[] subDirectories = System.IO.Directory.GetDirectories(dokuFolderPath,"1_Upload NWL_Rev.*");
            if(subDirectories.Length == 0)
            {
                throw new System.Exception($"Error on FrontEndProgram.run(): could not find any UploadFolderPath like {dokuFolderPath + "\\1_Upload NWL_Rev.*"}!");
            }
            if(subDirectories.Length > 1)
            {
                throw new System.Exception($"Error in FrontEndProgram.run(): Found more than 1 UploadFolder in dokuFolder!");
            }
            uploadFolderPath = subDirectories[0];
            getDokuID();
            getDokuRev();
        }

        private void stempelnADB(string filePath, string fileName, string outPath)
        {
            string ADBRev = getADBRev(fileName);
            string header = $"A. ADB Rev.{ADBRev},\nDoku-ID: {dokuID} Rev.{dokuRev}";
            string footer = "Das Original ist an dieser Stelle rot gestempelt.";
            double[] color = new double[] {1, 0, 0};
            int[] verticalPages = new int[] {1, 2, 3};
            doc.setOutputPath(outPath);
            doc.setFilePath(filePath);
            doc.stempeln(header, footer, PageXPosition.Right, PageXPosition.Left, "/Helvetica", 12, color, 0.9, PageQuantifiers.NoneExceptArray, PageQuantifiers.AllExceptArray, verticalPages, verticalPages);
            doc.write();
        }

        private void stempelnOther(string filePath, string outPath)
        {
            string header = dokuID;
            string footer = "Seite /PageNum von /PagesCount";
            double[] color = new double[] {0, 0, 0};
            doc.setOutputPath(outPath);
            doc.setFilePath(filePath);
            doc.stempeln(header, footer, PageXPosition.Middle, PageXPosition.Right, "/Helvetica", 12, color, 0.9);
            doc.write();
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

        private string getADBRev(string filePath)
        {
            string pattern = "Rev.(?<rev>\\d+)$";
            System.Text.RegularExpressions.Regex regDokuID = new System.Text.RegularExpressions.Regex(pattern);
            var matches = regDokuID.Matches(filePath);
            if(matches.Count == 0)
            {
                System.Console.WriteLine("Warning in FrontEndProgram.getADBRev(): No ADBRev-match found in ADB-FilePath! -> using ADBRev = 0");
                return "0";
            }
            if(matches.Count > 1)
            {
                throw new SystemException("Error in FrontEndProgram.getADBRev(): More than 1 ADBRev-matches found in ADB-FilePath!");
            }
            return matches[0].Groups["rev"].Value;
        }

        private void allDokuStempeln()
        {
            prepare_AllDokuStempeln();

            System.Collections.Generic.List<string> lstMissedFiles = new System.Collections.Generic.List<string>();
            foreach(string docName in dictDocs.Keys)
            {
                bool isChecked = dictDocs[docName].box.Checked;
                if(!isChecked)
                    continue;
                System.Console.WriteLine($"Now Stempeln: {docName}");
                string subFolderName = dictDocs[docName].folderName;
                string fileNameWildcard = dictDocs[docName].fileName;
                string[] filePaths = System.IO.Directory.GetFiles(System.IO.Path.Combine(uploadFolderPath, subFolderName) , fileNameWildcard, System.IO.SearchOption.TopDirectoryOnly);
                if(filePaths.Length == 0)
                {
                    System.Console.WriteLine($"Warning in FrontEndProgram.stempeln(): Could not find any File like {fileNameWildcard}");
                    lstMissedFiles.Add(docName);
                    continue;
                }
                if(filePaths.Length > 1)
                {
                    System.Console.WriteLine("Warning in FrontEndProgram.stempeln(): More than one File found!");
                }
                string dirGestempelt = System.IO.Path.Combine(uploadFolderPath, subFolderName, "gestempelt");
                foreach(string filePath in filePaths)
                {
                    string fileName = System.IO.Path.GetFileName(filePath);
                    if(!System.IO.Directory.Exists(dirGestempelt))
                    {
                        System.IO.Directory.CreateDirectory(dirGestempelt);
                    }
                    string filePathGestempelt = System.IO.Path.Combine(dirGestempelt, fileName);
                    if(docName == "Abfalldatenblatt")
                    {
                        stempelnADB(filePath, fileName, filePathGestempelt);
                    }
                    else
                    {
                        stempelnOther(filePath, filePathGestempelt);
                    }
                }
            }
            if(lstMissedFiles.Count > 0)
            {
                System.Console.WriteLine($"{lstMissedFiles.Count} requested Files could not get gestempelt:");
                foreach(string docName in lstMissedFiles)
                    System.Console.WriteLine(docName);
            }
            else
            {
                System.Console.WriteLine("Success! All requested Files are gestempelt.");
            }
        }

        private void singlePDFStempeln()
        {
            string filePath = SinglePDFForm.Controls["SinglePDF_FilePath"].Text;
            if(!System.IO.File.Exists(filePath))
                throw new System.Exception($"Error in singlePDFStempeln(): File does not exist! {filePath}");
            if(filePath.Substring(filePath.Length-4, 4) != ".pdf")
                throw new System.Exception($"Error in singlePDFStempeln(): File is not PDF! {filePath}");
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string folder = System.IO.Path.GetDirectoryName(filePath);
            string outPath = System.IO.Path.Combine(folder,$"{fileName}_gestempelt.pdf");
            
            string header = SinglePDFForm.Controls["SinglePDF_Header"].Text;
            header = header.Replace("\r", "");
            string footer = SinglePDFForm.Controls["SinglePDF_Footer"].Text;
            footer = footer.Replace("\r", "");

            GroupBox box_vertical = (GroupBox)SinglePDFForm.Controls["Box_VerticalPages"];
            GroupBox box_horizontal = (GroupBox)SinglePDFForm.Controls["Box_HorizontalPages"];
            GroupBox box_headerXPos = (GroupBox)SinglePDFForm.Controls["Box_HeaderXPos"];
            GroupBox box_footerXPos = (GroupBox)SinglePDFForm.Controls["Box_FooterXPos"];
            TextBox tb_scaleFactor = (TextBox)SinglePDFForm.Controls["SinglePDF_Scaling"];

            (PageQuantifiers, int[]) vertPageSettings = getPageQuantifierForOrientation(box_vertical);
            (PageQuantifiers, int[]) horiPageSettings = getPageQuantifierForOrientation(box_horizontal);
            PageXPosition headerXPos = getPageXPosition(box_headerXPos);
            PageXPosition footerXPos = getPageXPosition(box_footerXPos);

            if(!double.TryParse(tb_scaleFactor.Text, out double scaleFactor))
                throw new System.Exception($"Error in singlePDFStempeln(): Invalid Scale Factor Format! {tb_scaleFactor.Text}");

            string colorHex = ((Label)SinglePDFForm.Controls["ColorName"]).Text;
            Color col = ColorTranslator.FromHtml(colorHex);
            double convert = 1.0/255;
            double[] arrColor = new double[] {convert*col.R, convert*col.G, convert*col.B};
            foreach(var c in arrColor)
            {
                System.Console.WriteLine($"{c}");
            }

            doc.setFilePath(filePath);
            doc.setOutputPath(outPath);
            doc.stempeln(header, footer, headerXPos, footerXPos, "/Helvetica", 12, arrColor, scaleFactor, 
                        vertPageSettings.Item1, horiPageSettings.Item1, vertPageSettings.Item2, horiPageSettings.Item2);
            doc.write();
        }

        private (PageQuantifiers, int[]) getPageQuantifierForOrientation(GroupBox box)
        {
            bool allExceptArray = false;
            bool noneExceptArray = false;
            string textbox_content = "";

            foreach(var cont in box.Controls)
            {
                if(cont is RadioButton rb)
                switch (rb.Name)
                    {
                        case "RB_AllExceptArray":
                            allExceptArray = rb.Checked;
                            break;
                        case "RB_NoneExceptArray":
                            noneExceptArray = rb.Checked;
                            break;
                        default:
                            throw new System.Exception($"Error in getPageQuantifierForOrientation(): Unknown RadioButton Name! {rb.Name}");
                    }
                else if(cont is TextBox tb)
                {
                    textbox_content = tb.Text;
                }
            }
            string[] arrTBContent = textbox_content.Replace(" ", "").Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
            int numPages = arrTBContent.Length;
            int[] arrPageNumbers = new int[numPages];
            for(int i=0; i<numPages; i++)
            {
                if(int.TryParse(arrTBContent[i], out int pageNum))
                {
                    arrPageNumbers[i] = pageNum;
                }
                else
                    throw new System.Exception("Error in getPageQuantifierForOrientation(): List of Pages in Horizontal-Option not valid Format!");
            }
            if(allExceptArray)
            {
                if(numPages == 0)
                    return (PageQuantifiers.All, null);
                else
                    return (PageQuantifiers.AllExceptArray, arrPageNumbers);
            }
            else if(noneExceptArray)
            {
                if(numPages == 0)
                    return (PageQuantifiers.None, null);
                else
                    return (PageQuantifiers.NoneExceptArray, arrPageNumbers);
            }
            throw new System.Exception("Error in getPageQuantifierForOrientation(): No possible Option Selected!");
        }

        private PageXPosition getPageXPosition(GroupBox box)
        {
            foreach(var cont in box.Controls)
            {
                if(cont is RadioButton rb && rb.Checked)
                {
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