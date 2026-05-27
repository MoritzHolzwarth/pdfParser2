//Written in C# 5
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Windows.Forms;

namespace pdfParserByMH
{
    enum ProgramState {DoAllDoku, DoSingelPDF, AdjustDokuParams, Null}
    public partial class FrontEndProgram
    {
        Form form;
        Form DokuParamsForm;
        Panel DokuParamsPanel;
        Size formSize;
        TabControl tabControl;
        ProgramState progState = ProgramState.Null;
        TabPage AllDokuForm;
        TabPage SinglePDFForm;
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
        Dictionary<string, DocData> dictDocs;
        Dictionary<string, CheckBox> dictDocCheckboxes;
        bool anytingHasChanged = false;
        bool overrideDokuFiles = false;
        bool overrideSinglePDF = false;
        Dictionary<DocPropertyType,DocProperty> dictDocProperties;  //This is for Doku-Documents
        Dictionary<DocPropertyType,DocProperty> dictSinglePDFProperties; //This is for the Single PDF editing
        System.Web.Script.Serialization.JavaScriptSerializer jsSerializer;
        public FrontEndProgram()
        {
            savedContentsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                            "pdfParser2ByMH" );
            jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            doc = new pdfDocument();

            int width = Math.Min(800, Screen.PrimaryScreen.WorkingArea.Width);
            int height = Math.Min(1000, Screen.PrimaryScreen.WorkingArea.Height);
            formSize = new Size(width, height);

            form = new Form();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.Font = new Font("Arial", 12);
            form.Text = "PDF Stempeln";
            form.AutoScaleMode = AutoScaleMode.Font;
            form.Size = new Size(800,1000);
            form.AutoScroll = true;

            tabControl = new TabControl();
            form.Controls.Add(tabControl);
            tabControl.Size = formSize;
            
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
