using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace pdfParserByMH
{
    public enum DocPropertyType {Null, FilePath, FolderName, ApproxFileName, Header, Footer, HeaderXPos, FooterXPos, VertPageQuant, HoriPageQuant, VertPageNumbers, HoriPageNumbers, ScaleFactor, TextColor}
    public class DocProperty
    {
        public static string unv = "/unverändert";
        public virtual DocPropertyType type {get {return DocPropertyType.Null;}}
        public object value {get; set;}
        public bool hasChanged {get; set;}
        public DocProperty()
        {
            hasChanged = false;
        }
        public virtual void read()
        {}
        public virtual bool write()
        {
            return false;
        }
        public virtual void clear()
        {}
    }

    public class DocStringProperty : DocProperty
    {
        public static DocPropertyType[] allSubTypes = {DocPropertyType.FolderName, DocPropertyType.ApproxFileName, DocPropertyType.Header,
                                                                DocPropertyType.Footer};
        public override DocPropertyType type {get {return DocPropertyType.Null;}}
        public TextBox textbox {get; set;}
        public DocStringProperty(TextBox tb = null)
        {
            textbox = tb;
        }
        public override void read()
        {
            if(textbox.Text == unv)
                hasChanged = false;
            else
            {
                value = textbox.Text;
                hasChanged = true;
            }
        }
        public override bool write()
        {
            string txt = (string)value;
            textbox.Text = txt;
            return true;
        }
        public override void clear()
        {
            value = null;
            textbox.Text = unv;
        }
    }

    public class DocPageXPosProperty : DocProperty
    {
        public static DocPropertyType[] allSubTypes = {DocPropertyType.HeaderXPos, DocPropertyType.FooterXPos};
        public override DocPropertyType type {get {return DocPropertyType.Null;}}
        public PageXPosition[] arrValues = {PageXPosition.Left, PageXPosition.Middle, PageXPosition.Right};
        public RadioButton[] arrRBs {get; set;}
        public DocPageXPosProperty(RadioButton[] arr = null)
        {
            if(arr.Length != 3)
                throw new System.Exception("Error in DocPageXPosProperty(): Parameter 'arr' must have Length 3, since there are 3 defined XPos values!");
            arrRBs = arr;
        }
        public override void read()
        {
            for(int i=0; i<3; i++)
            {
                if(arrRBs[i].Checked)
                {
                    if(arrRBs[(i+1)%3].Checked || arrRBs[(i+2)%3].Checked)
                        throw new System.Exception("Error in DocPageXPosProperty.read(): More than 1 RadioButton checked!");
                    value = arrValues[i];
                    hasChanged = true;
                    return;
                }
            }
            hasChanged = false; //if no RBs are checked
        }
        public override bool write()
        {
            PageXPosition pos = (PageXPosition)value;
            for(int i=0; i<3; i++)
            {
                if(arrValues[i] == pos)
                {
                    arrRBs[i].Checked = true;
                    return true;
                }
            }
            return false;
        }
        public override void clear()
        {
            value = PageXPosition.Null;
            foreach(RadioButton rb in arrRBs)
                rb.Checked = false;
        }
    }

    public class DocPageQuantifierProperty : DocProperty
    {
        public static DocPropertyType[] allSubTypes = {DocPropertyType.VertPageQuant, DocPropertyType.HoriPageQuant};
        public override DocPropertyType type {get {return DocPropertyType.Null;}}
        public PageQuantifiers[] arrValues {get; set;}
        public RadioButton[] arrRBs {get; set;}
        public DocPageQuantifierProperty(RadioButton[] arr = null)
        {
            if(arr.Length != 2)
                throw new System.Exception("Error in DocPageQuantifierProperty(): Parameter 'arr' must have Length 2, since there are 2 defined PageQuantifier values!");
            arrRBs = arr;
        }
        public override void read()
        {
            if(arrRBs[0].Checked && arrRBs[1].Checked)
                throw new System.Exception("Error in DocPageQuantifierProperty(): More than 1 RadioButton checked!");
            else if(arrRBs[0].Checked)
            {
                value = PageQuantifiers.NoneExceptArray;
                hasChanged = true;
            }
            else if(arrRBs[1].Checked)
            {
                value = PageQuantifiers.AllExceptArray;
                hasChanged = true;
            }
            else
                hasChanged = false; //if no RBs are checked
        }
        public override bool write()
        {
            PageQuantifiers quant = (PageQuantifiers)value;
            switch (quant)
            {
                case PageQuantifiers.NoneExceptArray:
                    arrRBs[0].Checked = true;
                    return true;
                case PageQuantifiers.AllExceptArray:
                    arrRBs[1].Checked = true;
                    return true;
                default:
                    return false;
            }
        }
        public override void clear()
        {
            value = PageQuantifiers.Null;
            foreach(RadioButton rb in arrRBs)
                rb.Checked = false;
        }
    }

    public class DocIntArrayProperty : DocProperty
    {
        public static DocPropertyType[] allSubTypes = {DocPropertyType.VertPageNumbers, DocPropertyType.HoriPageNumbers};
        public override DocPropertyType type {get {return DocPropertyType.Null;}}
        public TextBox textbox {get; set;}
        public DocIntArrayProperty(TextBox tb = null)
        {
            textbox = tb;
        }
        public override void read()
        {
            string txt = textbox.Text;
            if(txt == unv)
            {
                hasChanged = false;
                return;
            }
            txt = txt.Replace(" ", "");
            string[] arrTxt = txt.Split(new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);
            int[] numbers = new int[arrTxt.Length];
            for(int i=0; i<arrTxt.Length; i++)
            {
                int num;
                if(!int.TryParse(arrTxt[i], out num))
                    throw new System.Exception("Error in DocIntArrayProperty.read(): non-integer value :" + arrTxt[i]);
                numbers[i] = num;
            }
            value = numbers;
            hasChanged = true;
        }
        public override bool write()
        {
            int[] arr = (int[])value;
            string txt = string.Join(", ", arr);
            textbox.Text = txt;
            return true;
        }
        public override void clear()
        {
            value = null;
            textbox.Text = unv;
        }
    }

    public class DocFilePath : DocStringProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.FilePath;}}
        public DocFilePath(TextBox tb = null) : base(tb)
        {}
    }


    public class DocFolderName : DocStringProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.FolderName;}}
        public DocFolderName(TextBox tb = null) : base(tb)
        {}
    }

    public class DocFileApproxName : DocStringProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.ApproxFileName;}}
        public DocFileApproxName(TextBox tb = null) : base(tb)
        {}
    }

    public class DocHeader : DocStringProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.Header;}}
        public DocHeader(TextBox tb = null) : base(tb)
        {}
    }

    public class DocFooter : DocStringProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.Footer;}}
        public DocFooter(TextBox tb = null) : base(tb)
        {}
    }

    public class DocHeaderXPos : DocPageXPosProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.HeaderXPos;}}
        public DocHeaderXPos(RadioButton[] arr) : base(arr)
        {}
    }

    public class DocFooterXPos : DocPageXPosProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.FooterXPos;}}

        public DocFooterXPos(RadioButton[] arr) : base(arr)
        {}
    }

    public class DocVertPageQuantifier : DocPageQuantifierProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.VertPageQuant;}}
        public DocVertPageQuantifier(RadioButton[] arr = null) : base(arr)
        {}
    }

    public class DocHoriPageQuantifier : DocPageQuantifierProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.HoriPageQuant;}}
        public DocHoriPageQuantifier(RadioButton[] arr = null) : base(arr)
        {}
    }

    public class DocVertPageNumbers : DocIntArrayProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.VertPageNumbers;}}
        public DocVertPageNumbers(TextBox tb = null) : base(tb)
        {}
    }

    public class DocHoriPageNumbers : DocIntArrayProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.HoriPageNumbers;}}
        public DocHoriPageNumbers(TextBox tb = null) : base(tb)
        {}
    }

    public class DocScaleFactor : DocProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.ScaleFactor;}}
        public TextBox textbox {get; set;}
        public DocScaleFactor(TextBox tb = null)
        {
            textbox = tb;
        }
        public override void read()
        {
            string txt = textbox.Text;
            if(txt == unv)
            {
                hasChanged = false;
                return;
            }
            double val;
            if(!double.TryParse(txt, out val))
                throw new System.Exception("Error in DocScaleFactor.read(): non-numeric value :" + txt);
            value = val;
            hasChanged = true;
        }
        public override bool write()
        {
            textbox.Text = ((double)value).ToString();
            return true;
        }
        public override void clear()
        {
            value = -1;
            textbox.Text = unv;
        }
    }

    public class DocTextColor : DocProperty
    {
        public override DocPropertyType type {get {return DocPropertyType.TextColor;}}
        public Label label {get; set;}

        public DocTextColor(Label lab =null)
        {
            label = lab;
        }
        public override void read()
        {
            string txt = label.Text;
            if(txt == unv)
            {
                hasChanged = false;
                return;
            }
            Color col = ColorTranslator.FromHtml(txt);
            double convert = 1.0/255;
            value = new double[] {convert*col.R, convert*col.G, convert*col.B};
            hasChanged = true;
        }
        public override bool write()
        {
            double[] rgb = (double[])value; //rgb values in form 0 - 1
            string hex = rgbDoubleColorToHexStringColor(rgb);
            label.Text = hex;
            label.ForeColor = ColorTranslator.FromHtml(hex);
            return true;
        }
        public override void clear()
        {
            value = -1;
            label.Text = unv;
            label.ForeColor = Color.Black;
        }

        public static string rgbDoubleColorToHexStringColor(double[] rgb)
        {
            if(rgb.Length != 3)
                throw new System.Exception("Erron in rgbDoubleColorToHexStringColor(): More than 3 rgb values!");
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
    }

    
}