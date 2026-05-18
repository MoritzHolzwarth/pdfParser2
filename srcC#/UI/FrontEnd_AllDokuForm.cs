
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace pdfParserByMH
{
    public class PathAndBox
    {
        public string fileApproxName {get; set;}
        public string folderName {get; set;}
        public string header {get; set;}
        public string footer {get; set;}
        public PageXPosition headerXPos {get; set;}
        public PageXPosition footerXPos {get; set;}
        public PageQuantifiers vertPagesQuatifier;
        public PageQuantifiers horiPagesQuantifier;
        public int[] vertPagesNumbers {get; set;}
        public int[] horiPagesNumbers {get; set;}
        public double[] textColor {get; set;}
        public double scaleFactor {get; set;}
        public System.Windows.Forms.CheckBox box {get; set;}
        public PathAndBox(string folder = "", string file = "")
        {
            folderName = folder;
            fileApproxName = file;
            box = new System.Windows.Forms.CheckBox();
            header = "";
            footer = "";
            headerXPos = PageXPosition.Left;
            footerXPos = PageXPosition.Left;
            vertPagesQuatifier = PageQuantifiers.None;
            horiPagesQuantifier = PageQuantifiers.None;
            vertPagesNumbers = new int[] {};
            horiPagesNumbers = new int[] {};
            textColor = new double[] {0,0,0};
            scaleFactor = 1.0;
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

            AllDokuForm_initCheckboxes();
            HashSet<string> setFolderNames = new HashSet<string>();
            foreach(PathAndBox pb in dictDocs.Values)
            {
                setFolderNames.Add(pb.folderName);
            }

            int y = 10;
            y = AllDokuForm_makeFolderPathControl(y);
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
                foreach(PathAndBox docData in dictDocs.Values)
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
                foreach(PathAndBox docData in dictDocs.Values)
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

        private int AllDokuForm_makeFolderPathControl(int y)
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
            foreach(KeyValuePair<string,PathAndBox> kvp in dictDocs)
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




        private void AllDokuForm_initCheckboxes()
        {
            dictDocs = new System.Collections.Generic.Dictionary<string, PathAndBox>();
            fillDictDocsFromDefault();
            if(System.IO.File.Exists(savedContentsPath))
                fillDictDocsFromSavedContents();
        }

        private void fillDictDocsFromSavedContents()
        {
            foreach(string docName in dictDocs.Keys)
            {
                PathAndBox docData = dictDocs[docName];
                if(dictSavedContents.ContainsKey(docName + "_folder"))
                    docData.folderName = dictSavedContents[docName + "_folder"];
                if(dictSavedContents.ContainsKey(docName + "_searchPath"))
                    docData.fileApproxName = dictSavedContents[docName + "_searchPath"];
                if(dictSavedContents.ContainsKey(docName + "_header"))
                    docData.header = dictSavedContents[docName + "_header"];
                if(dictSavedContents.ContainsKey(docName + "_footer"))
                    docData.footer = dictSavedContents[docName + "_footer"];
                if(dictSavedContents.ContainsKey(docName + "_headerXPos"))
                    docData.headerXPos = getPageXPositionFromText(dictSavedContents[docName + "_headerXPos"]);
                if(dictSavedContents.ContainsKey(docName + "_footerXPos"))
                    docData.headerXPos = getPageXPositionFromText(dictSavedContents[docName + "_footerXPos"]);
                if(dictSavedContents.ContainsKey(docName + "_vertPageQuatifier"))
                    docData.vertPagesQuatifier = getPageQuantifierFromText(dictSavedContents[docName + "_vertPageQuantifier"]);
                if(dictSavedContents.ContainsKey(docName + "_horiPageQuatifier"))
                    docData.vertPagesQuatifier = getPageQuantifierFromText(dictSavedContents[docName + "_horiPageQuantifier"]);
                if(dictSavedContents.ContainsKey(docName + "vertPageNumbers"))
                    docData.vertPagesNumbers = getIntArrayFromText(dictSavedContents[docName + "vertPageNumbers"]);
                if(dictSavedContents.ContainsKey(docName + "horiPageNumbers"))
                    docData.vertPagesNumbers = getIntArrayFromText(dictSavedContents[docName + "horiPageNumbers"]);
                if(dictSavedContents.ContainsKey(docName + "_textColor"))
                    docData.textColor = getDoubleArrayFromText(dictSavedContents[docName + "_textColor"]);
                if(dictSavedContents.ContainsKey(docName + "_scaleFactor"))
                    docData.scaleFactor = Convert.ToDouble(dictSavedContents[docName + "_scaleFactor"]);
            }
        }

        private void fillDictDocsFromDefault()
        {   
            dictDocs.Add("Abfalldatenblatt",new PathAndBox("A", "A. ADB APG0*.pdf"));
            dictDocs.Add("Repräs. Begleitschein",new PathAndBox("B.1", "B.1 Repräsentative* Begleitschein*_Rev.*.pdf"));
            dictDocs.Add("Verarbeitungsweg",new PathAndBox("B.1", "B.1 Verarbeitungsweg*_Rev.*.pdf"));
            dictDocs.Add("Deklarationsvorschriften",new PathAndBox("B.1", "B.1 Deklarationsvorschrift*_Rev.*.pdf"));
            dictDocs.Add("Gamma-Analyse",new PathAndBox("B.1", "B.1 Gamma-Analyse*_Rev.*.pdf"));
            dictDocs.Add("LSC-Analyse",new PathAndBox("B.1", "B.1 LSC-Analyse*_Rev.*.pdf"));
            dictDocs.Add("Chargenfreigabe",new PathAndBox("C.1", "C.1 Chargenfreigabe*_Rev.*.pdf"));
            dictDocs.Add("Fassmessprotokolle",new PathAndBox("C.1", "C.1 Fassmessprotokoll*_Rev.*.pdf"));
            dictDocs.Add("Gasphasenanalyse",new PathAndBox("C.1", "C.1 Gasphasenanalyse*_Rev.*.pdf"));
            dictDocs.Add("Prüfprotokoll",new PathAndBox("C.1", "C.1 Prüfprotokoll*_Rev.*.pdf"));
            dictDocs.Add("Trocknungsprotokolle",new PathAndBox("C.1", "C.1 Trocknungsprotokoll*_Rev.*.pdf"));
            dictDocs.Add("Trocknungsnachweis",new PathAndBox("C.1", "C.1 Nachweis Trocknungserfolg*_Rev.*.pdf"));
            dictDocs.Add("Wiegescheine",new PathAndBox("C.1", "C.1 Wiegeschein*.pdf"));
            dictDocs.Add("Fotodokumentation",new PathAndBox("C.1", "C.1 Fotodokumentation_Rev.*.pdf"));
            dictDocs.Add("Inspektionsbericht",new PathAndBox("C.1", "C.1 Inspektionsbericht*_Rev.*.pdf"));
            dictDocs.Add("Verdampferkonzentratanalyse",new PathAndBox("C.1", "C.1 Analyse* Verdampferkonzentrat*_Rev.*.pdf"));
            dictDocs.Add("Klärschlammanalyse",new PathAndBox("C.1", "C.1 Klärschlamm-Analyse* (FSC)_Rev.*.pdf"));
            dictDocs.Add("PSC-Zerlegung",new PathAndBox("C.1", "C.1 PSC-Zerlegung_Rev.*.pdf"));
            dictDocs.Add("Ablaufplan",new PathAndBox("C.1", "C.1 Ablaufpl*n*_Rev.*.pdf"));
            dictDocs.Add("Querkontamination",new PathAndBox("D.2", "D.2 Querkontamination_Rev.*.pdf"));
            dictDocs.Add("Gammamessung Rados",new PathAndBox("D.2", "D.2 Gammamessung* Rados_Rev.*.pdf"));
            dictDocs.Add("Prüfbericht Druckfestigkeit",new PathAndBox("D.2", "D.2 Prüfbericht* Druckfestigkeit_Rev.*.pdf"));
            dictDocs.Add("Prüfbericht Zementierfähigkeit",new PathAndBox("D.2", "D.2 Prüfbericht* Zementierfähigkeit_Rev.*.pdf"));
            dictDocs.Add("TNE-Stellungnahme NDKL",new PathAndBox("D.2", "D.2 TNE-Stellungnahme* NDKL*_Rev.*.pdf"));
            dictDocs.Add("Verifizierung NDKL C-Altchargen",new PathAndBox("D.2", "D.2 Verifizierung der NDKL der C-Altchargen_Rev.*.pdf"));
            dictDocs.Add("Behälterabnahme",new PathAndBox("D.3", "D.3 Abnahme Behälter_Rev.*.pdf"));
            dictDocs.Add("Reparaturen",new PathAndBox("D.3", "D.3 Reparaturen_Rev.*.pdf"));
            
            foreach(KeyValuePair<string,PathAndBox> kvp in dictDocs)
            {
                if(kvp.Key == "Abfalldatenblatt")
                {
                    kvp.Value.header = "A. ADB Rev./ADB_Rev,\nDoku-ID: /Doku_ID: Rev./Doku_Rev";
                    kvp.Value.footer = "Das Original ist an dieser Stelle rot gestempelt.";
                    kvp.Value.headerXPos = PageXPosition.Middle;
                    kvp.Value.footerXPos = PageXPosition.Left;
                    kvp.Value.vertPagesQuatifier = PageQuantifiers.NoneExceptArray;
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
                    kvp.Value.vertPagesQuatifier = PageQuantifiers.NoneExceptArray;
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
                case "None":
                    return PageQuantifiers.None;
                case "All":
                    return PageQuantifiers.All;
                case "NoneExceptArray":
                    return PageQuantifiers.NoneExceptArray;
                case "AllExceptArray":
                    return PageQuantifiers.AllExceptArray;
                default:
                    throw new System.Exception(string.Format("Error: Unknown PageQuantifier Text: {0}", txt));
            }
        }

        private int[] getIntArrayFromText(string txt)
        {
            string[] arrTxt = txt.Split(separator: new char[] {','}, options: StringSplitOptions.RemoveEmptyEntries);
            int[] arrInts = new int[arrTxt.Length];
            for(int i=0; i<arrTxt.Length; i++)
            {
                arrInts[i] = Convert.ToInt32(arrTxt[i]);
            }
            return arrInts;
        }

        private double[] getDoubleArrayFromText(string txt)
        {
            string[] arrTxt = txt.Split(separator: new char[] {','}, options: StringSplitOptions.RemoveEmptyEntries);
            double[] arrDoubles = new double[arrTxt.Length];
            for(int i=0; i<arrTxt.Length; i++)
            {
                arrDoubles[i] = Convert.ToDouble(arrTxt[i]);
            }
            return arrDoubles;
        }
    }
}