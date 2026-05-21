

using System.ComponentModel;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Windows.Markup;
using pdfParserByMH;

namespace pdfParserByMH
{
    public class DocProperty
    {
        public static string unv = "/unverändert";
        public DocProperty()
        {}
        public virtual bool read(Form form)
        {
            return false;
        }
    }

    public class DocStringProperty : DocProperty
    {
        public string value {get; set;}
        public  DocStringProperty(string val =null)
        {
            value = val;
        }
    }

    public class DocPageXPosProperty : DocProperty
    {
        public PageXPosition value {get; set;}
        public DocPageXPosProperty(PageXPosition val =PageXPosition.Null)
        {
            value = val;
        }
    }

    public class DocPageQuantifierProperty : DocProperty
    {
        public PageQuantifiers value {get; set;}
        public DocPageQuantifierProperty(PageQuantifiers val =PageQuantifiers.Null)
        {
            value = val;
        }
    }

    public class DocIntArrayProperty : DocProperty
    {
        public int[] value {get; set;}
        public DocIntArrayProperty(int[] val = null)
        {
            value = val;
        }
    }


    public class DocFolderName : DocStringProperty
    {
        public DocFolderName(string name =null)
        {
            value = name;
        }
        public override bool read(Form form)
        {
            string txt = ((TextBox)form.Controls["DocFolderName"]).Text;
            if(txt == unv)
                return false;
            value = txt;
            return true;
        }
    }

    public class DocApproxFilename : DocStringProperty
    {
        public DocApproxFilename(string name =null)
        {
            value = name;
        }
        public override bool read(Form form)
        {
            string txt = ((TextBox)form.Controls["DocApproxFileName"]).Text;
            if(txt == unv)
                return false;
            value = txt;
            return true;
        }
    }

    public class DocHeader : DocStringProperty
    {
        public DocHeader(string val =null)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            string txt = ((TextBox)form.Controls["DocHeader"]).Text;
            if(txt == unv)
                return false;
            value = txt;
            return true;
        }
    }

    public class DocFooter : DocStringProperty
    {
        public DocFooter(string val =null)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            string txt = ((TextBox)form.Controls["DocFooter"]).Text;
            if(txt == unv)
                return false;
            value = txt;
            return true;
        }
    }

    public class DocHeaderXPos : DocPageXPosProperty
    {
        public DocHeaderXPos(PageXPosition val =PageXPosition.Null)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            GroupBox box = (GroupBox)form.Controls["box_HeaderXPos"];
            PageXPosition newVal = FrontEndProgram.getPageXPositionFromBox(box);
            if(newVal == PageXPosition.Null)
                return false;
            value = newVal;
            return true;
        }
    }

    public class DocFooterXPos : DocPageXPosProperty
    {

        public DocFooterXPos(PageXPosition val =PageXPosition.Null)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            GroupBox box = (GroupBox)form.Controls["box_FooterXPos"];
            PageXPosition newVal = FrontEndProgram.getPageXPositionFromBox(box);
            if(newVal == PageXPosition.Null)
                return false;
            value = newVal;
            return true;
        }
    }

    public class DocVertPageQuantifier : DocPageQuantifierProperty
    {
        public DocVertPageQuantifier(PageQuantifiers val =PageQuantifiers.Null)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            GroupBox box = (GroupBox)form.Controls["box_VertPages"];
            PageQuantifiers newVal = FrontEndProgram.getPageQuantifierFromBox(box);
            if(newVal == PageQuantifiers.Null)
                return false;
            value = newVal;
            return true;
        }
    }

    public class DocHoriPageQuantifier : DocPageQuantifierProperty
    {
        public DocHoriPageQuantifier(PageQuantifiers val =PageQuantifiers.Null)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            GroupBox box = (GroupBox)form.Controls["box_HoriPages"];
            PageQuantifiers newVal = FrontEndProgram.getPageQuantifierFromBox(box);
            if(newVal == PageQuantifiers.Null)
                return false;
            value = newVal;
            return true;
        }
    }

    public class DocVertPageNumbers : DocIntArrayProperty
    {
        public DocVertPageNumbers(int[] val =null)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            TextBox box = (TextBox)((GroupBox)form.Controls["box_VertPages"]).Controls["DocVertPageNumbers"];
            string txt = box.Text;
            if(txt == unv)
                return false;
            value = FrontEndProgram.getIntArrayFromText(txt);
            return true;
        }
    }

    public class DocHoriPageNumbers : DocIntArrayProperty
    {
        public DocHoriPageNumbers(int[] val =null)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            TextBox box = (TextBox)((GroupBox)form.Controls["box_HoriPages"]).Controls["DocHoriPageNumbers"];
            string txt = box.Text;
            if(txt == unv)
                return false;
            value = FrontEndProgram.getIntArrayFromText(txt);
            return true;
        }
    }

    public class DocScaleFactor : DocProperty
    {
        public double value {get; set;}
        public DocScaleFactor(double val =1)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            TextBox box = (TextBox)form.Controls["DocScaleFactor"];
            string txt = box.Text;
            if(txt == unv)
                return false;
            value = FrontEndProgram.getDoubleFromText(txt);
            return true;
        }
    }

    public class DocTextColor : DocProperty
    {
        public double[] value {get; set;}
        public DocTextColor(double[] val =null)
        {
            value = val;
        }
        public override bool read(Form form)
        {
            Label lab = (Label)form.Controls["DocTextColor"];
            string txt = lab.Text;
            if(txt == unv)
                return false;
            value = FrontEndProgram.getDoubleArrayFromHexColorText(txt);
            return true;
        }
    }
}