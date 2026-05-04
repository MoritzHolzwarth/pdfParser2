using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void makeSingelPDFForm()
        {
            SinglePDFForm = new System.Windows.Forms.TabPage("Einzelnes PDF Stempeln");
            SinglePDFForm.Size = new System.Drawing.Size(800,900);
            SinglePDFForm.Font = new System.Drawing.Font("Helvetica", 12);
            SinglePDFForm.BackColor = System.Drawing.ColorTranslator.FromHtml("#E1F3F5");

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            int y = 10;
            System.Windows.Forms.Label textbox_FilePath_Label = new System.Windows.Forms.Label();
            textbox_FilePath_Label.Location = new System.Drawing.Point(40,y);
            textbox_FilePath_Label.Text = "Pfad der PDF-Datei";
            textbox_FilePath_Label.AutoSize = true;
            SinglePDFForm.Controls.Add(textbox_FilePath_Label);

            y += 30;
            System.Windows.Forms.TextBox textbox_FilePath = new System.Windows.Forms.TextBox();
            textbox_FilePath.Name = "SinglePDF_FilePath";
            textbox_FilePath.Location = new System.Drawing.Point(40,y);
            textbox_FilePath.Size = new System.Drawing.Size(600,20);
            SinglePDFForm.Controls.Add(textbox_FilePath);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            y += 40;
            System.Windows.Forms.Label textbox_Header_Label = new System.Windows.Forms.Label();
            textbox_Header_Label.Location = new System.Drawing.Point(40,y);
            textbox_Header_Label.Text = "Kopfzeile";
            textbox_Header_Label.AutoSize = true;
            SinglePDFForm.Controls.Add(textbox_Header_Label);

            y += 30;
            System.Windows.Forms.TextBox textbox_Header = new System.Windows.Forms.TextBox();
            textbox_Header.Name = "SinglePDF_Header";
            textbox_Header.Multiline = true;
            textbox_Header.AcceptsReturn = true;
            textbox_Header.AcceptsTab = true;
            textbox_Header.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textbox_Header.Location = new System.Drawing.Point(40,y);
            textbox_Header.Size = new System.Drawing.Size(700,80);
            SinglePDFForm.Controls.Add(textbox_Header);

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            y += 90;
            System.Windows.Forms.GroupBox box_HeaderXPos = new System.Windows.Forms.GroupBox();
            box_HeaderXPos.Name = "Box_HeaderXPos";
            box_HeaderXPos.Text = "Kopfzeile Position";
            box_HeaderXPos.Location = new System.Drawing.Point(40,y);
            box_HeaderXPos.Size = new System.Drawing.Size(700, 60);
            
            RadioButton rb_HeaderXLeft = new RadioButton();
            rb_HeaderXLeft.Name = "RB_XLeft";
            rb_HeaderXLeft.Text = "Links";
            rb_HeaderXLeft.Location = new Point(10,30);
            rb_HeaderXLeft.Checked = true;
            box_HeaderXPos.Controls.Add(rb_HeaderXLeft);
            RadioButton rb_HeaderXMiddle = new RadioButton();
            rb_HeaderXMiddle.Name = "RB_XMiddle";
            rb_HeaderXMiddle.Text = "Mittig";
            rb_HeaderXMiddle.Location = new Point(200,30);
            box_HeaderXPos.Controls.Add(rb_HeaderXMiddle);
            RadioButton rb_HeaderXRight = new RadioButton();
            rb_HeaderXRight.Name = "RB_XRight";
            rb_HeaderXRight.Text = "Rechts";
            rb_HeaderXRight.Location = new Point(400,30);
            box_HeaderXPos.Controls.Add(rb_HeaderXRight);

            SinglePDFForm.Controls.Add(box_HeaderXPos);

///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            
            y += 80;
            System.Windows.Forms.Label textbox_Footer_Label = new System.Windows.Forms.Label();
            textbox_Footer_Label.Location = new System.Drawing.Point(40,y);
            textbox_Footer_Label.Text = "Fußzeile";
            textbox_Footer_Label.AutoSize = true;
            SinglePDFForm.Controls.Add(textbox_Footer_Label);

            y += 30;
            System.Windows.Forms.TextBox textbox_Footer = new System.Windows.Forms.TextBox();
            textbox_Footer.Name = "SinglePDF_Footer";
            textbox_Footer.Multiline = true;
            textbox_Footer.AcceptsReturn = true;
            textbox_Footer.AcceptsTab = true;
            textbox_Footer.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            textbox_Footer.Location = new System.Drawing.Point(40,y);
            textbox_Footer.Size = new System.Drawing.Size(700,80);
            SinglePDFForm.Controls.Add(textbox_Footer);

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            y += 90;
            System.Windows.Forms.GroupBox box_FooterXPos = new System.Windows.Forms.GroupBox();
            box_FooterXPos.Name = "Box_FooterXPos";
            box_FooterXPos.Text = "Fußzeile Position";
            box_FooterXPos.Location = new System.Drawing.Point(40,y);
            box_FooterXPos.Size = new System.Drawing.Size(700, 60);
            
            RadioButton rb_FooterXLeft = new RadioButton();
            rb_FooterXLeft.Name = "RB_XLeft";
            rb_FooterXLeft.Text = "Links";
            rb_FooterXLeft.Location = new Point(10,30);
            rb_FooterXLeft.Checked = true;
            box_FooterXPos.Controls.Add(rb_FooterXLeft);
            RadioButton rb_FooterXMiddle = new RadioButton();
            rb_FooterXMiddle.Name = "RB_XMiddle";
            rb_FooterXMiddle.Text = "Mittig";
            rb_FooterXMiddle.Location = new Point(200,30);
            box_FooterXPos.Controls.Add(rb_FooterXMiddle);
            RadioButton rb_FooterXRight = new RadioButton();
            rb_FooterXRight.Name = "RB_XRight";
            rb_FooterXRight.Text = "Rechts";
            rb_FooterXRight.Location = new Point(400,30);
            box_FooterXPos.Controls.Add(rb_FooterXRight);

            SinglePDFForm.Controls.Add(box_FooterXPos);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            y += 80;
            System.Windows.Forms.GroupBox groupbox_vertical = new System.Windows.Forms.GroupBox();
            groupbox_vertical.Name = "Box_VerticalPages";
            groupbox_vertical.Text = "Seiten vertikal machen:";
            groupbox_vertical.Location = new System.Drawing.Point(40,y);
            groupbox_vertical.Size = new System.Drawing.Size(700,110);

            System.Windows.Forms.RadioButton rb_NoneVertical = new System.Windows.Forms.RadioButton();
            rb_NoneVertical.Name = "RB_NoneExceptArray";
            rb_NoneVertical.Text = "Nur angegebene Seiten";
            rb_NoneVertical.Location = new System.Drawing.Point(30, 30);
            rb_NoneVertical.Size = new System.Drawing.Size(200,20);
            rb_NoneVertical.Checked = true;
            groupbox_vertical.Controls.Add(rb_NoneVertical);

            System.Windows.Forms.RadioButton rb_AllVertical = new System.Windows.Forms.RadioButton();
            rb_AllVertical.Name = "RB_AllExceptArray";
            rb_AllVertical.Text = "Alle außer angegebene Seiten";
            rb_AllVertical.Location = new System.Drawing.Point(330, 30);
            rb_AllVertical.Size = new System.Drawing.Size(250,20);
            rb_AllVertical.Checked = false;
            groupbox_vertical.Controls.Add(rb_AllVertical);

            System.Windows.Forms.Label verticalPages_Label = new System.Windows.Forms.Label();
            verticalPages_Label.Location = new System.Drawing.Point(10,70);
            verticalPages_Label.Text = "Seiten (z.B. 1, 3, 12, ...)";
            verticalPages_Label.AutoSize = true;
            groupbox_vertical.Controls.Add(verticalPages_Label);

            System.Windows.Forms.TextBox textbox_verticalPages = new System.Windows.Forms.TextBox();
            textbox_verticalPages.Name = "SinglePDF_VerticalPages";
            textbox_verticalPages.Location = new System.Drawing.Point(200,70);
            textbox_verticalPages.Size = new System.Drawing.Size(400,20);
            groupbox_vertical.Controls.Add(textbox_verticalPages);

            SinglePDFForm.Controls.Add(groupbox_vertical);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            y += 130;
            System.Windows.Forms.GroupBox groupbox_horizontal = new System.Windows.Forms.GroupBox();
            groupbox_horizontal.Name = "Box_HorizontalPages";
            groupbox_horizontal.Text = "Seiten horizontal machen:";
            groupbox_horizontal.Location = new System.Drawing.Point(40,y);
            groupbox_horizontal.Size = new System.Drawing.Size(700,110);

            System.Windows.Forms.RadioButton rb_NoneHorizontal = new System.Windows.Forms.RadioButton();
            rb_NoneHorizontal.Name = "RB_NoneExceptArray";
            rb_NoneHorizontal.Text = "Nur angegebene Seiten";
            rb_NoneHorizontal.Location = new System.Drawing.Point(30, 30);
            rb_NoneHorizontal.Size = new System.Drawing.Size(200,20);
            rb_NoneHorizontal.Checked = true;
            groupbox_horizontal.Controls.Add(rb_NoneHorizontal);

            System.Windows.Forms.RadioButton rb_AllHorizontal = new System.Windows.Forms.RadioButton();
            rb_AllHorizontal.Name = "RB_AllExceptArray";
            rb_AllHorizontal.Text = "Alle außer angegebene Seiten";
            rb_AllHorizontal.Location = new System.Drawing.Point(330, 30);
            rb_AllHorizontal.Size = new System.Drawing.Size(250,20);
            rb_AllHorizontal.Checked = false;
            groupbox_horizontal.Controls.Add(rb_AllHorizontal);

            System.Windows.Forms.Label horizontalPages_Label = new System.Windows.Forms.Label();
            horizontalPages_Label.Location = new System.Drawing.Point(10,70);
            horizontalPages_Label.Text = "Seiten (z.B. 1, 3, 12, ...)";
            horizontalPages_Label.AutoSize = true;
            groupbox_horizontal.Controls.Add(horizontalPages_Label);

            System.Windows.Forms.TextBox textbox_horizontalPages = new System.Windows.Forms.TextBox();
            textbox_horizontalPages.Name = "HorizontalPages";
            textbox_horizontalPages.Location = new System.Drawing.Point(200,70);
            textbox_horizontalPages.Size = new System.Drawing.Size(400,20);
            groupbox_horizontal.Controls.Add(textbox_horizontalPages);

            SinglePDFForm.Controls.Add(groupbox_horizontal);

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            y += 120;
            System.Windows.Forms.Label scalierung_Label = new System.Windows.Forms.Label();
            scalierung_Label.Location = new System.Drawing.Point(40,y);
            scalierung_Label.Text = "Seitenskalierung Faktor:";
            scalierung_Label.AutoSize = true;
            SinglePDFForm.Controls.Add(scalierung_Label);

            System.Windows.Forms.TextBox textbox_Scalierung = new System.Windows.Forms.TextBox();
            textbox_Scalierung.Name = "SinglePDF_Scaling";
            textbox_Scalierung.Location = new System.Drawing.Point(240,y);
            textbox_Scalierung.Size = new System.Drawing.Size(100,20);
            textbox_Scalierung.Text = "1.0";
            SinglePDFForm.Controls.Add(textbox_Scalierung);

            y += 40;
            Label colorName = new Label();
            colorName.Name = "ColorName";
            colorName.Location = new Point(150,y);
            colorName.Text = "#000000";
            SinglePDFForm.Controls.Add(colorName);

            SinglePDFForm_initButtons(y);
        }

        private void SinglePDFForm_initButtons(int y)
        {
            System.Windows.Forms.Button browseButton = new System.Windows.Forms.Button();
            browseButton.Location = new System.Drawing.Point(650,15);
            browseButton.Size = new System.Drawing.Size(100,50);
            browseButton.Text = "Browse";
            browseButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            browseButton.Click += (sender, e) =>
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "PDF files (*.pdf)|*.pdf";
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    SinglePDFForm.Controls["SinglePDF_FilePath"].Text = dialog.FileName;
                }
            };
            SinglePDFForm.Controls.Add(browseButton);

            Button colorButton = new Button();
            colorButton.Location = new Point(40, y-10);
            colorButton.Size = new Size(100,50);
            colorButton.Text = "Farbe Wählen";
            colorButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#E1F3F5");
            colorButton.Click += (sender, e) =>
            {
                ColorDialog dialog = new ColorDialog();
                dialog.AllowFullOpen = true;
                dialog.FullOpen = true;
                if(dialog.ShowDialog() == DialogResult.OK)
                {
                    string colorHex = string.Format("#{0:X2}{1:X2}{2:X2}", dialog.Color.R, dialog.Color.G, dialog.Color.B);
                    Label labelColor = (Label)SinglePDFForm.Controls["ColorName"];
                    labelColor.Text = colorHex;
                    labelColor.ForeColor = dialog.Color;
                }
            };
            SinglePDFForm.Controls.Add(colorButton);

            System.Windows.Forms.Button okButton = new System.Windows.Forms.Button();
            okButton.Location = new System.Drawing.Point(500,740);
            okButton.Size = new System.Drawing.Size(100,50);
            okButton.Text = "OK";
            okButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            okButton.Click += (sender, e) =>
            {
                stempelOption = StempelOptions.DoSingelPDF;
                form.DialogResult = System.Windows.Forms.DialogResult.OK;
                form.Close();
            };
            SinglePDFForm.Controls.Add(okButton);
        }
    }
}