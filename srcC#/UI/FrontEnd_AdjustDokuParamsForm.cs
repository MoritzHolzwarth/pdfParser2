using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System;
using System.Data;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void makeDokuParamsForm(DocData docdata = null)
        {
            DokuParamsForm = new System.Windows.Forms.Form();
            DokuParamsForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            DokuParamsForm.Size = new System.Drawing.Size(800, 950);
            DokuParamsForm.Font = new System.Drawing.Font("Helvetica", 12);
            DokuParamsForm.BackColor = System.Drawing.ColorTranslator.FromHtml("#E1F3F5");
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
            TextBox folderTextBox = (TextBox)DokuParamsForm.Controls["DocFolderName"];
            folderTextBox.Text = (docdata==null)? "/unverändert": docdata.folderName;

            TextBox fileTextBox = (TextBox)DokuParamsForm.Controls["DocApproxFileName"];
            fileTextBox.Text = (docdata==null)? "/unverändert": docdata.fileApproxName;

            TextBox textbox_Header = (TextBox)DokuParamsForm.Controls["DocHeader"];
            textbox_Header.Text = (docdata==null)? "/unverändert": docdata.header;
            GroupBox box_HeaderXPos = (GroupBox)DokuParamsForm.Controls["box_HeaderXPos"];
            RadioButton rb_HeaderXLeft = (RadioButton)box_HeaderXPos.Controls["RB_XLeft"];
            rb_HeaderXLeft.Checked = (docdata==null)? false: (docdata.headerXPos==PageXPosition.Left);
            RadioButton rb_HeaderXMiddle = (RadioButton)box_HeaderXPos.Controls["RB_XMiddle"];
            rb_HeaderXMiddle.Checked = (docdata==null)? false: (docdata.headerXPos==PageXPosition.Middle);
            RadioButton rb_HeaderXRight = (RadioButton)box_HeaderXPos.Controls["RB_XRight"];
            rb_HeaderXRight.Checked = (docdata==null)? false: (docdata.headerXPos==PageXPosition.Right);

            TextBox textbox_Footer = (TextBox)DokuParamsForm.Controls["DocFooter"];
            textbox_Footer.Text = (docdata==null)? "/unverändert": docdata.footer;
            GroupBox box_FooterXPos = (GroupBox)DokuParamsForm.Controls["box_FooterXPos"];
            RadioButton rb_FooterXLeft = (RadioButton)box_FooterXPos.Controls["RB_XLeft"];
            rb_FooterXLeft.Checked = (docdata==null)? false: (docdata.footerXPos==PageXPosition.Left);
            RadioButton rb_FooterXMiddle = (RadioButton)box_FooterXPos.Controls["RB_XMiddle"];
            rb_FooterXMiddle.Checked = (docdata==null)? false: (docdata.footerXPos==PageXPosition.Middle);
            RadioButton rb_FooterXRight = (RadioButton)box_FooterXPos.Controls["RB_XRight"];
            rb_FooterXRight.Checked = (docdata==null)? false: (docdata.footerXPos==PageXPosition.Right);

            GroupBox groupbox_vertical = (GroupBox)DokuParamsForm.Controls["box_VertPages"];
            RadioButton rb_NoneVertical = (RadioButton)groupbox_vertical.Controls["RB_NoneExceptArray"];
            rb_NoneVertical.Checked = (docdata==null)? false: (docdata.vertPagesQuantifier==PageQuantifiers.NoneExceptArray);
            RadioButton rb_AllVertical = (RadioButton)groupbox_vertical.Controls["RB_AllExceptArray"];
            rb_AllVertical.Checked = (docdata==null)? false: (docdata.vertPagesQuantifier==PageQuantifiers.AllExceptArray);
            TextBox textbox_verticalPages = (TextBox)groupbox_vertical.Controls["DocVertPageNumbers"];
            textbox_verticalPages.Text = (docdata==null)? "/unverändert": string.Join(", ", docdata.vertPagesNumbers);

            GroupBox groupbox_horizontal = (GroupBox)DokuParamsForm.Controls["box_HoriPages"];
            RadioButton rb_NoneHorizontal = (RadioButton)groupbox_horizontal.Controls["RB_NoneExceptArray"];
            rb_NoneHorizontal.Checked = (docdata==null)? false: (docdata.horiPagesQuantifier==PageQuantifiers.NoneExceptArray);
            RadioButton rb_AllHorizontal = (RadioButton)groupbox_horizontal.Controls["RB_AllExceptArray"];
            rb_AllHorizontal.Checked = (docdata==null)? false: (docdata.horiPagesQuantifier==PageQuantifiers.AllExceptArray);
            TextBox textbox_horizontalPages = (TextBox)groupbox_horizontal.Controls["DocHoriPageNumbers"];
            textbox_horizontalPages.Text = (docdata==null)? "/unverändert": string.Join(", ", docdata.horiPagesNumbers);

            TextBox textbox_Scalierung = (TextBox)DokuParamsForm.Controls["DocScaleFactor"];
            textbox_Scalierung.Text = (docdata==null)? "/unverändert": docdata.scaleFactor.ToString();

            Label colorName = (Label)DokuParamsForm.Controls["DocTextColor"];
            colorName.Text = (docdata==null)? "/unverändert": rgbDoubleColorToHexStringColor(docdata.textColor);
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
            System.Windows.Forms.TextBox folderTextBox = new System.Windows.Forms.TextBox();
            folderTextBox.Name = "DocFolderName";
            folderTextBox.Location = new System.Drawing.Point(x, y);
            folderTextBox.Size = new System.Drawing.Size(100, 20);
            DokuParamsForm.Controls.Add(folderTextBox);

            x += folderTextBox_Label.PreferredWidth + 100;
            System.Windows.Forms.TextBox fileTextBox = new System.Windows.Forms.TextBox();
            fileTextBox.Name = "DocApproxFileName";
            fileTextBox.Location = new System.Drawing.Point(x, y);
            fileTextBox.Size = new System.Drawing.Size(500, 20);
            DokuParamsForm.Controls.Add(fileTextBox);

            y += folderTextBox.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeHeaderTextBoxControl(int y, DocData docdata = null)
        {
            int x = 40;
            System.Windows.Forms.Label textbox_Header_Label = new System.Windows.Forms.Label();
            textbox_Header_Label.Location = new System.Drawing.Point(x, y);
            textbox_Header_Label.Text = "Kopfzeile";
            textbox_Header_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(textbox_Header_Label);

            y += textbox_Header_Label.Height + 10;
            System.Windows.Forms.TextBox textbox_Header = new System.Windows.Forms.TextBox();
            textbox_Header.Name = "DocHeader";
            textbox_Header.Multiline = true;
            textbox_Header.AcceptsReturn = true;
            textbox_Header.AcceptsTab = true;
            textbox_Header.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textbox_Header.Location = new System.Drawing.Point(x, y);
            textbox_Header.Size = new System.Drawing.Size(700, 80);
            DokuParamsForm.Controls.Add(textbox_Header);

            y += textbox_Header.Height + 10;
            System.Windows.Forms.GroupBox box_HeaderXPos = new System.Windows.Forms.GroupBox();
            box_HeaderXPos.Name = "box_HeaderXPos";
            box_HeaderXPos.Text = "Kopfzeile Position";
            box_HeaderXPos.Location = new System.Drawing.Point(x, y);
            box_HeaderXPos.Size = new System.Drawing.Size(700, 60);
            
            int y_boxlocal = 30;
            int x_boxlocal = 30;
            int rb_distance = 100;
            System.Windows.Forms.RadioButton rb_HeaderXLeft = new RadioButton();
            rb_HeaderXLeft.Name = "RB_XLeft";
            rb_HeaderXLeft.Text = "Links";
            rb_HeaderXLeft.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            box_HeaderXPos.Controls.Add(rb_HeaderXLeft);

            x_boxlocal += rb_HeaderXLeft.Size.Width + rb_distance;
            RadioButton rb_HeaderXMiddle = new RadioButton();
            rb_HeaderXMiddle.Name = "RB_XMiddle";
            rb_HeaderXMiddle.Text = "Mittig";
            rb_HeaderXMiddle.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            box_HeaderXPos.Controls.Add(rb_HeaderXMiddle);

            x_boxlocal += rb_HeaderXMiddle.Size.Width + rb_distance;
            RadioButton rb_HeaderXRight = new RadioButton();
            rb_HeaderXRight.Name = "RB_XRight";
            rb_HeaderXRight.Text = "Rechts";
            rb_HeaderXRight.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            box_HeaderXPos.Controls.Add(rb_HeaderXRight);

            DokuParamsForm.Controls.Add(box_HeaderXPos);

            y += box_HeaderXPos.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeFooterTextBoxControl(int y, DocData docdata = null)
        {
            int x = 40;
            System.Windows.Forms.Label textbox_Footer_Label = new System.Windows.Forms.Label();
            textbox_Footer_Label.Location = new System.Drawing.Point(x, y);
            textbox_Footer_Label.Text = "Fußzeile";
            textbox_Footer_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(textbox_Footer_Label);

            y += textbox_Footer_Label.Height + 10;
            System.Windows.Forms.TextBox textbox_Footer = new System.Windows.Forms.TextBox();
            textbox_Footer.Name = "DocFooter";
            textbox_Footer.Multiline = true;
            textbox_Footer.AcceptsReturn = true;
            textbox_Footer.AcceptsTab = true;
            textbox_Footer.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textbox_Footer.Location = new System.Drawing.Point(x, y);
            textbox_Footer.Size = new System.Drawing.Size(700, 80);
            DokuParamsForm.Controls.Add(textbox_Footer);

            y += textbox_Footer.Height + 10;
            System.Windows.Forms.GroupBox box_FooterXPos = new System.Windows.Forms.GroupBox();
            box_FooterXPos.Name = "box_FooterXPos";
            box_FooterXPos.Text = "Fußzeile Position";
            box_FooterXPos.Location = new System.Drawing.Point(x, y);
            box_FooterXPos.Size = new System.Drawing.Size(700, 60);
            
            int y_boxlocal = 30;
            int x_boxlocal = 30;
            int rb_distance = 100;
            RadioButton rb_FooterXLeft = new RadioButton();
            rb_FooterXLeft.Name = "RB_XLeft";
            rb_FooterXLeft.Text = "Links";
            rb_FooterXLeft.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            box_FooterXPos.Controls.Add(rb_FooterXLeft);

            x_boxlocal += rb_FooterXLeft.Size.Width + rb_distance;
            RadioButton rb_FooterXMiddle = new RadioButton();
            rb_FooterXMiddle.Name = "RB_XMiddle";
            rb_FooterXMiddle.Text = "Mittig";
            rb_FooterXMiddle.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            box_FooterXPos.Controls.Add(rb_FooterXMiddle);

            x_boxlocal += rb_FooterXMiddle.Size.Width + rb_distance;
            RadioButton rb_FooterXRight = new RadioButton();
            rb_FooterXRight.Name = "RB_XRight";
            rb_FooterXRight.Text = "Rechts";
            rb_FooterXRight.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            box_FooterXPos.Controls.Add(rb_FooterXRight);

            DokuParamsForm.Controls.Add(box_FooterXPos);

            y += box_FooterXPos.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeVerticalPagesControl(int y, DocData docdata = null)
        {
            int x = 40;
            System.Windows.Forms.GroupBox groupbox_vertical = new System.Windows.Forms.GroupBox();
            groupbox_vertical.Name = "box_VertPages";
            groupbox_vertical.Text = "Seiten vertikal machen:";
            groupbox_vertical.Location = new System.Drawing.Point(x, y);
            groupbox_vertical.Size = new System.Drawing.Size(700,110);

            int x_boxlocal = 30;
            int y_boxlocal = 30;
            int rb_distance = 150;
            System.Windows.Forms.RadioButton rb_NoneVertical = new System.Windows.Forms.RadioButton();
            rb_NoneVertical.Name = "RB_NoneExceptArray";
            rb_NoneVertical.Text = "Nur angegebene Seiten";
            rb_NoneVertical.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            rb_NoneVertical.Size = new System.Drawing.Size(200,20);
            groupbox_vertical.Controls.Add(rb_NoneVertical);

            x_boxlocal += rb_NoneVertical.Size.Width + rb_distance;
            System.Windows.Forms.RadioButton rb_AllVertical = new System.Windows.Forms.RadioButton();
            rb_AllVertical.Name = "RB_AllExceptArray";
            rb_AllVertical.Text = "Alle außer angegebene Seiten";
            rb_AllVertical.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            rb_AllVertical.Size = new System.Drawing.Size(250,20);
            groupbox_vertical.Controls.Add(rb_AllVertical);

            x_boxlocal = 10;
            y_boxlocal += rb_AllVertical.Size.Height + 10;
            System.Windows.Forms.Label verticalPages_Label = new System.Windows.Forms.Label();
            verticalPages_Label.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            verticalPages_Label.Text = "Seiten (z.B. 1, 3, 12, ...)";
            verticalPages_Label.AutoSize = true;
            groupbox_vertical.Controls.Add(verticalPages_Label);

            x_boxlocal += verticalPages_Label.PreferredWidth + 10;
            System.Windows.Forms.TextBox textbox_verticalPages = new System.Windows.Forms.TextBox();
            textbox_verticalPages.Name = "DocVertPageNumbers";
            textbox_verticalPages.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            textbox_verticalPages.Size = new System.Drawing.Size(400,20);
            groupbox_vertical.Controls.Add(textbox_verticalPages);

            DokuParamsForm.Controls.Add(groupbox_vertical);

            y += groupbox_vertical.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeHorizontalPagesControl(int y, DocData docdata = null)
        {
            int x = 40;
            System.Windows.Forms.GroupBox groupbox_horizontal = new System.Windows.Forms.GroupBox();
            groupbox_horizontal.Name = "box_HoriPages";
            groupbox_horizontal.Text = "Seiten horizontal machen:";
            groupbox_horizontal.Location = new System.Drawing.Point(x, y);
            groupbox_horizontal.Size = new System.Drawing.Size(700,110);

            int x_boxlocal = 30;
            int y_boxlocal = 30;
            int rb_distance = 150;
            System.Windows.Forms.RadioButton rb_NoneHorizontal = new System.Windows.Forms.RadioButton();
            rb_NoneHorizontal.Name = "RB_NoneExceptArray";
            rb_NoneHorizontal.Text = "Nur angegebene Seiten";
            rb_NoneHorizontal.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            rb_NoneHorizontal.Size = new System.Drawing.Size(200,20);
            rb_NoneHorizontal.Checked = false;
            groupbox_horizontal.Controls.Add(rb_NoneHorizontal);

            x_boxlocal += rb_NoneHorizontal.Size.Width + rb_distance;
            System.Windows.Forms.RadioButton rb_AllHorizontal = new System.Windows.Forms.RadioButton();
            rb_AllHorizontal.Name = "RB_AllExceptArray";
            rb_AllHorizontal.Text = "Alle außer angegebene Seiten";
            rb_AllHorizontal.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            rb_AllHorizontal.Size = new System.Drawing.Size(250,20);
            rb_AllHorizontal.Checked = false;
            groupbox_horizontal.Controls.Add(rb_AllHorizontal);

            x_boxlocal = 10;
            y_boxlocal += rb_AllHorizontal.Size.Height + 10;
            System.Windows.Forms.Label horizontalPages_Label = new System.Windows.Forms.Label();
            horizontalPages_Label.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            horizontalPages_Label.Text = "Seiten (z.B. 1, 3, 12, ...)";
            horizontalPages_Label.AutoSize = true;
            groupbox_horizontal.Controls.Add(horizontalPages_Label);

            x_boxlocal += horizontalPages_Label.PreferredWidth + 10;
            System.Windows.Forms.TextBox textbox_horizontalPages = new System.Windows.Forms.TextBox();
            textbox_horizontalPages.Name = "DocHoriPageNumbers";
            textbox_horizontalPages.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            textbox_horizontalPages.Size = new System.Drawing.Size(400,20);
            groupbox_horizontal.Controls.Add(textbox_horizontalPages);

            DokuParamsForm.Controls.Add(groupbox_horizontal);

            y += groupbox_horizontal.Size.Height;
            return y;
        }

        private int DokuParamsForm_makePageScalingAndTextColorControl(int y, DocData docdata = null)
        {
            int x = 40;
            System.Windows.Forms.Label scalierung_Label = new System.Windows.Forms.Label();
            scalierung_Label.Location = new System.Drawing.Point(x, y);
            scalierung_Label.Text = "Seitenskalierung Faktor:";
            scalierung_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(scalierung_Label);

            x += scalierung_Label.PreferredWidth + 5;
            System.Windows.Forms.TextBox textbox_Scalierung = new System.Windows.Forms.TextBox();
            textbox_Scalierung.Name = "DocScaleFactor";
            textbox_Scalierung.Location = new System.Drawing.Point(x, y);
            textbox_Scalierung.Size = new System.Drawing.Size(100,20);
            DokuParamsForm.Controls.Add(textbox_Scalierung);

            x += textbox_Scalierung.PreferredSize.Width + 100;
            Label colorLabel = new Label();
            colorLabel.Location = new System.Drawing.Point(x, y);
            colorLabel.Text = "Textfarbe:";
            colorLabel.AutoSize = true;
            DokuParamsForm.Controls.Add(colorLabel);

            x += colorLabel.PreferredWidth + 5;
            Label colorName = new Label();
            colorName.Name = "DocTextColor";
            colorName.Location = new System.Drawing.Point(x, y);
            colorName.AutoSize = true;
            DokuParamsForm.Controls.Add(colorName);

            x += colorName.PreferredWidth + 10;
            Button colorButton = new Button();
            colorButton.Location = new System.Drawing.Point(x, y);
            colorButton.Size = new System.Drawing.Size(100,50);
            colorButton.Text = "Farbe Wählen";
            colorButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
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