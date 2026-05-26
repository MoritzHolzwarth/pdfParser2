
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace pdfParserByMH
{

    
    public partial class FrontEndProgram
    {
        private void makeAllDokuForm()
        {
            AllDokuForm = new TabPage();
            AllDokuForm.Size = new Size(800, 700);
            AllDokuForm.Font = new Font("Helvetica", 12);
            AllDokuForm.BackColor = ColorTranslator.FromHtml("#E1F3F5");
            AllDokuForm.Text = "Doku Stempeln";
            AllDokuForm_makeFields();
            AllDokuForm_fillFields();
        }

        private void AllDokuForm_makeFields()
        {
            foreach(Control c in AllDokuForm.Controls) //resetting current fields
                c.Dispose();
            AllDokuForm.Controls.Clear();

            HashSet<string> setFolderNames = new HashSet<string>();
            foreach(DocData docdata in dictDocs.Values)
                setFolderNames.Add(docdata.folderName);

            dictDocCheckboxes = new Dictionary<string, CheckBox>();
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
                dictDocs[kvp.Key].doStempeln = kvp.Value.Checked;
            }
        }

        private void AllDokuForm_fillFields()
        {
            AllDokuForm.Controls["AllDoku_FolderPath"].Text = dokuFolderPath;
            ((CheckBox)AllDokuForm.Controls["Override_Files"]).Checked = overrideDokuFiles;
            foreach(KeyValuePair<string,CheckBox> kvp in dictDocCheckboxes)
            {
                kvp.Value.Checked = dictDocs[kvp.Key].doStempeln;
            }
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
                foreach(CheckBox cb in dictDocCheckboxes.Values)
                    cb.Checked = true;
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
                progState = ProgramState.DoAllDoku;
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
                foreach(CheckBox cb in dictDocCheckboxes.Values)
                    cb.Checked = false;
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
                progState = ProgramState.AdjustDokuParams;
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
            groubboxFolder.Location = new Point(x, y);
            groubboxFolder.Size = new Size(700, 10);
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
                CheckBox chb = new CheckBox();
                chb.AutoSize = true;
                groubboxFolder.Controls.Add(docName);
                groubboxFolder.Controls.Add(chb);
                dictDocCheckboxes.Add(kvp.Key, chb);

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
    }
}