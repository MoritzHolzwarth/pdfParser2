//Written in C# 5
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;

namespace pdfParserByMH
{
    enum ProgramState {DoAllDoku, DoSingelPDF, AdjustDokuParams, Null}
    public partial class FrontEndProgram
    {
        Form form;
        TabControl tabControl;
        ProgramState progState = ProgramState.Null;
        System.Windows.Forms.TabPage AllDokuForm;
        System.Windows.Forms.TabPage SinglePDFForm;
        Form DokuParamsForm;
        bool anytingHasChanged;
        pdfDocument doc;
        string dokuFolderPath;
        string uploadFolderApproxName = "1_Upload NWL_Rev.*";
        string uploadFolderPath;
        string dokuID;
        string dokuRev;
        string ADBRev;
        string savedContentsFolderPath;
        System.Web.Script.Serialization.JavaScriptSerializer jsSerializer;
        Dictionary<string, DocData> dictDocs;

        Dictionary<DocPropertyType,DocProperty> dictDocProperties;
        public FrontEndProgram()
        {
            anytingHasChanged = false;
            savedContentsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                            "pdfParser2ByMH" );
            jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();

            doc = new pdfDocument();
            form = new Form();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.AutoScaleMode = AutoScaleMode.Font;
            form.Size = new Size(800,950);
            tabControl = new TabControl();
            tabControl.Size = new Size(800,900);
            makeSingelPDFForm();
            makeDokuParamsForm();
            makeAllDokuForm();
            tabControl.Controls.Add(SinglePDFForm);
            form.Controls.Add(tabControl);
            form.AutoScroll = true;
        }

        private void saveFormContents()
        {
            if(!anytingHasChanged)
                return;
            bool newDirectory = false;
            if(!Directory.Exists(savedContentsFolderPath))
            {
                Directory.CreateDirectory(savedContentsFolderPath);
                newDirectory = true;
            }
            foreach(string docname in dictDocs.Keys)
            {
                DocData docdata = dictDocs[docname];
                if(!newDirectory && !docdata.hasChanged)
                    continue;
                string filepath = Path.Combine(savedContentsFolderPath, docname + ".json");
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

        public void run()
        {
            DialogResult result = form.ShowDialog();
            while(result == DialogResult.OK)
            {
                switch (progState)
                {
                    case ProgramState.DoAllDoku:
                        allDokuStempeln();
                        break;
                    case ProgramState.DoSingelPDF:
                        singlePDFStempeln();
                        break;
                    case ProgramState.AdjustDokuParams:
                        adjustDokuParams();
                        break;
                    default:
                        throw new Exception(string.Format("Error in FrontEndProgram.run(): Invalid Program State '{0}'!", progState.ToString()));
                }
                progState = ProgramState.Null; //reset Program State after execution, to be set again by next Form Dialog.
                result = form.ShowDialog();
            }
            saveFormContents();
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

        

        private string insertPlaceHolders(string txt)
        {
            string newTxt;
            if(dokuID != null)
                newTxt = Regex.Replace(txt,"/Doku_ID", dokuID);
            else
                newTxt = Regex.Replace(txt,"/Doku_ID", "?");
            if(dokuRev != null)
                newTxt = Regex.Replace(newTxt, "/Doku_Rev", dokuRev);
            else
                newTxt = Regex.Replace(newTxt, "/Doku_Rev", "?");
            if(ADBRev != null)
                newTxt = Regex.Replace(newTxt, "/ADB_Rev", ADBRev);
            else
                newTxt = Regex.Replace(newTxt, "/ADB_Rev", "?");
            return newTxt;
        }


    }
}
