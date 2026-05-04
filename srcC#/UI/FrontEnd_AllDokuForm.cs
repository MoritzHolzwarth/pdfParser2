
namespace pdfParserByMH
{
    public class PathAndBox
    {
        public string fileName {get; set;}
        public string folderName {get; set;}
        public System.Windows.Forms.CheckBox box {get; set;}
        public PathAndBox(string folder = "", string file = "")
        {
            folderName = folder;
            fileName = file;
            box = new System.Windows.Forms.CheckBox();
        }
    }
    public partial class FrontEndProgram
    {
        private void makeAllDokuForm()
        {
            AllDokuForm = new System.Windows.Forms.TabPage("Doku Stempeln");
            AllDokuForm.Size = new System.Drawing.Size(800,700);
            AllDokuForm.Font = new System.Drawing.Font("Helvetica", 12);
            AllDokuForm.BackColor = System.Drawing.ColorTranslator.FromHtml("#E1F3F5");

            System.Windows.Forms.TextBox textbox = new System.Windows.Forms.TextBox();
            textbox.Name = "AllDoku_FolderPath";
            textbox.Location = new System.Drawing.Point(40,40);
            textbox.Size = new System.Drawing.Size(600,20);
            AllDokuForm.Controls.Add(textbox);

            System.Windows.Forms.Label textboxLabel = new System.Windows.Forms.Label();
            textboxLabel.Location = new System.Drawing.Point(40,10);
            textboxLabel.Text = "Pfad des Dokuordner";
            textboxLabel.AutoSize = true;
            AllDokuForm.Controls.Add(textboxLabel);

            allDokuForm_initCheckboxes();
            AllDokuForm_initButtons();
        }
        private void allDokuForm_initCheckboxes()
        {
            dictDocs = new System.Collections.Generic.Dictionary<string, PathAndBox>();
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
            dictDocs.Add("Ablaufplan",new PathAndBox("C.1", "C.2 Ablaufpl*n*_Rev.*.pdf"));
            dictDocs.Add("Querkontamination",new PathAndBox("D.2", "D.2 Querkontamination_Rev.*.pdf"));
            dictDocs.Add("Gammamessung Rados",new PathAndBox("D.2", "D.2 Gammamessung* Rados_Rev.*.pdf"));
            dictDocs.Add("Prüfbericht Druckfestigkeit",new PathAndBox("D.2", "D.2 Prüfbericht* Druckfestigkeit_Rev.*.pdf"));
            dictDocs.Add("Prüfbericht Zementierfähigkeit",new PathAndBox("D.2", "D.2 Prüfbericht* Zementierfähigkeit_Rev.*.pdf"));
            dictDocs.Add("TNE-Stellungnahme NDKL",new PathAndBox("D.2", "D.2 TNE-Stellungnahme* NDKL*_Rev.*.pdf"));
            dictDocs.Add("Verifizierung NDKL C-Altchargen",new PathAndBox("D.2", "D.2 Verifizierung der NDKL der C-Altchargen_Rev.*.pdf"));
            dictDocs.Add("Behälterabnahme",new PathAndBox("D.3", "D.3 Abnahme Behälter_Rev.*.pdf"));
            dictDocs.Add("Reparaturen",new PathAndBox("D.3", "D.3 Reparaturen_Rev.*.pdf"));

            int i = 0; int j = 0;
            foreach(string docName in dictDocs.Keys)
            {
                int yPos = 100 + 30*i;
                int xPos = 60 + 280*j;

                System.Windows.Forms.Label label = new System.Windows.Forms.Label();
                label.Location = new System.Drawing.Point(xPos, yPos);
                label.Text = dictDocs[docName].folderName + ": " + docName;
                label.AutoSize = true;
                AllDokuForm.Controls.Add(label);

                dictDocs[docName].box.Location = new System.Drawing.Point(xPos-20, yPos);
                dictDocs[docName].box.AutoSize = true;
                AllDokuForm.Controls.Add(dictDocs[docName].box);

                i = (i+1) % 13;
                if(i == 0)
                    j++;
            }
        }

        private void AllDokuForm_initButtons()
        {
            System.Windows.Forms.Button browseButton = new System.Windows.Forms.Button();
            browseButton.Location = new System.Drawing.Point(650,15);
            browseButton.Size = new System.Drawing.Size(100,50);
            browseButton.Text = "Browse";
            browseButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            browseButton.Click += (sender, e) =>
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    AllDokuForm.Controls["AllDoku_Textbox"].Text = dialog.SelectedPath;
                }
            };
            AllDokuForm.Controls.Add(browseButton);

            System.Windows.Forms.Button setAllButton = new System.Windows.Forms.Button();
            setAllButton.Location = new System.Drawing.Point(40,500);
            setAllButton.Size = new System.Drawing.Size(150,50);
            setAllButton.Text = "Alles Auswählen";
            setAllButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            setAllButton.Click += (sender, e) =>
            {
                foreach(PathAndBox pathbox in dictDocs.Values)
                    pathbox.box.Checked = true;
            };
            AllDokuForm.Controls.Add(setAllButton);

            System.Windows.Forms.Button unsetAllButton = new System.Windows.Forms.Button();
            unsetAllButton.Location = new System.Drawing.Point(250,500);
            unsetAllButton.Size = new System.Drawing.Size(150,50);
            unsetAllButton.Text = "Alles Abwählen";
            unsetAllButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            unsetAllButton.Click += (sender, e) =>
            {
                foreach(PathAndBox pathbox in dictDocs.Values)
                    pathbox.box.Checked = false;
            };
            AllDokuForm.Controls.Add(unsetAllButton);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Location = new System.Drawing.Point(460,500);
            okButton.Size = new System.Drawing.Size(100,50);
            okButton.Text = "OK";
            okButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            okButton.Click += (sender, e) =>
            {
                stempelOption = StempelOptions.DoAllDoku;
                form.DialogResult = System.Windows.Forms.DialogResult.OK;
                form.Close();
            };
            AllDokuForm.Controls.Add(okButton);
        }
    }
}