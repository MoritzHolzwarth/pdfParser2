
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace pdfParserByMH
{

    public class DocData
    {
        public Dictionary<DocPropertyType,object> dictDocProperties {get; set;}
        public string folderName
        {
            get {return (string)dictDocProperties[DocPropertyType.FolderName];}
            set {dictDocProperties[DocPropertyType.FolderName] = value;}
        }
        public string fileApproxName
        {
            get {return (string)dictDocProperties[DocPropertyType.ApproxFileName];}
            set {dictDocProperties[DocPropertyType.ApproxFileName] = value;}
        }
        public string header
        {
            get {return (string)dictDocProperties[DocPropertyType.Header];}
            set {dictDocProperties[DocPropertyType.Header] = value;}
        }
        public string footer
        {
            get {return (string)dictDocProperties[DocPropertyType.Footer];}
            set {dictDocProperties[DocPropertyType.Footer] = value;}
        }
        public PageXPosition headerXPos
        {
            get {return (PageXPosition)dictDocProperties[DocPropertyType.HeaderXPos];}
            set {dictDocProperties[DocPropertyType.HeaderXPos] = value;}
        }
        public PageXPosition footerXPos
        {
            get {return (PageXPosition)dictDocProperties[DocPropertyType.FooterXPos];}
            set {dictDocProperties[DocPropertyType.FooterXPos] = value;}
        }
        public PageQuantifiers vertPagesQuantifier
        {
            get {return (PageQuantifiers)dictDocProperties[DocPropertyType.VertPageQuant];}
            set {dictDocProperties[DocPropertyType.VertPageQuant] = value;}
        }
        public PageQuantifiers horiPagesQuantifier
        {
            get {return (PageQuantifiers)dictDocProperties[DocPropertyType.HoriPageQuant];}
            set {dictDocProperties[DocPropertyType.HoriPageQuant] = value;}
        }
        public int[] vertPagesNumbers
        {
            get {return (int[])dictDocProperties[DocPropertyType.VertPageNumbers];}
            set {dictDocProperties[DocPropertyType.VertPageNumbers] = value;}
        }
        public int[] horiPagesNumbers
        {
            get {return (int[])dictDocProperties[DocPropertyType.HoriPageNumbers];}
            set {dictDocProperties[DocPropertyType.HoriPageNumbers] = value;}
        }
        public double scaleFactor
        {
            get {return (double)dictDocProperties[DocPropertyType.ScaleFactor];}
            set {dictDocProperties[DocPropertyType.ScaleFactor] = value;}
        }
        public double[] textColor
        {
            get {return (double[])dictDocProperties[DocPropertyType.TextColor];}
            set {dictDocProperties[DocPropertyType.TextColor] = value;}
        }
        public System.Windows.Forms.CheckBox box {get; set;}
        public bool hasChanged {get; set;}
        public DocData(string folder = "", string file = "")
        {
            dictDocProperties = new Dictionary<DocPropertyType, object>();
            folderName = folder;
            fileApproxName = file;
            box = new CheckBox();
            hasChanged = false;
        }
    }
    public partial class FrontEndProgram
    {
        private void makeAllDokuForm()
        {
            AllDokuForm = new System.Windows.Forms.TabPage("Doku Stempeln");
            AllDokuForm.Size = new System.Drawing.Size(800, 700);
            AllDokuForm.Font = new System.Drawing.Font("Helvetica", 12);
            AllDokuForm.BackColor = System.Drawing.ColorTranslator.FromHtml("#E1F3F5");

            AllDokuForm_fillDictDocs();
            HashSet<string> setFolderNames = new HashSet<string>();
            foreach(DocData docdata in dictDocs.Values)
                setFolderNames.Add(docdata.folderName);

            int y = 10;
            y = AllDokuForm_makeDokuFolderPathControl(y);
            foreach(string folderName in setFolderNames)
                y = AllDokuForm_makeFolderCheckboxes(y + 10, folderName);
            
            y = AllDokuForm_makeButtons(y + 10);
        }

    

        private int AllDokuForm_makeButtons(int y)
        {
            int x = 100;

            Button selectAllButton = new Button();
            selectAllButton.Location = new Point(x, y);
            selectAllButton.Size = new Size(100, 50);
            selectAllButton.Text = "Alles Auswählen";
            selectAllButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            selectAllButton.Click += (sender, e) =>
            {
                foreach(DocData docData in dictDocs.Values)
                    docData.box.Checked = true;
            };
            AllDokuForm.Controls.Add(selectAllButton);

            x += selectAllButton.Size.Width + 200;
            Button stempelButton = new Button();
            stempelButton.Location = new Point(x, y);
            stempelButton.Size = new Size(100,50);
            stempelButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            stempelButton.Text = "Stempeln";
            stempelButton.Click += (sender, e) =>
            {
                stempelOption = StempelOptions.DoAllDoku;
                form.DialogResult = DialogResult.OK;
                form.Close();
            };
            AllDokuForm.Controls.Add(stempelButton);

            y += selectAllButton.Size.Height + 10;
            x = 100;
            Button selectNoneButton = new Button();
            selectNoneButton.Location = new Point(x, y);
            selectNoneButton.Size = new Size(100, 50);
            selectNoneButton.Text = "Alles Abwählen";
            selectNoneButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            selectNoneButton.Click += (sender, e) =>
            {
                foreach(DocData docData in dictDocs.Values)
                    docData.box.Checked = false;
            };
            AllDokuForm.Controls.Add(selectNoneButton);

            x += stempelButton.Size.Width + 200;
            Button stempelParamsButton = new Button();
            stempelParamsButton.Location = new Point(x, y);
            stempelParamsButton.Size = new Size(100, 50);
            stempelParamsButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            stempelParamsButton.Text = "Parameter Anpassen";
            stempelParamsButton.Click += (sender, e) =>
            {
                stempelOption = StempelOptions.AdjustDokuParams;
                form.DialogResult = DialogResult.OK;
                form.Close();
            };
            AllDokuForm.Controls.Add(stempelParamsButton);

            y += stempelButton.Size.Height;
            return y;
        }

        private int AllDokuForm_makeDokuFolderPathControl(int y)
        {
            int x = 40;
            System.Windows.Forms.Label textboxLabel = new System.Windows.Forms.Label();
            textboxLabel.Location = new System.Drawing.Point(x, y);
            textboxLabel.Text = "Pfad des Dokuordner";
            textboxLabel.AutoSize = true;
            AllDokuForm.Controls.Add(textboxLabel);

            x += textboxLabel.PreferredWidth + 300;
            System.Windows.Forms.Label overrideFiles_Label = new Label();
            overrideFiles_Label.Location = new System.Drawing.Point(x, y);
            overrideFiles_Label.Text = "Dateien überschreiben:";
            overrideFiles_Label.AutoSize = true;
            AllDokuForm.Controls.Add(overrideFiles_Label);

            x += overrideFiles_Label.PreferredWidth + 10;
            CheckBox overrideFile_Checkbox = new CheckBox();
            overrideFile_Checkbox.Name = "Override_Files";
            overrideFile_Checkbox.Location = new Point(x, y);
            overrideFile_Checkbox.Checked = false;
            AllDokuForm.Controls.Add(overrideFile_Checkbox);

            y += textboxLabel.PreferredHeight;
            x = 40;
            System.Windows.Forms.TextBox textbox = new System.Windows.Forms.TextBox();
            textbox.Name = "AllDoku_FolderPath";
            textbox.Location = new System.Drawing.Point(x, y);
            textbox.Size = new System.Drawing.Size(600,20);
            AllDokuForm.Controls.Add(textbox);

            x += textbox.Size.Width + 10;
            System.Windows.Forms.Button browseButton = new System.Windows.Forms.Button();
            browseButton.Location = new System.Drawing.Point(x, y);
            browseButton.Size = new System.Drawing.Size(100,50);
            browseButton.Text = "Browse";
            browseButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            browseButton.Click += (sender, e) =>
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textbox.Text = dialog.SelectedPath;
                }
            };
            AllDokuForm.Controls.Add(browseButton);

            y += Math.Max(textbox.PreferredHeight, browseButton.PreferredSize.Height);
            return y;
        }

        private int AllDokuForm_makeFolderCheckboxes(int y, string folderName)
        {
            int x = 40;
            GroupBox groubboxFolder = new GroupBox();
            groubboxFolder.Name = "AllDoku_FolderBox_" + folderName;
            groubboxFolder.Text = folderName;
            groubboxFolder.Location = new System.Drawing.Point(x, y);
            groubboxFolder.Size = new System.Drawing.Size(700, 10);
            AllDokuForm.Controls.Add(groubboxFolder);

            int x_boxlocal = 30;
            int y_boxlocal = 30;
            int labelHeight = 0;
            int distance = 20;
            foreach(KeyValuePair<string,DocData> kvp in dictDocs)
            {
                if(kvp.Value.folderName != folderName)
                    continue;
                Label docName = new Label();
                docName.Text = kvp.Key;
                docName.AutoSize = true;
                CheckBox chb = kvp.Value.box;
                chb.AutoSize = true;
                groubboxFolder.Controls.Add(docName);
                groubboxFolder.Controls.Add(chb);

                int width = docName.PreferredWidth + 10 + chb.PreferredSize.Width;
                labelHeight = docName.PreferredHeight;
                if(x_boxlocal + width > groubboxFolder.Size.Width)
                {
                    y_boxlocal += 30;
                    x_boxlocal = 30;
                }

                docName.Location = new Point(x_boxlocal, y_boxlocal);
                x_boxlocal += docName.PreferredWidth + 10;
                chb.Location = new Point(x_boxlocal, y_boxlocal);
                x_boxlocal += chb.PreferredSize.Width + distance;
            }

            y_boxlocal += labelHeight;
            if(y_boxlocal > groubboxFolder.Size.Height)
                groubboxFolder.Size = new Size(groubboxFolder.Size.Width, y_boxlocal + 10);
            
            y += groubboxFolder.Size.Height;
            return y;
        }




        private void AllDokuForm_fillDictDocs()
        {
            dictDocs = new Dictionary<string, DocData>();
            fillDictDocsFromDefault();
            fillDictDocsFromSavedContents();
        }

        private void fillDictDocsFromSavedContents()
        {
            if(!Directory.Exists(savedContentsFolderPath))
                return;
            foreach(string docname in dictDocs.Keys)
            {
                string filepath = Path.Combine(savedContentsFolderPath, docname + ".txt");
                if(!File.Exists(filepath))
                    continue;
                Dictionary<string,string> dictFromSavedProps = jsSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(filepath));
                Dictionary<DocPropertyType,object> dictProps = dictDocs[docname].dictDocProperties;
                foreach(string txtkey in dictFromSavedProps.Keys)
                {
                    string txtval = dictFromSavedProps[txtkey];
                    DocPropertyType proptype;
                    if(!Enum.TryParse(txtkey, out proptype))
                        throw new Exception(string.Format("Error in fillDictDocsFromSavedContents(): unknown Key in saved Contents of {0}!:\n{1}", docname, txtkey));
                    dictProps[proptype] = getDocPropertyValueFromSavedText(proptype, txtval);
                }
            }
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



        private void fillDictDocsFromDefault()
        {   
            dictDocs.Add("Abfalldatenblatt",new DocData("A", "A. ADB APG0*.pdf"));
            dictDocs.Add("Repräs. Begleitschein",new DocData("B.1", "B.1 Repräsentative* Begleitschein*_Rev.*.pdf"));
            dictDocs.Add("Verarbeitungsweg",new DocData("B.1", "B.1 Verarbeitungsweg*_Rev.*.pdf"));
            dictDocs.Add("Deklarationsvorschriften",new DocData("B.1", "B.1 Deklarationsvorschrift*_Rev.*.pdf"));
            dictDocs.Add("Gamma-Analyse",new DocData("B.1", "B.1 Gamma-Analyse*_Rev.*.pdf"));
            dictDocs.Add("LSC-Analyse",new DocData("B.1", "B.1 LSC-Analyse*_Rev.*.pdf"));
            dictDocs.Add("Chargenfreigabe",new DocData("C.1", "C.1 Chargenfreigabe*_Rev.*.pdf"));
            dictDocs.Add("Fassmessprotokolle",new DocData("C.1", "C.1 Fassmessprotokoll*_Rev.*.pdf"));
            dictDocs.Add("Gasphasenanalyse",new DocData("C.1", "C.1 Gasphasenanalyse*_Rev.*.pdf"));
            dictDocs.Add("Prüfprotokoll",new DocData("C.1", "C.1 Prüfprotokoll*_Rev.*.pdf"));
            dictDocs.Add("Trocknungsprotokolle",new DocData("C.1", "C.1 Trocknungsprotokoll*_Rev.*.pdf"));
            dictDocs.Add("Trocknungsnachweis",new DocData("C.1", "C.1 Nachweis Trocknungserfolg*_Rev.*.pdf"));
            dictDocs.Add("Wiegescheine",new DocData("C.1", "C.1 Wiegeschein*.pdf"));
            dictDocs.Add("Fotodokumentation",new DocData("C.1", "C.1 Fotodokumentation_Rev.*.pdf"));
            dictDocs.Add("Inspektionsbericht",new DocData("C.1", "C.1 Inspektionsbericht*_Rev.*.pdf"));
            dictDocs.Add("Verdampferkonzentratanalyse",new DocData("C.1", "C.1 Analyse* Verdampferkonzentrat*_Rev.*.pdf"));
            dictDocs.Add("Klärschlammanalyse",new DocData("C.1", "C.1 Klärschlamm-Analyse* (FSC)_Rev.*.pdf"));
            dictDocs.Add("PSC-Zerlegung",new DocData("C.1", "C.1 PSC-Zerlegung_Rev.*.pdf"));
            dictDocs.Add("Ablaufplan",new DocData("C.1", "C.1 Ablaufpl*n*_Rev.*.pdf"));
            dictDocs.Add("Querkontamination",new DocData("D.2", "D.2 Querkontamination_Rev.*.pdf"));
            dictDocs.Add("Gammamessung Rados",new DocData("D.2", "D.2 Gammamessung* Rados_Rev.*.pdf"));
            dictDocs.Add("Prüfbericht Druckfestigkeit",new DocData("D.2", "D.2 Prüfbericht* Druckfestigkeit_Rev.*.pdf"));
            dictDocs.Add("Prüfbericht Zementierfähigkeit",new DocData("D.2", "D.2 Prüfbericht* Zementierfähigkeit_Rev.*.pdf"));
            dictDocs.Add("TNE-Stellungnahme NDKL",new DocData("D.2", "D.2 TNE-Stellungnahme* NDKL*_Rev.*.pdf"));
            dictDocs.Add("Verifizierung NDKL C-Altchargen",new DocData("D.2", "D.2 Verifizierung der NDKL der C-Altchargen_Rev.*.pdf"));
            dictDocs.Add("Behälterabnahme",new DocData("D.3", "D.3 Abnahme Behälter_Rev.*.pdf"));
            dictDocs.Add("Reparaturen",new DocData("D.3", "D.3 Reparaturen_Rev.*.pdf"));
            
            foreach(KeyValuePair<string,DocData> kvp in dictDocs)
            {
                if(kvp.Key == "Abfalldatenblatt")
                {
                    kvp.Value.header = "A. ADB Rev./ADB_Rev,\nDoku-ID: /Doku_ID: Rev./Doku_Rev";
                    kvp.Value.footer = "Das Original ist an dieser Stelle rot gestempelt.";
                    kvp.Value.headerXPos = PageXPosition.Middle;
                    kvp.Value.footerXPos = PageXPosition.Left;
                    kvp.Value.vertPagesQuantifier = PageQuantifiers.NoneExceptArray;
                    kvp.Value.vertPagesNumbers = new int[] {1, 2};
                    kvp.Value.horiPagesQuantifier = PageQuantifiers.NoneExceptArray;
                    kvp.Value.horiPagesNumbers = new int[] {};
                    kvp.Value.textColor = new double[] {1, 0, 0};
                    kvp.Value.scaleFactor = 0.9;
                }
                else
                {
                    kvp.Value.header = "/Doku_ID";
                    kvp.Value.footer = "Seite /PageNum von /PagesCount";
                    kvp.Value.headerXPos = PageXPosition.Middle;
                    kvp.Value.footerXPos = PageXPosition.Right;
                    kvp.Value.vertPagesQuantifier = PageQuantifiers.NoneExceptArray;
                    kvp.Value.vertPagesNumbers = new int[] {};
                    kvp.Value.horiPagesQuantifier = PageQuantifiers.NoneExceptArray;
                    kvp.Value.horiPagesNumbers = new int[] {};
                    kvp.Value.textColor = new double[] {0, 0, 0};
                    kvp.Value.scaleFactor = 0.9;
                }
            }
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
                    throw new System.Exception(string.Format("Error: Unknown PageXPosition Text: {0}", txt));
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
                    throw new Exception(string.Format("Error: Unknown PageQuantifier Text: {0}", txt));
            }
        }

        public static int[] getIntArrayFromText(string txt)
        {
            string[] arrTxt = txt.Split(separator: new char[] {','}, options: StringSplitOptions.RemoveEmptyEntries);
            int[] arrInts = new int[arrTxt.Length];
            for(int i=0; i<arrTxt.Length; i++)
            {
                arrInts[i] = Convert.ToInt32(arrTxt[i]);
            }
            return arrInts;
        }

        private double[] getDoubleArrayFromRGB1ColorText(string txt)
        {
            string[] arrTxt = txt.Split(separator: new char[] {','}, options: StringSplitOptions.RemoveEmptyEntries);
            double[] arrDoubles = new double[arrTxt.Length];
            for(int i=0; i<arrTxt.Length; i++)
            {
                arrDoubles[i] = Convert.ToDouble(arrTxt[i]);
            }
            return arrDoubles;
        }
        
        public static double getDoubleFromText(string txt)
        {
            double val;
            if(!double.TryParse(txt, out val))
                throw new System.Exception("Error in getDoubleFromText(): Scale Factor cannot be converted to Double!");
            return val;
        }

        public static double[] getDoubleArrayFromHexColorText(string txt)
        {
            Color col = ColorTranslator.FromHtml(txt);
            double convert = 1.0/255;
            double[] arrColor = new double[] {convert*col.R, convert*col.G, convert*col.B};
            return arrColor;
        }
    }
}