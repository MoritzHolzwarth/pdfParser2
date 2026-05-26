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
        bool anytingHasChanged = false;
        pdfDocument doc;
        string dokuFolderPath = "";
        string uploadFolderApproxName = "1_Upload NWL_Rev.*";
        string uploadFolderPath;
        string dokuID;
        string dokuRev;
        string ADBRev;
        string savedContentsFolderPath;
        System.Web.Script.Serialization.JavaScriptSerializer jsSerializer;
        Dictionary<string, DocData> dictDocs;
        Dictionary<string, CheckBox> dictDocCheckboxes;
        bool overrideDokuFiles = false;

        Dictionary<DocPropertyType,DocProperty> dictDocProperties;
        public FrontEndProgram()
        {
            savedContentsFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                                            "pdfParser2ByMH" );
            jsSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            doc = new pdfDocument();

            form = new Form();
            form.StartPosition = FormStartPosition.CenterScreen;
            form.AutoScaleMode = AutoScaleMode.Font;
            form.Size = new Size(800,950);
            form.AutoScroll = true;

            tabControl = new TabControl();
            tabControl.Size = new Size(800,900);
            form.Controls.Add(tabControl);
            
            makeDictDocs();
            loadData();

            makeAllDokuForm();
            tabControl.Controls.Add(AllDokuForm);

            makeSingelPDFForm();
            tabControl.Controls.Add(SinglePDFForm);

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
