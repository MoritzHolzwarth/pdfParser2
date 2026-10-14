using System.Windows.Forms;
using System;
using System.IO;
using System.Collections.Generic;


namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void undoStempel()
        {
            string filePath;
            string outPath;
            DocData docdata;
            try
            {
                SinglePDFForm_readFields();
//-----------
                docdata = new DocData();
                foreach(KeyValuePair<DocPropertyType, DocProperty> kvp in dictSinglePDFProperties)
                {
                    DocProperty docprop = kvp.Value;
                    docdata.dictDocProperties[kvp.Key] = docprop.value;
                }
    //-------------
                filePath = docdata.filePath;
                if(!File.Exists(filePath))
                {
                    MessageBox.Show("Der angegeben Dateipfad konnte nicht gefunden werden!", "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Console.WriteLine("Error in singlePDFStempeln(): Couldn't find File Path!");
                    return;
                }
                if(overrideSinglePDF)
                    outPath = filePath;
                else
                    outPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_stempel_weg.pdf");
                }
            catch(Exception exc)
            {
                MessageBox.Show(string.Format("Fehler beim Auslesen der Parameter und Vorbereitung der Stempel-Rückgängigmachung:\n\n{0}", exc.Message),
                            "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
//---------------
            bool foundMarkedContent;
            try
            {
                doc.setFilePath(filePath);
                doc.setOutputPath(outPath);
                foundMarkedContent = doc.undoStempel();
                if(foundMarkedContent)
                    doc.write();
            }
            catch(Exception exc)
            {
                MessageBox.Show(string.Format("Die Rückgängigmachung des Stempels bei der Datei {0} ist fehlgeschlagen!\nEs gab folgenden Fehler bei der Behandlung:\n\n{1}", Path.GetFileName(filePath), exc.Message),
                                "Fehler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(foundMarkedContent)
                MessageBox.Show("Stempelbezogener Content und Skalierung wurden entfernt.", "Fertig", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show("Kein stempelbezogener Content oder Skalierung gefunden.", "Fertig", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}