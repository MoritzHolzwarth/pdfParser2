using System.CodeDom;
using System.Linq;
namespace pdfParserByMH
{

    public class pdfEntity
    {
        public pdfEntity()
        {}
    }

    public class pdfStringEntity : pdfEntity
    {
        public string value {get; set;}
        public pdfStringEntity(string val = "")
        {
            value = val;
        }
    }

    public class pdfInteger : pdfEntity
    {
        public int value {get; set;}
        public pdfInteger(int val = 0)
        {
            value = val;
        }
    }

    public class pdfReal : pdfEntity
    {
        public double value {get; set;}
        public pdfReal(double val = 0d)
        {
            value = val;
        }
    }

    public class pdfName : pdfStringEntity
    {
        public pdfName(string val = "/")
        {
            if(val[0] != '/' )
                throw new System.Exception("Error in pdfName(): value must start with '/'!");
            value = val;
        }
    }

    public class pdfString : pdfStringEntity
    {
        public pdfString(string val = "()")
        {
            if(val[0] != '(' || val[val.Length-1] != ')')
                throw new System.Exception("Error in pdfString(): string must start with '(' and end with ')'");
            value = val;
        }
    }

    public class pdfHexString : pdfStringEntity
    {
        public pdfHexString(string val = "<>")
        {
            if(val[0] != '<' || val[val.Length-1] != '>')
                throw new System.Exception("Error in pdfHexString(): string must start with '<' and end with '>'");
            value = val;
        }
    }

    public class pdfObjectReference : pdfEntity
    {
        public int index {get; set;}
        public int generation {get; set;}
        public pdfObjectReference(int valIndex = 0, int valGen = 0)
        {
            if(valIndex < 0 || valGen < 0)
                throw new System.Exception("Error in pdfObjectReference(): index or generation < 0!");
            index = valIndex;
            generation = valGen;
        }
    }

    public class pdfBool : pdfStringEntity
    {
        public pdfBool(string val = "null")
        {
            if( val != "null" && val != "true" && val != "false")
                throw new System.Exception("Error in pdfBool(): value must be 'null', 'true' or 'false'!");
            value = val;
        }
    }
}