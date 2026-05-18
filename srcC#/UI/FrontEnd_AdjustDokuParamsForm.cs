using System.Windows.Forms;
using System.Drawing;
using System.Linq;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void makeDokuParamsForm()
        {
            DokuParamsForm = new System.Windows.Forms.Form();
            DokuParamsForm.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            form.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            DokuParamsForm.Size = new System.Drawing.Size(800, 950);
            DokuParamsForm.Font = new System.Drawing.Font("Helvetica", 12);
            DokuParamsForm.BackColor = System.Drawing.ColorTranslator.FromHtml("#E1F3F5");
            DokuParamsForm.AutoScroll = true;

            int y = 10;
            y = DokuParamsForm_makeFilePathControl(y);
            y = DokuParamsForm_makeHeaderTextBoxControl(y + 10);
            y = DokuParamsForm_makeFooterTextBoxControl(y + 10);
            y = DokuParamsForm_makeVerticalPagesControl(y + 10);
            y = DokuParamsForm_makeHorizontalPagesControl(y + 10);
            y = DokuParamsForm_makePageScalingAndTextColorControl(y + 10);
            y = DokuParamsForm_makeOKButtonControl(y + 10);
        }

        private int DokuParamsForm_makeFilePathControl(int y)
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
            folderTextBox.Name = "FolderName";
            folderTextBox.Location = new System.Drawing.Point(x, y);
            folderTextBox.Size = new System.Drawing.Size(100, 20);
            folderTextBox.Text = "/unverändert";
            DokuParamsForm.Controls.Add(folderTextBox);

            x += folderTextBox_Label.PreferredWidth + 100;
            System.Windows.Forms.TextBox fileTextBox = new System.Windows.Forms.TextBox();
            fileTextBox.Name = "FileName";
            fileTextBox.Location = new System.Drawing.Point(x, y);
            fileTextBox.Size = new System.Drawing.Size(500, 20);
            fileTextBox.Text = "/unverändert";
            DokuParamsForm.Controls.Add(fileTextBox);

            y += folderTextBox.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeHeaderTextBoxControl(int y)
        {
            int x = 40;
            System.Windows.Forms.Label textbox_Header_Label = new System.Windows.Forms.Label();
            textbox_Header_Label.Location = new System.Drawing.Point(x, y);
            textbox_Header_Label.Text = "Kopfzeile";
            textbox_Header_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(textbox_Header_Label);

            y += textbox_Header_Label.Height + 10;
            System.Windows.Forms.TextBox textbox_Header = new System.Windows.Forms.TextBox();
            textbox_Header.Name = "Header";
            textbox_Header.Multiline = true;
            textbox_Header.AcceptsReturn = true;
            textbox_Header.AcceptsTab = true;
            textbox_Header.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textbox_Header.Location = new System.Drawing.Point(x, y);
            textbox_Header.Size = new System.Drawing.Size(700, 80);
            textbox_Header.Text = "/unverändert";
            DokuParamsForm.Controls.Add(textbox_Header);

            y += textbox_Header.Height + 10;
            System.Windows.Forms.GroupBox box_HeaderXPos = new System.Windows.Forms.GroupBox();
            box_HeaderXPos.Name = "Box_HeaderXPos";
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
            rb_HeaderXLeft.Checked = true;
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

        private int DokuParamsForm_makeFooterTextBoxControl(int y)
        {
            int x = 40;
            System.Windows.Forms.Label textbox_Footer_Label = new System.Windows.Forms.Label();
            textbox_Footer_Label.Location = new System.Drawing.Point(x, y);
            textbox_Footer_Label.Text = "Fußzeile";
            textbox_Footer_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(textbox_Footer_Label);

            y += textbox_Footer_Label.Height + 10;
            System.Windows.Forms.TextBox textbox_Footer = new System.Windows.Forms.TextBox();
            textbox_Footer.Name = "Footer";
            textbox_Footer.Multiline = true;
            textbox_Footer.AcceptsReturn = true;
            textbox_Footer.AcceptsTab = true;
            textbox_Footer.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textbox_Footer.Location = new System.Drawing.Point(x, y);
            textbox_Footer.Size = new System.Drawing.Size(700, 80);
            textbox_Footer.Text = "/unverändert";
            DokuParamsForm.Controls.Add(textbox_Footer);

            y += textbox_Footer.Height + 10;
            System.Windows.Forms.GroupBox box_FooterXPos = new System.Windows.Forms.GroupBox();
            box_FooterXPos.Name = "Box_FooterXPos";
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
            rb_FooterXLeft.Checked = true;
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

        private int DokuParamsForm_makeVerticalPagesControl(int y)
        {
            int x = 40;
            System.Windows.Forms.GroupBox groupbox_vertical = new System.Windows.Forms.GroupBox();
            groupbox_vertical.Name = "Box_VerticalPages";
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
            rb_NoneVertical.Checked = true;
            groupbox_vertical.Controls.Add(rb_NoneVertical);

            x_boxlocal += rb_NoneVertical.Size.Width + rb_distance;
            System.Windows.Forms.RadioButton rb_AllVertical = new System.Windows.Forms.RadioButton();
            rb_AllVertical.Name = "RB_AllExceptArray";
            rb_AllVertical.Text = "Alle außer angegebene Seiten";
            rb_AllVertical.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            rb_AllVertical.Size = new System.Drawing.Size(250,20);
            rb_AllVertical.Checked = false;
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
            textbox_verticalPages.Name = "VerticalPages";
            textbox_verticalPages.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            textbox_verticalPages.Size = new System.Drawing.Size(400,20);
            groupbox_vertical.Controls.Add(textbox_verticalPages);

            DokuParamsForm.Controls.Add(groupbox_vertical);

            y += groupbox_vertical.Size.Height;
            return y;
        }

        private int DokuParamsForm_makeHorizontalPagesControl(int y)
        {
            int x = 40;
            System.Windows.Forms.GroupBox groupbox_horizontal = new System.Windows.Forms.GroupBox();
            groupbox_horizontal.Name = "Box_HorizontalPages";
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
            rb_NoneHorizontal.Checked = true;
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
            textbox_horizontalPages.Name = "HorizontalPages";
            textbox_horizontalPages.Location = new System.Drawing.Point(x_boxlocal, y_boxlocal);
            textbox_horizontalPages.Size = new System.Drawing.Size(400,20);
            groupbox_horizontal.Controls.Add(textbox_horizontalPages);

            DokuParamsForm.Controls.Add(groupbox_horizontal);

            y += groupbox_horizontal.Size.Height;
            return y;
        }

        private int DokuParamsForm_makePageScalingAndTextColorControl(int y)
        {
            int x = 40;
            System.Windows.Forms.Label scalierung_Label = new System.Windows.Forms.Label();
            scalierung_Label.Location = new System.Drawing.Point(x, y);
            scalierung_Label.Text = "Seitenskalierung Faktor:";
            scalierung_Label.AutoSize = true;
            DokuParamsForm.Controls.Add(scalierung_Label);

            x += scalierung_Label.PreferredWidth + 5;
            System.Windows.Forms.TextBox textbox_Scalierung = new System.Windows.Forms.TextBox();
            textbox_Scalierung.Name = "Scaling";
            textbox_Scalierung.Location = new System.Drawing.Point(x, y);
            textbox_Scalierung.Size = new System.Drawing.Size(100,20);
            textbox_Scalierung.Text = "/unverändert";
            DokuParamsForm.Controls.Add(textbox_Scalierung);

            x += textbox_Scalierung.PreferredSize.Width + 100;
            Label colorLabel = new Label();
            colorLabel.Location = new System.Drawing.Point(x, y);
            colorLabel.Text = "Textfarbe:";
            colorLabel.AutoSize = true;
            DokuParamsForm.Controls.Add(colorLabel);

            x += colorLabel.PreferredWidth + 5;
            Label colorName = new Label();
            colorName.Name = "ColorName";
            colorName.Location = new System.Drawing.Point(x, y);
            colorName.Text = "/unverändert";
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