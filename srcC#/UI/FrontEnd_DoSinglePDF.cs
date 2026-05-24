using System.Windows.Forms;
using System.Drawing;
using System;
using System.IO;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void singlePDFStempeln()
        {
            string filePath = SinglePDFForm.Controls["SinglePDF_FilePath"].Text;
            if(!System.IO.File.Exists(filePath))
                throw new System.Exception(string.Format("Error in singlePDFStempeln(): File does not exist! {0}", filePath));
            if(filePath.Substring(filePath.Length-4, 4) != ".pdf")
                throw new System.Exception(string.Format("Error in singlePDFStempeln(): File is not PDF! {0}", filePath));
            
            bool overrideFile = ((CheckBox)SinglePDFForm.Controls["Override_File"]).Checked;
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            string folder = System.IO.Path.GetDirectoryName(filePath);
            string outPath = overrideFile ? filePath : System.IO.Path.Combine(folder, string.Format("{0}_gestempelt.pdf", fileName));
            
            string header = SinglePDFForm.Controls["SinglePDF_Header"].Text;
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
            PageXPosition headerXPos = getPageXPositionFromBox(box_headerXPos);
            PageXPosition footerXPos = getPageXPositionFromBox(box_footerXPos);

            double scaleFactor;
            if(!double.TryParse(tb_scaleFactor.Text, out scaleFactor))
                throw new System.Exception(string.Format("Error in singlePDFStempeln(): Invalid Scale Factor Format! {0}", tb_scaleFactor.Text));

            if(scaleFactor < 0)
                throw new System.Exception(string.Format("Error in singlePDFStempeln(): Negative Scale Factor Value: {0}", tb_scaleFactor.Text));

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

        static public ValueTuple<PageQuantifiers, int[]> getPageQuantifierForOrientation(GroupBox box)
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
                return new ValueTuple<PageQuantifiers,int[]>(PageQuantifiers.AllExceptArray, arrPageNumbers);
            }
            else if(noneExceptArray)
            {
                return new ValueTuple<PageQuantifiers,int[]>(PageQuantifiers.NoneExceptArray, arrPageNumbers);
            }
            throw new System.Exception("Error in getPageQuantifierForOrientation(): No possible Option Selected!");
        }

        static public PageQuantifiers getPageQuantifierFromBox(GroupBox box)
        {
            bool allExceptArray = false;
            bool noneExceptArray = false;
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
            }
            if(allExceptArray && noneExceptArray)
                throw new System.Exception("Error in etPageQuantifierFromBox(): Both Quantifier-Options selected! Invalid!");
            else if(allExceptArray)
                return PageQuantifiers.AllExceptArray;
            else if(noneExceptArray)
                return PageQuantifiers.NoneExceptArray;
            else
                return PageQuantifiers.Null;
        }

        static public PageXPosition getPageXPositionFromBox(GroupBox box)
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
            return PageXPosition.Null;
        }
    }
}