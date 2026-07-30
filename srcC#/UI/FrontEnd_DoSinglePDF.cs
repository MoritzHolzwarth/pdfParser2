using System.Windows.Forms;
using System;
using System.IO;
using System.Collections.Generic;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void singlePDFStempeln()
        {
            SinglePDFForm_readFields();
//-----------
            DocData docdata = new DocData();
            foreach(KeyValuePair<DocPropertyType, DocProperty> kvp in dictSinglePDFProperties)
            {
                DocProperty docprop = kvp.Value;
                docdata.dictDocProperties[kvp.Key] = docprop.value;
            }
//-------------
            string filePath = docdata.filePath;
            if(!File.Exists(filePath))
            {
                MessageBox.Show("Der angegeben Dateipfad konnte nicht gefunden werden!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine("Error in singlePDFStempeln(): Couldn't find File Path!");
                return;
            }
            string outPath;
            if(overrideSinglePDF)
                outPath = filePath;
            else
                outPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_gestempeln.pdf");
//---------------
            try
            {
                doc.setFilePath(filePath);
                doc.setOutputPath(outPath);
                doc.stempeln(docdata.header, docdata.footer, docdata.headerXPos, docdata.footerXPos, "/Helvetica", 12, docdata.textColor, docdata.scaleFactor, 
                            docdata.vertPagesQuantifier, docdata.horiPagesQuantifier, docdata.vertPagesNumbers, docdata.horiPagesNumbers);
                doc.write();

                doc.printData();
            }
            catch(Exception exc)
            {
                MessageBox.Show(string.Format("Die Datei {0} kann leider nicht gestempelt werden!\nEs gibt folgenden Fehler bei der Behandlung:\n\n{1}",Path.GetFileName(filePath), exc.Message),
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            MessageBox.Show("Datei gestempelt.", "Fertig", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}