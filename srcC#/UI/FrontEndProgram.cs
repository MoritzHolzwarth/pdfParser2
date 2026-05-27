//Written in C# 5
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace pdfParserByMH
{
    enum ProgramState {DoAllDoku, DoSingelPDF, AdjustDokuParams, Null}
    public partial class FrontEndProgram
    {
        Form form;
        TabControl tabControl;
        ProgramState progState = ProgramState.Null;
        TabPage AllDokuForm;
        TabPage SinglePDFForm;
        Form DokuParamsForm;
        bool anytingHasChanged = false;
        pdfDocument doc;
        string dokuFolderPath = "";
        string uploadFolderApproxName = "1_Upload NWL_Rev.*";
        string uploadFolderPath;
        string dokuID;
        string dokuRev;
        string ADBRev;
        string dokuID_placeholder = "{dokuid}";
        string DokuRev_placeholder = "{dokurev}";
        string ADBRev_placeholder = "{adbrev}";
        string savedContentsFolderPath;
        System.Web.Script.Serialization.JavaScriptSerializer jsSerializer;
        Dictionary<string, DocData> dictDocs;
        Dictionary<string, CheckBox> dictDocCheckboxes;
        bool overrideDokuFiles = false;
        bool overrideSinglePDF = false;
        Dictionary<DocPropertyType,DocProperty> dictDocProperties;  //This is for Doku-Documents
        Dictionary<DocPropertyType,DocProperty> dictSinglePDFProperties; //This is for the Single PDF editing
        public FrontEndProgram()
        {
            savedContentsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                            "pdfParser2ByMH" );
            jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            doc = new pdfDocument();

            form = new Form();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Font = new Font("Arial", 12);
            form.Text = "PDF Stempeln";
            form.AutoScaleMode = AutoScaleMode.Font;
            form.Size = new Size(800,950);
            form.AutoScroll = true;

            tabControl = new TabControl();
            tabControl.Size = new Size(800,900);
            form.Controls.Add(tabControl);
            
            makeDictDocs();
            loadData();

            makeAllDokuForm();

            makeSingelPDFForm();  

            makeDokuParamsForm();     
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
            saveData();
        }

    }
}
