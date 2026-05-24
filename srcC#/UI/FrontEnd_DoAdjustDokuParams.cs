using System.Collections.Generic;
using System.Windows.Forms;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void adjustDokuParams()
        {
            List<DocData> selectedDocDatas = new List<DocData>();
            foreach(DocData docdata in dictDocs.Values)
            {
                if(docdata.box.Checked)
                    selectedDocDatas.Add(docdata);
            }
            if(selectedDocDatas.Count == 0)
            {
                string message = "Wählen Sie mindestend einen Dokumententyp aus, um die Parameter anzupassen.";
                MessageBox.Show(message, "Keine Auswahl getroffen", MessageBoxButtons.OK);
                return;
            }
            if(selectedDocDatas.Count > 1)
                fillDokuParamsForm();
            else
                fillDokuParamsForm(selectedDocDatas[0]);

            DialogResult resultFromDokuParams = DokuParamsForm.ShowDialog();
            if(resultFromDokuParams != DialogResult.OK)
                return;
            
            bool hasChanged = false;
            foreach(KeyValuePair<DocPropertyType,DocProperty> kvp in dictDocProperties)
            {
                DocProperty docprop = kvp.Value;
                if(!docprop.read())
                    continue;
                if(!hasChanged)
                    hasChanged = true;
                foreach(DocData docdata in selectedDocDatas)
                    docdata.dictDocProperties[kvp.Key] = docprop.value;
            }
            if(hasChanged)  //mark edited Document Types
            {
                anytingHasChanged = true;
                foreach(DocData docdata in selectedDocDatas)
                    docdata.hasChanged = true;
            }
        }
    }
}