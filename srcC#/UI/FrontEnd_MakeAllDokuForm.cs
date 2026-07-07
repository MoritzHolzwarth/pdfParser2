
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;


namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void makeAllDokuForm()
        {
            AllDokuForm = new TabPage();
            tabControl.Controls.Add(AllDokuForm);
            AllDokuForm.BackColor = ColorTranslator.FromHtml("#E1F3F5");
            AllDokuForm.Text = "Doku Stempeln";

            dictDocCheckboxes = new Dictionary<string, CheckBox>();
            AllDokuForm_makeFields();
            AllDokuForm_fillFields();
        }

        private void AllDokuForm_makeFields()
        {
            dictDocCheckboxes.Clear();
            foreach(Control c in AllDokuForm.Controls) //resetting current fields
                c.Dispose();
            AllDokuForm.Controls.Clear();

            HashSet<string> setFolderNames = new HashSet<string>();
            foreach(DocData docdata in dictDocs.Values)
                setFolderNames.Add(docdata.folderName);

            int y = 10;
            y = AllDokuForm_makeDokuFolderPathControl(y);
            foreach(string folderName in setFolderNames)
                y = AllDokuForm_makeFolderCheckboxes(y + 10, folderName);
            
            y = AllDokuForm_makeButtons(y + 10);
        }

        private void AllDokuForm_readFields()
        {
            dokuFolderPath = AllDokuForm.Controls["AllDoku_FolderPath"].Text;
            overrideDokuFiles = ((CheckBox)AllDokuForm.Controls["Override_Files"]).Checked;
            foreach(KeyValuePair<string,CheckBox> kvp in dictDocCheckboxes)
            {
                dictDocs[kvp.Key].selected = kvp.Value.Checked;
            }
        }

        private void AllDokuForm_fillFields()
        {
            AllDokuForm.Controls["AllDoku_FolderPath"].Text = dokuFolderPath;
            ((CheckBox)AllDokuForm.Controls["Override_Files"]).Checked = overrideDokuFiles;
            foreach(KeyValuePair<string,CheckBox> kvp in dictDocCheckboxes)
            {
                kvp.Value.Checked = dictDocs[kvp.Key].selected;
            }
        }

        private int AllDokuForm_makeButtons(int y)
        {
            int x = 100;

            Button selectAllButton = new Button();
            AllDokuForm.Controls.Add(selectAllButton);
            selectAllButton.Location = new Point(x, y);
            selectAllButton.Size = new Size(100, 50);
            selectAllButton.Text = "Alles Auswählen";
            selectAllButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            selectAllButton.Click += (sender, e) =>
            {
                foreach(CheckBox cb in dictDocCheckboxes.Values)
                    cb.Checked = true;
            };

            x += selectAllButton.Size.Width + 200;
            Button stempelButton = new Button();
            AllDokuForm.Controls.Add(stempelButton);
            stempelButton.Location = new Point(x, y);
            stempelButton.Size = new Size(100,50);
            stempelButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            stempelButton.Text = "Stempeln";
            stempelButton.Click += (sender, e) =>
            {
                progState = ProgramState.DoAllDoku;
                form.DialogResult = DialogResult.OK;
                form.Close();
            };

            y += selectAllButton.Size.Height + 10;
            x = 100;
            Button selectNoneButton = new Button();
            AllDokuForm.Controls.Add(selectNoneButton);
            selectNoneButton.Location = new Point(x, y);
            selectNoneButton.Size = new Size(100, 50);
            selectNoneButton.Text = "Alles Abwählen";
            selectNoneButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            selectNoneButton.Click += (sender, e) =>
            {
                foreach(CheckBox cb in dictDocCheckboxes.Values)
                    cb.Checked = false;
            };

            x += stempelButton.Size.Width + 200;
            Button stempelParamsButton = new Button();
            AllDokuForm.Controls.Add(stempelParamsButton);
            stempelParamsButton.Location = new Point(x, y);
            stempelParamsButton.Size = new Size(100, 50);
            stempelParamsButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            stempelParamsButton.Text = "Parameter Anpassen";
            stempelParamsButton.Click += (sender, e) =>
            {
                progState = ProgramState.AdjustDokuParams;
                form.DialogResult = DialogResult.OK;
                form.Close();
            };

            y += stempelParamsButton.Size.Height;
            Button resetParamsButton = new Button();
            AllDokuForm.Controls.Add(resetParamsButton);
            resetParamsButton.Location = new Point(x, y);
            resetParamsButton.Size = new Size(100, 50);
            resetParamsButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            resetParamsButton.Text = "Reset Alle Parameter";
            resetParamsButton.Click += (sender, e) =>
            {
                fillDictDocsFromDefault();
                saveData(true);
            };
            y += resetParamsButton.Size.Height;
            
            return y;
        }

        private int AllDokuForm_makeDokuFolderPathControl(int y)
        {
            int x = 40;
            Label textboxLabel = new System.Windows.Forms.Label();
            AllDokuForm.Controls.Add(textboxLabel);
            textboxLabel.Location = new System.Drawing.Point(x, y);
            textboxLabel.Text = "Pfad des Dokuordner";
            textboxLabel.AutoSize = true;

            x += textboxLabel.PreferredWidth + 300;
            CheckBox overrideFile_Checkbox = new CheckBox();
            AllDokuForm.Controls.Add(overrideFile_Checkbox);
            overrideFile_Checkbox.Name = "Override_Files";
            overrideFile_Checkbox.Text = "Dateien überschreiben";
            overrideFile_Checkbox.Location = new Point(x, y);
            overrideFile_Checkbox.AutoSize = true;

            y += Math.Max(textboxLabel.Height, overrideFile_Checkbox.Height) + 10;
            x = 40;
            TextBox textbox = new System.Windows.Forms.TextBox();
            AllDokuForm.Controls.Add(textbox);
            textbox.Name = "AllDoku_FolderPath";
            textbox.Location = new System.Drawing.Point(x, y);
            textbox.Size = new System.Drawing.Size(600,20);

            x += textbox.Size.Width + 10;
            Button browseButton = new Button();
            AllDokuForm.Controls.Add(browseButton);
            browseButton.Location = new Point(x, y);
            browseButton.Size = new Size(100,50);
            browseButton.Text = "Browse";
            browseButton.BackColor = ColorTranslator.FromHtml("#C2EAF0");
            browseButton.Click += (sender, e) =>
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    textbox.Text = dialog.SelectedPath;
                }
            };

            y += Math.Max(textbox.PreferredHeight, browseButton.PreferredSize.Height);
            return y;
        }

        private int AllDokuForm_makeFolderCheckboxes(int y, string folderName)
        {
            int x = 40;
            GroupBox groubboxFolder = new GroupBox();
            AllDokuForm.Controls.Add(groubboxFolder);
            groubboxFolder.Name = "AllDoku_FolderBox_" + folderName;
            groubboxFolder.Text = folderName;
            groubboxFolder.Location = new Point(x, y);
            groubboxFolder.Size = new Size(700, 10);

            int x_boxlocal = 20;
            int y_boxlocal = 20;
            int xdistance = 20;
            int ydistance = 10;
            Size cbsize = new Size();
            foreach(KeyValuePair<string,DocData> kvp in dictDocs)
            {
                if(kvp.Value.folderName != folderName)
                    continue;
                CheckBox chb = new CheckBox();
                groubboxFolder.Controls.Add(chb);
                dictDocCheckboxes.Add(kvp.Key, chb);
                chb.AutoSize = true;
                chb.Text = kvp.Key;
                chb.AutoSize = true;
                cbsize = chb.Size;
                
                if(x_boxlocal + cbsize.Width > groubboxFolder.Size.Width)
                {
                    y_boxlocal += cbsize.Height + ydistance;
                    x_boxlocal = 20;
                }
                chb.Location = new Point(x_boxlocal, y_boxlocal);
                x_boxlocal += cbsize.Width + xdistance;
            }
            y_boxlocal += cbsize.Height + ydistance;
            if(y_boxlocal > groubboxFolder.Size.Height)
                groubboxFolder.Size = new Size(groubboxFolder.Size.Width, y_boxlocal + ydistance);
            
            y += groubboxFolder.Size.Height;
            return y;
        }
    }
}
