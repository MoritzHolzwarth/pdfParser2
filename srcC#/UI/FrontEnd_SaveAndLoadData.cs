using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {

        private void saveData()
        {
            if(!anytingHasChanged)
                return;
            if(!Directory.Exists(savedContentsFolderPath))
                Directory.CreateDirectory(savedContentsFolderPath);

            foreach(string docname in dictDocs.Keys)
            {
                DocData docdata = dictDocs[docname];
                string filepath = Path.Combine(savedContentsFolderPath, docname + ".json");
                if(File.Exists(filepath) && !docdata.contentHasChanged)
                    continue;
                Dictionary<DocPropertyType, object> dictProps = docdata.dictDocProperties;
                Dictionary<string,string> dictToSaveProps = new Dictionary<string, string>(dictProps.Count);
                foreach(DocPropertyType proptype in dictProps.Keys)
                {
                    object value = dictProps[proptype];
                    string savekey = proptype.ToString();
                    string savevalue = getSaveTextFromDocPropertyValue(proptype, value);
                    dictToSaveProps.Add(savekey, savevalue);
                }
                string serialized = jsSerializer.Serialize(dictToSaveProps);
                File.WriteAllText(filepath, serialized);
            }
        }
        
        private void loadData()
        {
            if(!Directory.Exists(savedContentsFolderPath))
                return;
            foreach(string docname in dictDocs.Keys)
            {
                string filepath = Path.Combine(savedContentsFolderPath, docname + ".json");
                if(!File.Exists(filepath))
                    continue;
                Dictionary<string,string> dictFromSavedProps = jsSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(filepath));
                Dictionary<DocPropertyType,object> dictProps = dictDocs[docname].dictDocProperties;
                foreach(string txtkey in dictFromSavedProps.Keys)
                {
                    string txtval = dictFromSavedProps[txtkey];
                    DocPropertyType proptype;
                    if(!Enum.TryParse(txtkey, out proptype))
                    {
                        MessageBox.Show(string.Format("Beim Laden der gespeicherten Parameter von Dokumententyp '{0}'.\n" + 
                                                    "Es wurde ein Parametereintrag mit invalidem Parametertyp '{1}' gefunden.\n" + 
                                                    "Dieser Parametertyp wird nun schlicht ignoriert, das Program wird fortgesetzt.", docname, txtkey), 
                                                    "Warnung", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Console.WriteLine(string.Format("Error in fillDictDocsFromSavedContents(): unknown Key in saved Contents of {0}!:\n{1}", docname, txtkey));
                        continue;
                    }
                    dictProps[proptype] = getDocPropertyValueFromSavedText(proptype, txtval);
                }
            }
        }

        private string getSaveTextFromDocPropertyValue(DocPropertyType proptype, object val)
        {
            if(DocStringProperty.allSubTypes.Contains(proptype))
                return (string)val;
            if(DocPageXPosProperty.allSubTypes.Contains(proptype))
                return ((PageXPosition)val).ToString();
            if(DocPageQuantifierProperty.allSubTypes.Contains(proptype))
                return ((PageQuantifiers)val).ToString();
            if(DocIntArrayProperty.allSubTypes.Contains(proptype))
                return string.Join(",",(int[])val);
            if(proptype == DocPropertyType.ScaleFactor)
                return ((double)val).ToString();
            if(proptype == DocPropertyType.TextColor)
                return string.Join(",",(double[])val);
            
            throw new Exception("Error in getSaveTextFromDocPropertyValue(): invalid DocPropertyType! " + proptype.ToString());
        }

        public object getDocPropertyValueFromSavedText(DocPropertyType proptype, string propText)
        {
            if(DocStringProperty.allSubTypes.Contains(proptype))
                return propText;
            if(DocPageXPosProperty.allSubTypes.Contains(proptype))
                return getPageXPositionFromText(propText);
            if(DocPageQuantifierProperty.allSubTypes.Contains(proptype))
                return getPageQuantifierFromText(propText);
            if(DocIntArrayProperty.allSubTypes.Contains(proptype))
                return getIntArrayFromText(propText);
            if(proptype == DocPropertyType.ScaleFactor)
                return getDoubleFromText(propText);
            if(proptype == DocPropertyType.TextColor)
                return getDoubleArrayFromRGB1ColorText(propText);
            
            throw new Exception("Error in getDocPropertyValueFromSavedText(): invalid DocPropertyType! " + proptype.ToString());
        }

        private PageXPosition getPageXPositionFromText(string txt)
        {
            switch (txt)
            {
                case "Left":
                    return PageXPosition.Left;
                case "Middle":
                    return PageXPosition.Middle;
                case "Right":
                    return PageXPosition.Right;
                default:
                    throw new System.Exception(string.Format("Error in getPageXPositionFromText(): Unknown PageXPosition Text: {0}", txt));
            }
        }

        private PageQuantifiers getPageQuantifierFromText(string txt)
        {
            switch (txt)
            {
                case "NoneExceptArray":
                    return PageQuantifiers.NoneExceptArray;
                case "AllExceptArray":
                    return PageQuantifiers.AllExceptArray;
                default:
                    throw new Exception(string.Format("Error in getPageQuantifierFromText(): Unknown PageQuantifier Text: {0}", txt));
            }
        }

        public static int[] getIntArrayFromText(string txt)
        {
            string txt1 = Regex.Replace(txt,"\\s","");
            string[] arrTxt = txt1.Split(separator: new char[] {','}, options: StringSplitOptions.RemoveEmptyEntries);
            int[] arrInts = new int[arrTxt.Length];
            for(int i=0; i<arrTxt.Length; i++)
            {
                if(!int.TryParse(arrTxt[i], out arrInts[i]))
                    throw new Exception(string.Format("Error in getIntArrayFromText(): Cannot convert text-array entry '{0}' to int!", arrTxt[i]));
            }
            return arrInts;
        }

        private double[] getDoubleArrayFromRGB1ColorText(string txt)
        {
            string txt1 = Regex.Replace(txt,"\\s","");
            string[] arrTxt = txt1.Split(separator: new char[] {','}, options: StringSplitOptions.RemoveEmptyEntries);
            double[] arrDoubles = new double[arrTxt.Length];
            for(int i=0; i<arrTxt.Length; i++)
            {
                if(!double.TryParse(arrTxt[i], out arrDoubles[i]))
                    throw new Exception(string.Format("Error in getDoubleArrayFromText(): Cannot convert text-array entry '{0}' to double!", arrTxt[i]));
            }
            return arrDoubles;
        }
        
        public static double getDoubleFromText(string txt)
        {
            double val;
            if(!double.TryParse(txt, out val))
                throw new Exception(string.Format("Error in getDoubleFromText(): Cannot convert text {0} to double!", txt));
            return val;
        }
    }
}