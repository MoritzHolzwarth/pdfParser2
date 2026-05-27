using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void makeDokuParamsForm(DocData docdata = null)
        {
            DokuParamsForm = new Form();
            DokuParamsForm.StartPosition = FormStartPosition.CenterScreen;
            DokuParamsForm.Font = new Font("Arial", 12);
            DokuParamsForm.AutoScaleMode = AutoScaleMode.Font;
            DokuParamsForm.Size = formSize;
            DokuParamsForm.BackColor = ColorTranslator.FromHtml("#E1F3F5");
            DokuParamsForm.AutoScroll = true;

            dictDocProperties = new Dictionary<DocPropertyType, DocProperty>();
            DokuParamsForm_makeFields();
        }

        private void DokuParamsForm_makeFields()
        {
            dictDocProperties.Clear();
            foreach(Control c in DokuParamsForm.Controls)
                c.Dispose();
            DokuParamsForm.Controls.Clear();

            int y = 10;
            y = DokuParamsForm_makeFilePathControl(y);
            y = DokuParamsForm_makeHeaderTextBoxControl(y + 10);
            y = DokuParamsForm_makeFooterTextBoxControl(y + 10);
            y = DokuParamsForm_makeVerticalPagesControl(y + 10);
            y = DokuParamsForm_makeHorizontalPagesControl(y + 10);
            y = DokuParamsForm_makePageScalingAndTextColorControl(y + 10);
            y = DokuParamsForm_makeOKButtonControl(y + 10);
        }

        private void DokuParamsForm_fillFields(DocData[] arrdocdata)
        {
            if(arrdocdata.Length == 1)
            {
                foreach(KeyValuePair<DocPropertyType,object> kvp in arrdocdata[0].dictDocProperties)
                {
                    dictDocProperties[kvp.Key].value = kvp.Value;
                    dictDocProperties[kvp.Key].write();
                }
            }
            else
            {
                foreach(DocProperty docprop in dictDocProperties.Values)
                    docprop.clear();
            }
        }

        private void DokuParamsForm_readFields()
        {
            foreach(DocProperty docprop in dictDocProperties.Values)
                docprop.read();
        }

        private int DokuParamsForm_makeFilePathControl(int y, DocData docdata = null)
        {
            int x = 40;
            Label folderTextBox_Label = new Label();
            DokuParamsForm.Controls.Add(folderTextBox_Label);
            folderTextBox_Label.Location = new Point(x, y);
            folderTextBox_Label.Text = "Ordner";
            folderTextBox_Label.AutoSize = true;

            x += folderTextBox_Label.PreferredWidth + 100;
            Label fileTextBox_label = new Label();
            DokuParamsForm.Controls.Add(fileTextBox_label);
            fileTextBox_label.Location = new Point(x, y);
            fileTextBox_label.Text = "Dateiname mit Platzhalter * zum Finden der Datei";
            fileTextBox_label.AutoSize = true;

            y += folderTextBox_Label.Height + 10;
            x = 40;
            TextBox folderTextBox = new TextBox();
            DokuParamsForm.Controls.Add(folderTextBox);
            dictDocProperties[DocPropertyType.FolderName] = new DocFolderName(folderTextBox);
            folderTextBox.Location = new Point(x, y);
            folderTextBox.Size = new Size(100, 20);

            x += folderTextBox_Label.PreferredWidth + 100;
            TextBox fileTextBox = new TextBox();
            DokuParamsForm.Controls.Add(fileTextBox);
            dictDocProperties[DocPropertyType.ApproxFileName] = new DocFileApproxName(fileTextBox);
            fileTextBox.Location = new Point(x, y);
            fileTextBox.Size = new Size(500, 20);

            y += folderTextBox.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeHeaderTextBoxControl(int y, DocData docdata = null)
        {
            int x = 40;
            Label textbox_Header_Label = new Label();
            DokuParamsForm.Controls.Add(textbox_Header_Label);
            textbox_Header_Label.Location = new Point(x, y);
            textbox_Header_Label.Text = "Kopfzeile";
            textbox_Header_Label.AutoSize = true;

            y += textbox_Header_Label.Height + 10;
            TextBox textbox_Header = new TextBox();
            DokuParamsForm.Controls.Add(textbox_Header);
            dictDocProperties[DocPropertyType.Header] = new DocHeader(textbox_Header);
            textbox_Header.Multiline = true;
            textbox_Header.AcceptsReturn = true;
            textbox_Header.AcceptsTab = true;
            textbox_Header.ScrollBars = ScrollBars.Vertical;
            textbox_Header.Location = new Point(x, y);
            textbox_Header.Size = new Size(700, 80);

            y += textbox_Header.Height + 10;
            GroupBox box_HeaderXPos = new System.Windows.Forms.GroupBox();
            DokuParamsForm.Controls.Add(box_HeaderXPos);
            box_HeaderXPos.Text = "Kopfzeile Position";
            box_HeaderXPos.Location = new System.Drawing.Point(x, y);
            box_HeaderXPos.Size = new System.Drawing.Size(700, 60);
            
            RadioButton rb_HeaderXLeft = new RadioButton();
            box_HeaderXPos.Controls.Add(rb_HeaderXLeft);
            RadioButton rb_HeaderXMiddle = new RadioButton();
            box_HeaderXPos.Controls.Add(rb_HeaderXMiddle);
            RadioButton rb_HeaderXRight = new RadioButton();
            box_HeaderXPos.Controls.Add(rb_HeaderXRight);
            dictDocProperties[DocPropertyType.HeaderXPos] = new DocHeaderXPos(new RadioButton[] {rb_HeaderXLeft, rb_HeaderXMiddle, rb_HeaderXRight});

            int y_boxlocal = 30;
            int x_boxlocal = 30;
            int rb_distance = 100;
            
            rb_HeaderXLeft.Text = "Links";
            rb_HeaderXLeft.Location = new Point(x_boxlocal, y_boxlocal);
            x_boxlocal += rb_HeaderXLeft.Size.Width + rb_distance;
            
            rb_HeaderXMiddle.Text = "Mittig";
            rb_HeaderXMiddle.Location = new Point(x_boxlocal, y_boxlocal);
            x_boxlocal += rb_HeaderXMiddle.Size.Width + rb_distance;
            
            rb_HeaderXRight.Text = "Rechts";
            rb_HeaderXRight.Location = new Point(x_boxlocal, y_boxlocal);

            y += box_HeaderXPos.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeFooterTextBoxControl(int y, DocData docdata = null)
        {
            int x = 40;
            Label textbox_Footer_Label = new Label();
            DokuParamsForm.Controls.Add(textbox_Footer_Label);
            textbox_Footer_Label.Location = new Point(x, y);
            textbox_Footer_Label.Text = "Fußzeile";
            textbox_Footer_Label.AutoSize = true;
            

            y += textbox_Footer_Label.Height + 10;
            TextBox textbox_Footer = new TextBox();
            DokuParamsForm.Controls.Add(textbox_Footer);
            dictDocProperties[DocPropertyType.Footer] = new DocFooter(textbox_Footer);
            textbox_Footer.Multiline = true;
            textbox_Footer.AcceptsReturn = true;
            textbox_Footer.AcceptsTab = true;
            textbox_Footer.ScrollBars = ScrollBars.Vertical;
            textbox_Footer.Location = new Point(x, y);
            textbox_Footer.Size = new Size(700, 80);
            

            y += textbox_Footer.Height + 10;
            GroupBox box_FooterXPos = new GroupBox();
            DokuParamsForm.Controls.Add(box_FooterXPos);
            box_FooterXPos.Text = "Fußzeile Position";
            box_FooterXPos.Location = new Point(x, y);
            box_FooterXPos.Size = new Size(700, 60);

            RadioButton rb_FooterXLeft = new RadioButton();
            box_FooterXPos.Controls.Add(rb_FooterXLeft);
            RadioButton rb_FooterXMiddle = new RadioButton();
            box_FooterXPos.Controls.Add(rb_FooterXMiddle);
            RadioButton rb_FooterXRight = new RadioButton();
            box_FooterXPos.Controls.Add(rb_FooterXRight);
            dictDocProperties[DocPropertyType.FooterXPos] = new DocFooterXPos(new RadioButton[] {rb_FooterXLeft, rb_FooterXMiddle, rb_FooterXRight});
            
            int y_boxlocal = 30;
            int x_boxlocal = 30;
            int rb_distance = 100;
            
            rb_FooterXLeft.Text = "Links";
            rb_FooterXLeft.Location = new Point(x_boxlocal, y_boxlocal);
            x_boxlocal += rb_FooterXLeft.Size.Width + rb_distance;
            rb_FooterXMiddle.Text = "Mittig";
            rb_FooterXMiddle.Location = new Point(x_boxlocal, y_boxlocal);
            x_boxlocal += rb_FooterXMiddle.Size.Width + rb_distance;
            rb_FooterXRight.Text = "Rechts";
            rb_FooterXRight.Location = new Point(x_boxlocal, y_boxlocal);
            
            y += box_FooterXPos.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeVerticalPagesControl(int y, DocData docdata = null)
        {
            int x = 40;
            GroupBox groupbox_vertical = new GroupBox();
            DokuParamsForm.Controls.Add(groupbox_vertical);
            groupbox_vertical.Text = "Seiten vertikal machen:";
            groupbox_vertical.Location = new Point(x, y);
            groupbox_vertical.Size = new Size(700,110);
            groupbox_vertical.Font = new Font("Arial", 12);

            RadioButton rb_NoneVertical = new RadioButton();
            groupbox_vertical.Controls.Add(rb_NoneVertical);
            RadioButton rb_AllVertical = new RadioButton();
            groupbox_vertical.Controls.Add(rb_AllVertical);
            dictDocProperties[DocPropertyType.VertPageQuant] = new DocVertPageQuantifier(new RadioButton[] {rb_NoneVertical, rb_AllVertical});

            int x_boxlocal = 30;
            int y_boxlocal = 30;
            int rb_distance = 150;
            
            rb_NoneVertical.Text = "Nur angegebene Seiten";
            rb_NoneVertical.Location = new Point(x_boxlocal, y_boxlocal);
            rb_NoneVertical.AutoSize = true;
            x_boxlocal += rb_NoneVertical.Size.Width + rb_distance;
            rb_AllVertical.Text = "Alle außer angegebene Seiten";
            rb_AllVertical.Location = new Point(x_boxlocal, y_boxlocal);
            rb_AllVertical.AutoSize = true;

            Label verticalPages_Label = new Label();
            groupbox_vertical.Controls.Add(verticalPages_Label);
            x_boxlocal = 10;
            y_boxlocal += rb_AllVertical.Size.Height + 10;
            verticalPages_Label.Location = new Point(x_boxlocal, y_boxlocal);
            verticalPages_Label.Text = "Seiten (z.B. 1, 3, 12, ...)";
            verticalPages_Label.AutoSize = true;
            x_boxlocal += verticalPages_Label.Width + 10;

            TextBox textbox_verticalPages = new TextBox();
            groupbox_vertical.Controls.Add(textbox_verticalPages);
            dictDocProperties[DocPropertyType.VertPageNumbers] = new DocVertPageNumbers(textbox_verticalPages);
            textbox_verticalPages.Location = new Point(x_boxlocal, y_boxlocal);
            textbox_verticalPages.Size = new Size(400,20);

            y += groupbox_vertical.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeHorizontalPagesControl(int y, DocData docdata = null)
        {
            int x = 40;
            GroupBox groupbox_horizontal = new GroupBox();
            DokuParamsForm.Controls.Add(groupbox_horizontal);
            groupbox_horizontal.Text = "Seiten horizontal machen:";
            groupbox_horizontal.Location = new Point(x, y);
            groupbox_horizontal.Size = new Size(700,110);

            RadioButton rb_NoneHorizontal = new RadioButton();
            groupbox_horizontal.Controls.Add(rb_NoneHorizontal);
            RadioButton rb_AllHorizontal = new RadioButton();
            groupbox_horizontal.Controls.Add(rb_AllHorizontal);
            dictDocProperties[DocPropertyType.HoriPageQuant] = new DocHoriPageQuantifier(new RadioButton[] {rb_NoneHorizontal, rb_AllHorizontal});

            int x_boxlocal = 30;
            int y_boxlocal = 30;
            int rb_distance = 150;
            
            rb_NoneHorizontal.Text = "Nur angegebene Seiten";
            rb_NoneHorizontal.Location = new Point(x_boxlocal, y_boxlocal);
            rb_NoneHorizontal.AutoSize = true;
            x_boxlocal += rb_NoneHorizontal.Size.Width + rb_distance;
            rb_AllHorizontal.Text = "Alle außer angegebene Seiten";
            rb_AllHorizontal.Location = new Point(x_boxlocal, y_boxlocal);
            rb_AllHorizontal.AutoSize = true;

            Label horizontalPages_Label = new Label();
            groupbox_horizontal.Controls.Add(horizontalPages_Label);
            x_boxlocal = 10;
            y_boxlocal += rb_AllHorizontal.Size.Height + 10;
            horizontalPages_Label.Location = new Point(x_boxlocal, y_boxlocal);
            horizontalPages_Label.Text = "Seiten (z.B. 1, 3, 12, ...)";
            horizontalPages_Label.AutoSize = true;
            x_boxlocal += horizontalPages_Label.Width + 10;

            TextBox textbox_horizontalPages = new TextBox();
            groupbox_horizontal.Controls.Add(textbox_horizontalPages);
            dictDocProperties[DocPropertyType.HoriPageNumbers] = new DocHoriPageNumbers(textbox_horizontalPages);
            textbox_horizontalPages.Location = new Point(x_boxlocal, y_boxlocal);
            textbox_horizontalPages.Size = new Size(400,20);

            y += groupbox_horizontal.Size.Height;
            return y;
        }

        private int DokuParamsForm_makePageScalingAndTextColorControl(int y, DocData docdata = null)
        {
            int x = 40;
            Label scalierung_Label = new Label();
            DokuParamsForm.Controls.Add(scalierung_Label);
            scalierung_Label.Location = new Point(x, y);
            scalierung_Label.Text = "Seitenskalierung Faktor:";
            scalierung_Label.AutoSize = true;
            x += scalierung_Label.Width + 5;

            TextBox textbox_Scalierung = new TextBox();
            DokuParamsForm.Controls.Add(textbox_Scalierung);
            dictDocProperties[DocPropertyType.ScaleFactor] = new DocScaleFactor(textbox_Scalierung);
            textbox_Scalierung.Location = new Point(x, y);
            textbox_Scalierung.Size = new Size(100,20);
            x += textbox_Scalierung.Width + 100;
            
            Label colorLabel = new Label();
            DokuParamsForm.Controls.Add(colorLabel);
            colorLabel.Location = new Point(x, y);
            colorLabel.Text = "Textfarbe:";
            colorLabel.AutoSize = true;
            x += colorLabel.Width + 5;

            Label colorName = new Label();
            DokuParamsForm.Controls.Add(colorName);
            dictDocProperties[DocPropertyType.TextColor] = new DocTextColor(colorName);
            colorName.Location = new Point(x, y);
            colorName.AutoSize = true;
            colorName.PerformLayout();
            colorName.Text = DocProperty.unv; //This Label is not just a Label. Its Text stores the Textcolor. Its text is initially set to '/unverändert'.
            x += colorName.Width + 5;

            Button colorButton = new Button();
            DokuParamsForm.Controls.Add(colorButton);
            colorButton.Location = new Point(x, y);
            colorButton.Size = new Size(100,50);
            colorButton.Text = "Farbe Wählen";
            colorButton.BackColor = ColorTranslator.FromHtml("#C2EAF0");
            colorButton.Click += (sender, e) =>
            {
                ColorDialog dialog = new ColorDialog();
                dialog.AllowFullOpen = true;
                dialog.FullOpen = true;
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    string colorHex = string.Format("#{0:X2}{1:X2}{2:X2}", dialog.Color.R, dialog.Color.G, dialog.Color.B);
                    colorName.Text = colorHex;
                    colorName.ForeColor = dialog.Color;
                }
            };
            
            int[] heights = new int[] {scalierung_Label.Height, 
                                    textbox_Scalierung.Height, 
                                    colorName.Height, 
                                    colorButton.Size.Height};
            y += heights.Max();
            return y;
        }

        private int DokuParamsForm_makeOKButtonControl(int y)
        {
            int x = 350;
            Button okButton = new Button();
            DokuParamsForm.Controls.Add(okButton);
            okButton.Location = new System.Drawing.Point(x, y);
            okButton.Size = new System.Drawing.Size(100,50);
            okButton.Text = "Speichern";
            okButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            okButton.Click += (sender, e) =>
            {
                DokuParamsForm.DialogResult = DialogResult.OK;
                DokuParamsForm.Close();
            };
            y += okButton.Size.Height;
            return y;
        }

        
    }
}