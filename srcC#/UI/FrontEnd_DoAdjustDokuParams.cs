using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void adjustDokuParams()
        {
            AllDokuForm_readFields();
            List<DocData> selectedDocDatas = new List<DocData>();
            foreach(DocData docdata in dictDocs.Values)
            {
                if(docdata.doStempeln) //Note. This bool only represents whether the box is checked! As this does not mean Stempeln in this case, the bool should have another name!
                    selectedDocDatas.Add(docdata);
            }
            if(selectedDocDatas.Count == 0)
            {
                string message = "Wählen Sie mindestend einen Dokumententyp aus, um die Parameter anzupassen.";
                MessageBox.Show(message, "Keine Auswahl getroffen", MessageBoxButtons.OK);
                return;
            }
//---------------
            if(selectedDocDatas.Count > 1)
                DokuParamsForm_fillFields();
            else
                DokuParamsForm_fillFields(selectedDocDatas[0]);

            DialogResult resultFromDokuParams = DokuParamsForm.ShowDialog();
            if(resultFromDokuParams != DialogResult.OK)
                return;
//--------------
            DokuParamsForm_readFields();
            foreach(KeyValuePair<DocPropertyType,DocProperty> kvp in dictDocProperties)
            {
                DocProperty docprop = kvp.Value;
                if(!docprop.hasChanged)
                    continue;
                foreach(DocData docdata in selectedDocDatas)
                    docdata.dictDocProperties[kvp.Key] = docprop.value;
            }
//------------
            if(dictDocProperties.Values.Any(prop => prop.hasChanged))
            {
                anytingHasChanged = true;
                foreach(DocData docdata in selectedDocDatas)
                    docdata.contentHasChanged = true;
            }
//--------------
            AllDokuForm_makeFields();
            AllDokuForm_fillFields();
        }
    }
}