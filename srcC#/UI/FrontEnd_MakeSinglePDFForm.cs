using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Windows.Forms;

namespace pdfParserByMH
{
    public partial class FrontEndProgram
    {
        private void makeSingelPDFForm()
        {
            SinglePDFForm = new System.Windows.Forms.TabPage("Einzelnes PDF Stempeln");
            tabControl.Controls.Add(SinglePDFForm);
            SinglePDFForm.Size = new System.Drawing.Size(800,950);
            SinglePDFForm.BackColor = System.Drawing.ColorTranslator.FromHtml("#E1F3F5");

            dictSinglePDFProperties = new System.Collections.Generic.Dictionary<DocPropertyType, DocProperty>();
            SinglePDFForm_makeFields();
            SingelPDFForm_fillFields();
        }

        private void SinglePDFForm_makeFields()
        {
            dictSinglePDFProperties.Clear();
            foreach(Control c in SinglePDFForm.Controls)
                c.Dispose();
            SinglePDFForm.Controls.Clear();

            int y = 10;
            y = SinglePDFForm_makeFilePathControl(y);
            y = SinglePDFForm_makeHeaderTextBoxControl(y + 10);
            y = SinglePDFForm_makeFooterTextBoxControl(y + 10);
            y = SinglePDFForm_makeVerticalPagesControl(y + 10);
            y = SinglePDFForm_makeHorizontalPagesControl(y + 10);
            y = SinglePDFForm_makePageScalingAndTextColorControl(y + 10);
            y = SinglePDFForm_makeOKButtonControl(y + 10);
        }

        private void SinglePDFForm_readFields()
        {
            overrideSinglePDF = ((CheckBox)SinglePDFForm.Controls["Override_File"]).Checked;
            foreach(DocProperty docprop in dictSinglePDFProperties.Values)
                docprop.read();
        }

        private void SingelPDFForm_fillFields()
        {
            dictSinglePDFProperties[DocPropertyType.HeaderXPos].value = PageXPosition.Left;
            dictSinglePDFProperties[DocPropertyType.FooterXPos].value = PageXPosition.Left;
            dictSinglePDFProperties[DocPropertyType.VertPageQuant].value = PageQuantifiers.NoneExceptArray;
            dictSinglePDFProperties[DocPropertyType.HoriPageQuant].value = PageQuantifiers.NoneExceptArray;
            dictSinglePDFProperties[DocPropertyType.VertPageNumbers].value = new int[] {};
            dictSinglePDFProperties[DocPropertyType.HoriPageNumbers].value = new int[] {};
            dictSinglePDFProperties[DocPropertyType.ScaleFactor].value = 1.0;
            dictSinglePDFProperties[DocPropertyType.TextColor].value = new double[] {0,0,0};

            foreach(DocProperty docprop in dictSinglePDFProperties.Values)
                docprop.write();
        }

        private int SinglePDFForm_makeFilePathControl(int y)
        {
            int x = 40;
            System.Windows.Forms.Label textbox_FilePath_Label = new Label();
            SinglePDFForm.Controls.Add(textbox_FilePath_Label);
            textbox_FilePath_Label.Location = new System.Drawing.Point(x, y);
            textbox_FilePath_Label.Text = "Pfad der PDF-Datei";
            textbox_FilePath_Label.AutoSize = true;
            x += textbox_FilePath_Label.PreferredWidth + 300;

            Label overrideFile_Label = new Label();
            SinglePDFForm.Controls.Add(overrideFile_Label);
            overrideFile_Label.Location = new Point(x, y);
            overrideFile_Label.Text = "Datei überschreiben:";
            overrideFile_Label.AutoSize = true;
            x += overrideFile_Label.PreferredWidth + 10;

            CheckBox overrideFile_Checkbox = new CheckBox();
            SinglePDFForm.Controls.Add(overrideFile_Checkbox);
            overrideFile_Checkbox.Name = "Override_File";
            overrideFile_Checkbox.Location = new Point(x, y);
            overrideFile_Checkbox.Checked = false;
            x = 40;
            y += textbox_FilePath_Label.Height + 10;

            System.Windows.Forms.TextBox textbox_FilePath = new TextBox();
            SinglePDFForm.Controls.Add(textbox_FilePath);
            dictSinglePDFProperties[DocPropertyType.FilePath] = new DocFilePath(textbox_FilePath);
            textbox_FilePath.Name = "SinglePDF_FilePath";
            textbox_FilePath.Location = new System.Drawing.Point(x, y);
            textbox_FilePath.Size = new System.Drawing.Size(600, 20);
            x += textbox_FilePath.Size.Width + 10;

            System.Windows.Forms.Button browseButton = new Button();
            SinglePDFForm.Controls.Add(browseButton);
            browseButton.Location = new System.Drawing.Point(x, y);
            browseButton.Size = new System.Drawing.Size(100, 50);
            browseButton.Text = "Browse";
            browseButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            browseButton.Click += (sender, e) =>
            {
                System.Windows.Forms.OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "PDF files (*.pdf)|*.pdf";
                if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    textbox_FilePath.Text = dialog.FileName;
                }
            };
            
            y += Math.Max(textbox_FilePath.Height, browseButton.Size.Height);
            return y;
        }

        private int SinglePDFForm_makeHeaderTextBoxControl(int y)
        {
            int x = 40;
            Label textbox_Header_Label = new Label();
            SinglePDFForm.Controls.Add(textbox_Header_Label);
            textbox_Header_Label.Location = new Point(x, y);
            textbox_Header_Label.Text = "Kopfzeile";
            textbox_Header_Label.AutoSize = true;

            y += textbox_Header_Label.Height + 10;
            TextBox textbox_Header = new TextBox();
            SinglePDFForm.Controls.Add(textbox_Header);
            dictSinglePDFProperties[DocPropertyType.Header] = new DocHeader(textbox_Header);
            textbox_Header.Multiline = true;
            textbox_Header.AcceptsReturn = true;
            textbox_Header.AcceptsTab = true;
            textbox_Header.ScrollBars = ScrollBars.Vertical;
            textbox_Header.Location = new Point(x, y);
            textbox_Header.Size = new Size(700, 80);

            y += textbox_Header.Height + 10;
            GroupBox box_HeaderXPos = new System.Windows.Forms.GroupBox();
            SinglePDFForm.Controls.Add(box_HeaderXPos);
            box_HeaderXPos.Text = "Kopfzeile Position";
            box_HeaderXPos.Location = new System.Drawing.Point(x, y);
            box_HeaderXPos.Size = new System.Drawing.Size(700, 60);
            
            RadioButton rb_HeaderXLeft = new RadioButton();
            box_HeaderXPos.Controls.Add(rb_HeaderXLeft);
            RadioButton rb_HeaderXMiddle = new RadioButton();
            box_HeaderXPos.Controls.Add(rb_HeaderXMiddle);
            RadioButton rb_HeaderXRight = new RadioButton();
            box_HeaderXPos.Controls.Add(rb_HeaderXRight);
            dictSinglePDFProperties[DocPropertyType.HeaderXPos] = new DocHeaderXPos(new RadioButton[] {rb_HeaderXLeft, rb_HeaderXMiddle, rb_HeaderXRight});

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

        private int SinglePDFForm_makeFooterTextBoxControl(int y)
        {
            int x = 40;
            Label textbox_Footer_Label = new Label();
            SinglePDFForm.Controls.Add(textbox_Footer_Label);
            textbox_Footer_Label.Location = new Point(x, y);
            textbox_Footer_Label.Text = "Fußzeile";
            textbox_Footer_Label.AutoSize = true;
            

            y += textbox_Footer_Label.Height + 10;
            TextBox textbox_Footer = new TextBox();
            SinglePDFForm.Controls.Add(textbox_Footer);
            dictSinglePDFProperties[DocPropertyType.Footer] = new DocFooter(textbox_Footer);
            textbox_Footer.Multiline = true;
            textbox_Footer.AcceptsReturn = true;
            textbox_Footer.AcceptsTab = true;
            textbox_Footer.ScrollBars = ScrollBars.Vertical;
            textbox_Footer.Location = new Point(x, y);
            textbox_Footer.Size = new Size(700, 80);
            

            y += textbox_Footer.Height + 10;
            GroupBox box_FooterXPos = new GroupBox();
            SinglePDFForm.Controls.Add(box_FooterXPos);
            box_FooterXPos.Text = "Fußzeile Position";
            box_FooterXPos.Location = new Point(x, y);
            box_FooterXPos.Size = new Size(700, 60);

            RadioButton rb_FooterXLeft = new RadioButton();
            box_FooterXPos.Controls.Add(rb_FooterXLeft);
            RadioButton rb_FooterXMiddle = new RadioButton();
            box_FooterXPos.Controls.Add(rb_FooterXMiddle);
            RadioButton rb_FooterXRight = new RadioButton();
            box_FooterXPos.Controls.Add(rb_FooterXRight);
            dictSinglePDFProperties[DocPropertyType.FooterXPos] = new DocFooterXPos(new RadioButton[] {rb_FooterXLeft, rb_FooterXMiddle, rb_FooterXRight});
            
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

        private int SinglePDFForm_makeVerticalPagesControl(int y)
        {
            int x = 40;
            GroupBox groupbox_vertical = new GroupBox();
            SinglePDFForm.Controls.Add(groupbox_vertical);
            groupbox_vertical.Text = "Seiten vertikal machen:";
            groupbox_vertical.Location = new Point(x, y);
            groupbox_vertical.Size = new Size(700,110);
            groupbox_vertical.Font = new Font("Arial", 12);

            RadioButton rb_NoneVertical = new RadioButton();
            groupbox_vertical.Controls.Add(rb_NoneVertical);
            RadioButton rb_AllVertical = new RadioButton();
            groupbox_vertical.Controls.Add(rb_AllVertical);
            dictSinglePDFProperties[DocPropertyType.VertPageQuant] = new DocVertPageQuantifier(new RadioButton[] {rb_NoneVertical, rb_AllVertical});

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
            dictSinglePDFProperties[DocPropertyType.VertPageNumbers] = new DocVertPageNumbers(textbox_verticalPages);
            textbox_verticalPages.Location = new Point(x_boxlocal, y_boxlocal);
            textbox_verticalPages.Size = new Size(400,20);

            y += groupbox_vertical.Size.Height;
            return y;
        }

        private int SinglePDFForm_makeHorizontalPagesControl(int y)
        {
            int x = 40;
            GroupBox groupbox_horizontal = new GroupBox();
            SinglePDFForm.Controls.Add(groupbox_horizontal);
            groupbox_horizontal.Text = "Seiten horizontal machen:";
            groupbox_horizontal.Location = new Point(x, y);
            groupbox_horizontal.Size = new Size(700,110);

            RadioButton rb_NoneHorizontal = new RadioButton();
            groupbox_horizontal.Controls.Add(rb_NoneHorizontal);
            RadioButton rb_AllHorizontal = new RadioButton();
            groupbox_horizontal.Controls.Add(rb_AllHorizontal);
            dictSinglePDFProperties[DocPropertyType.HoriPageQuant] = new DocHoriPageQuantifier(new RadioButton[] {rb_NoneHorizontal, rb_AllHorizontal});

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
            dictSinglePDFProperties[DocPropertyType.HoriPageNumbers] = new DocHoriPageNumbers(textbox_horizontalPages);
            textbox_horizontalPages.Location = new Point(x_boxlocal, y_boxlocal);
            textbox_horizontalPages.Size = new Size(400,20);

            y += groupbox_horizontal.Size.Height;
            return y;
        }

        private int SinglePDFForm_makePageScalingAndTextColorControl(int y)
        {
            int x = 40;
            Label scalierung_Label = new Label();
            SinglePDFForm.Controls.Add(scalierung_Label);
            scalierung_Label.Location = new Point(x, y);
            scalierung_Label.Text = "Seitenskalierung Faktor:";
            scalierung_Label.AutoSize = true;
            x += scalierung_Label.Width + 5;

            TextBox textbox_Scalierung = new TextBox();
            SinglePDFForm.Controls.Add(textbox_Scalierung);
            dictSinglePDFProperties[DocPropertyType.ScaleFactor] = new DocScaleFactor(textbox_Scalierung);
            textbox_Scalierung.Location = new Point(x, y);
            textbox_Scalierung.Size = new Size(100,20);
            x += textbox_Scalierung.Width + 100;
            
            Label colorLabel = new Label();
            SinglePDFForm.Controls.Add(colorLabel);
            colorLabel.Location = new Point(x, y);
            colorLabel.Text = "Textfarbe:";
            colorLabel.AutoSize = true;
            x += colorLabel.Width + 5;

            Label colorName = new Label();
            SinglePDFForm.Controls.Add(colorName);
            dictSinglePDFProperties[DocPropertyType.TextColor] = new DocTextColor(colorName);
            colorName.Location = new Point(x, y);
            colorName.AutoSize = true;
            colorName.PerformLayout();
            colorName.Text = "#000000";
            x += colorName.Width + 5;

            Button colorButton = new Button();
            SinglePDFForm.Controls.Add(colorButton);
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

        private int SinglePDFForm_makeOKButtonControl(int y)
        {
            int x = 350;
            Button okButton = new Button();
            SinglePDFForm.Controls.Add(okButton);
            okButton.Location = new System.Drawing.Point(x, y);
            okButton.Size = new System.Drawing.Size(100,50);
            okButton.Text = "Stempeln";
            okButton.BackColor = System.Drawing.ColorTranslator.FromHtml("#C2EAF0");
            okButton.Click += (sender, e) =>
            {
                form.DialogResult = DialogResult.OK;
                progState = ProgramState.DoSingelPDF;
                form.Close();
            };
            y += okButton.Size.Height;
            return y;
        }
    }
}