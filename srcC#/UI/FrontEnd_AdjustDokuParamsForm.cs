using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System;
using System.Data;
using System.Collections.Generic;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void makeDokuParamsForm(DocData docdata = null)
        {
            dictDocProperties = new Dictionary<DocPropertyType, DocProperty>();

            DokuParamsForm = new Form();
            DokuParamsForm.StartPosition = FormStartPosition.CenterScreen;
            form.AutoScaleMode = AutoScaleMode.Font;
            DokuParamsForm.Size = new Size(800, 950);
            DokuParamsForm.Font = new Font("Helvetica", 12);
            DokuParamsForm.BackColor = ColorTranslator.FromHtml("#E1F3F5");
            DokuParamsForm.AutoScroll = true;

            int y = 10;
            y = DokuParamsForm_makeFilePathControl(y, docdata);
            y = DokuParamsForm_makeHeaderTextBoxControl(y + 10, docdata);
            y = DokuParamsForm_makeFooterTextBoxControl(y + 10, docdata);
            y = DokuParamsForm_makeVerticalPagesControl(y + 10, docdata);
            y = DokuParamsForm_makeHorizontalPagesControl(y + 10, docdata);
            y = DokuParamsForm_makePageScalingAndTextColorControl(y + 10, docdata);
            y = DokuParamsForm_makeOKButtonControl(y + 10);
        }

        private void fillDokuParamsForm(DocData docdata = null)
        {
            if(docdata != null)
            {
                foreach(KeyValuePair<DocPropertyType,object> kvp in docdata.dictDocProperties)
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

        private int DokuParamsForm_makeFilePathControl(int y, DocData docdata = null)
        {
            int x = 40;
            System.Windows.Forms.Label folderTextBox_Label = new System.Windows.Forms.Label();
            folderTextBox_Label.Location = new System.Drawing.Point(x, y);
            folderTextBox_Label.Text = "Ordner";
            folderTextBox_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(folderTextBox_Label);

            x += folderTextBox_Label.PreferredWidth + 100;
            System.Windows.Forms.Label fileTextBox_label = new System.Windows.Forms.Label();
            fileTextBox_label.Location = new System.Drawing.Point(x, y);
            fileTextBox_label.Text = "Dateiname mit Platzhalter * zum Finden der Datei";
            fileTextBox_label.AutoSize = true;
            DokuParamsForm.Controls.Add(fileTextBox_label);

            y += folderTextBox_Label.Height + 10;
            x = 40;
            TextBox folderTextBox = new TextBox();
            folderTextBox.Location = new Point(x, y);
            folderTextBox.Size = new Size(100, 20);
            DokuParamsForm.Controls.Add(folderTextBox);
            dictDocProperties[DocPropertyType.FolderName] = new DocFolderName(folderTextBox);

            x += folderTextBox_Label.PreferredWidth + 100;
            System.Windows.Forms.TextBox fileTextBox = new TextBox();
            fileTextBox.Location = new Point(x, y);
            fileTextBox.Size = new Size(500, 20);
            DokuParamsForm.Controls.Add(fileTextBox);
            dictDocProperties[DocPropertyType.ApproxFileName] = new DocFileApproxName(fileTextBox);

            y += folderTextBox.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeHeaderTextBoxControl(int y, DocData docdata = null)
        {
            int x = 40;
            System.Windows.Forms.Label textbox_Header_Label = new Label();
            textbox_Header_Label.Location = new Point(x, y);
            textbox_Header_Label.Text = "Kopfzeile";
            textbox_Header_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(textbox_Header_Label);

            y += textbox_Header_Label.Height + 10;
            System.Windows.Forms.TextBox textbox_Header = new TextBox();
            textbox_Header.Multiline = true;
            textbox_Header.AcceptsReturn = true;
            textbox_Header.AcceptsTab = true;
            textbox_Header.ScrollBars = ScrollBars.Vertical;
            textbox_Header.Location = new Point(x, y);
            textbox_Header.Size = new Size(700, 80);
            DokuParamsForm.Controls.Add(textbox_Header);
            dictDocProperties[DocPropertyType.Header] = new DocHeader(textbox_Header);

            y += textbox_Header.Height + 10;
            GroupBox box_HeaderXPos = new System.Windows.Forms.GroupBox();
            box_HeaderXPos.Text = "Kopfzeile Position";
            box_HeaderXPos.Location = new System.Drawing.Point(x, y);
            box_HeaderXPos.Size = new System.Drawing.Size(700, 60);
            
            int y_boxlocal = 30;
            int x_boxlocal = 30;
            int rb_distance = 100;
            RadioButton rb_HeaderXLeft = new RadioButton();
            rb_HeaderXLeft.Text = "Links";
            rb_HeaderXLeft.Location = new Point(x_boxlocal, y_boxlocal);
            box_HeaderXPos.Controls.Add(rb_HeaderXLeft);

            x_boxlocal += rb_HeaderXLeft.Size.Width + rb_distance;
            RadioButton rb_HeaderXMiddle = new RadioButton();
            rb_HeaderXMiddle.Text = "Mittig";
            rb_HeaderXMiddle.Location = new Point(x_boxlocal, y_boxlocal);
            box_HeaderXPos.Controls.Add(rb_HeaderXMiddle);

            x_boxlocal += rb_HeaderXMiddle.Size.Width + rb_distance;
            RadioButton rb_HeaderXRight = new RadioButton();
            rb_HeaderXRight.Text = "Rechts";
            rb_HeaderXRight.Location = new Point(x_boxlocal, y_boxlocal);
            box_HeaderXPos.Controls.Add(rb_HeaderXRight);

            DokuParamsForm.Controls.Add(box_HeaderXPos);
            dictDocProperties[DocPropertyType.HeaderXPos] = new DocHeaderXPos(new RadioButton[] {rb_HeaderXLeft, rb_HeaderXMiddle, rb_HeaderXRight});

            y += box_HeaderXPos.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeFooterTextBoxControl(int y, DocData docdata = null)
        {
            int x = 40;
            Label textbox_Footer_Label = new Label();
            textbox_Footer_Label.Location = new Point(x, y);
            textbox_Footer_Label.Text = "Fußzeile";
            textbox_Footer_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(textbox_Footer_Label);

            y += textbox_Footer_Label.Height + 10;
            TextBox textbox_Footer = new TextBox();
            textbox_Footer.Multiline = true;
            textbox_Footer.AcceptsReturn = true;
            textbox_Footer.AcceptsTab = true;
            textbox_Footer.ScrollBars = ScrollBars.Vertical;
            textbox_Footer.Location = new Point(x, y);
            textbox_Footer.Size = new Size(700, 80);
            DokuParamsForm.Controls.Add(textbox_Footer);
            dictDocProperties[DocPropertyType.Footer] = new DocFooter(textbox_Footer);

            y += textbox_Footer.Height + 10;
            System.Windows.Forms.GroupBox box_FooterXPos = new GroupBox();
            box_FooterXPos.Text = "Fußzeile Position";
            box_FooterXPos.Location = new Point(x, y);
            box_FooterXPos.Size = new Size(700, 60);
            
            int y_boxlocal = 30;
            int x_boxlocal = 30;
            int rb_distance = 100;
            RadioButton rb_FooterXLeft = new RadioButton();
            rb_FooterXLeft.Text = "Links";
            rb_FooterXLeft.Location = new Point(x_boxlocal, y_boxlocal);
            box_FooterXPos.Controls.Add(rb_FooterXLeft);

            x_boxlocal += rb_FooterXLeft.Size.Width + rb_distance;
            RadioButton rb_FooterXMiddle = new RadioButton();
            rb_FooterXMiddle.Text = "Mittig";
            rb_FooterXMiddle.Location = new Point(x_boxlocal, y_boxlocal);
            box_FooterXPos.Controls.Add(rb_FooterXMiddle);

            x_boxlocal += rb_FooterXMiddle.Size.Width + rb_distance;
            RadioButton rb_FooterXRight = new RadioButton();
            rb_FooterXRight.Text = "Rechts";
            rb_FooterXRight.Location = new Point(x_boxlocal, y_boxlocal);
            box_FooterXPos.Controls.Add(rb_FooterXRight);

            DokuParamsForm.Controls.Add(box_FooterXPos);
            dictDocProperties[DocPropertyType.FooterXPos] = new DocFooterXPos(new RadioButton[] {rb_FooterXLeft, rb_FooterXMiddle, rb_FooterXRight});

            y += box_FooterXPos.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeVerticalPagesControl(int y, DocData docdata = null)
        {
            int x = 40;
            GroupBox groupbox_vertical = new GroupBox();
            groupbox_vertical.Text = "Seiten vertikal machen:";
            groupbox_vertical.Location = new Point(x, y);
            groupbox_vertical.Size = new Size(700,110);

            int x_boxlocal = 30;
            int y_boxlocal = 30;
            int rb_distance = 150;
            RadioButton rb_NoneVertical = new RadioButton();
            rb_NoneVertical.Text = "Nur angegebene Seiten";
            rb_NoneVertical.Location = new Point(x_boxlocal, y_boxlocal);
            rb_NoneVertical.Size = new Size(200,20);
            groupbox_vertical.Controls.Add(rb_NoneVertical);

            x_boxlocal += rb_NoneVertical.Size.Width + rb_distance;
            System.Windows.Forms.RadioButton rb_AllVertical = new RadioButton();
            rb_AllVertical.Text = "Alle außer angegebene Seiten";
            rb_AllVertical.Location = new Point(x_boxlocal, y_boxlocal);
            rb_AllVertical.Size = new Size(250,20);
            groupbox_vertical.Controls.Add(rb_AllVertical);

            x_boxlocal = 10;
            y_boxlocal += rb_AllVertical.Size.Height + 10;
            System.Windows.Forms.Label verticalPages_Label = new Label();
            verticalPages_Label.Location = new Point(x_boxlocal, y_boxlocal);
            verticalPages_Label.Text = "Seiten (z.B. 1, 3, 12, ...)";
            verticalPages_Label.AutoSize = true;
            groupbox_vertical.Controls.Add(verticalPages_Label);

            x_boxlocal += verticalPages_Label.PreferredWidth + 10;
            System.Windows.Forms.TextBox textbox_verticalPages = new TextBox();
            textbox_verticalPages.Location = new Point(x_boxlocal, y_boxlocal);
            textbox_verticalPages.Size = new Size(400,20);
            groupbox_vertical.Controls.Add(textbox_verticalPages);

            DokuParamsForm.Controls.Add(groupbox_vertical);
            dictDocProperties[DocPropertyType.VertPageQuant] = new DocVertPageQuantifier(new RadioButton[] {rb_NoneVertical, rb_AllVertical});
            dictDocProperties[DocPropertyType.VertPageNumbers] = new DocVertPageNumbers(textbox_verticalPages);

            y += groupbox_vertical.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeHorizontalPagesControl(int y, DocData docdata = null)
        {
            int x = 40;
            System.Windows.Forms.GroupBox groupbox_horizontal = new GroupBox();
            groupbox_horizontal.Text = "Seiten horizontal machen:";
            groupbox_horizontal.Location = new Point(x, y);
            groupbox_horizontal.Size = new Size(700,110);

            int x_boxlocal = 30;
            int y_boxlocal = 30;
            int rb_distance = 150;
            RadioButton rb_NoneHorizontal = new RadioButton();
            rb_NoneHorizontal.Text = "Nur angegebene Seiten";
            rb_NoneHorizontal.Location = new Point(x_boxlocal, y_boxlocal);
            rb_NoneHorizontal.Size = new Size(200,20);
            rb_NoneHorizontal.Checked = false;
            groupbox_horizontal.Controls.Add(rb_NoneHorizontal);

            x_boxlocal += rb_NoneHorizontal.Size.Width + rb_distance;
            RadioButton rb_AllHorizontal = new RadioButton();
            rb_AllHorizontal.Text = "Alle außer angegebene Seiten";
            rb_AllHorizontal.Location = new Point(x_boxlocal, y_boxlocal);
            rb_AllHorizontal.Size = new Size(250,20);
            rb_AllHorizontal.Checked = false;
            groupbox_horizontal.Controls.Add(rb_AllHorizontal);

            x_boxlocal = 10;
            y_boxlocal += rb_AllHorizontal.Size.Height + 10;
            Label horizontalPages_Label = new Label();
            horizontalPages_Label.Location = new Point(x_boxlocal, y_boxlocal);
            horizontalPages_Label.Text = "Seiten (z.B. 1, 3, 12, ...)";
            horizontalPages_Label.AutoSize = true;
            groupbox_horizontal.Controls.Add(horizontalPages_Label);

            x_boxlocal += horizontalPages_Label.PreferredWidth + 10;
            TextBox textbox_horizontalPages = new TextBox();
            textbox_horizontalPages.Location = new Point(x_boxlocal, y_boxlocal);
            textbox_horizontalPages.Size = new Size(400,20);
            groupbox_horizontal.Controls.Add(textbox_horizontalPages);

            DokuParamsForm.Controls.Add(groupbox_horizontal);
            dictDocProperties[DocPropertyType.HoriPageQuant] = new DocHoriPageQuantifier(new RadioButton[] {rb_NoneHorizontal, rb_AllHorizontal});
            dictDocProperties[DocPropertyType.HoriPageNumbers] = new DocHoriPageNumbers(textbox_horizontalPages);

            y += groupbox_horizontal.Size.Height;
            return y;
        }

        private int DokuParamsForm_makePageScalingAndTextColorControl(int y, DocData docdata = null)
        {
            int x = 40;
            Label scalierung_Label = new Label();
            scalierung_Label.Location = new Point(x, y);
            scalierung_Label.Text = "Seitenskalierung Faktor:";
            scalierung_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(scalierung_Label);

            x += scalierung_Label.PreferredWidth + 5;
            TextBox textbox_Scalierung = new TextBox();
            textbox_Scalierung.Location = new Point(x, y);
            textbox_Scalierung.Size = new Size(100,20);
            DokuParamsForm.Controls.Add(textbox_Scalierung);
            dictDocProperties[DocPropertyType.ScaleFactor] = new DocScaleFactor(textbox_Scalierung);

            x += textbox_Scalierung.PreferredSize.Width + 100;
            Label colorLabel = new Label();
            colorLabel.Location = new Point(x, y);
            colorLabel.Text = "Textfarbe:";
            colorLabel.AutoSize = true;
            DokuParamsForm.Controls.Add(colorLabel);

            x += colorLabel.PreferredWidth + 5;
            Label colorName = new Label();
            colorName.Location = new Point(x, y);
            colorName.AutoSize = true;
            colorName.Text = DocProperty.unv;
            DokuParamsForm.Controls.Add(colorName);
            dictDocProperties[DocPropertyType.TextColor] = new DocTextColor(colorName);

            x += colorName.PreferredWidth + 10;
            Button colorButton = new Button();
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
            DokuParamsForm.Controls.Add(colorButton);

            int[] heights = new int[] {scalierung_Label.Height, 
                                    textbox_Scalierung.Height, 
                                    colorName.Height, 
                                    colorButton.Size.Height};
            y += heights.Max();
            return y;
        }

        string rgbDoubleColorToHexStringColor(double[] rgb)
        {
            int[] RGB = new int[rgb.Length];
            for(int i=0; i<3; i++)
            {
                if(rgb[i] < 0)
                    throw new System.Exception("Error in rgbDoubleColorToHexStringColor(): rgb value < 0!");
                if(rgb[i] >= 1)
                    RGB[i] = 255;
                else
                    RGB[i] = (int)rgb[i]*255;
            }
            return string.Format("#{0:X2}{1:X2}{2:X2}", RGB[0], RGB[1], RGB[2]);
        }

        private int DokuParamsForm_makeOKButtonControl(int y)
        {
            int x = 350;
            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Location = new System.Drawing.Point(x, y);
            okButton.Size = new System.Drawing.Size(100,50);
            okButton.Text = "OK";
            okButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            okButton.Click += (sender, e) =>
            {
                stempelOption = StempelOptions.Null;
                DokuParamsForm.DialogResult = System.Windows.Forms.DialogResult.OK;
                DokuParamsForm.Close();
            };
            DokuParamsForm.Controls.Add(okButton);

            y += okButton.Size.Height;
            return y;
        }
    }
}