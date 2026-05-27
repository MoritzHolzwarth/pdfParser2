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
//------------
            DocData[] arrDocDatas = dictDocs.Values.Where(d => d.selected).ToArray();
            if(arrDocDatas.Length == 0)
            {
                string message = "Wählen Sie mindestend einen Dokumententyp aus, um die Parameter anzupassen.";
                MessageBox.Show(message, "Keine Auswahl getroffen", MessageBoxButtons.OK);
                return;
            }
//---------------
            DokuParamsForm_makeFields();
            DokuParamsForm_fillFields(arrDocDatas);
//--------------
            DialogResult resultFromDokuParams = DokuParamsForm.ShowDialog();
            if(resultFromDokuParams != DialogResult.OK)
                return;
//--------------
            DokuParamsForm_readFields();
//--------------
            foreach(KeyValuePair<DocPropertyType,DocProperty> kvp in dictDocProperties)
            {
                DocProperty docprop = kvp.Value;
                if(!docprop.hasChanged)
                    continue;
                foreach(DocData docdata in arrDocDatas)
                    docdata.dictDocProperties[kvp.Key] = docprop.value;
            }
//------------
            if(dictDocProperties.Values.Any(p => p.hasChanged))
            {
                anytingHasChanged = true;
                foreach(DocData docdata in arrDocDatas)
                    docdata.contentHasChanged = true;
//------------
            AllDokuForm_makeFields();
            AllDokuForm_fillFields();
            }
        }
    }
}