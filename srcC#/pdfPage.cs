using System.CodeDom;
using System.Diagnostics.SymbolStore;
using System.Linq;
namespace pdfParserByMH
{
    public partial class pdfPage
    {
        private pdfDocument document;
        public int number {get;}
        private pdfDictionary dictionary;
        public pdfDictionary parent {get;}
        public pdfDictionary resources {get; set;}
        public double[] mediaBox {get; set;}
        private pdfInteger rotateState;
        private System.Collections.Generic.List<pdfStream> lstContents;
        public pdfPage(pdfDocument doc, pdfDictionary dict, int num = -1)
        {
            document = doc;
            dictionary = dict;
            number = num;
            lstContents = new System.Collections.Generic.List<pdfStream>();
            try
            {
                makeMediaBox();
                makeRotationState();
                makeResources();
                makeContents();
            }
            catch(System.Exception exc)
            {
                System.Console.Error.WriteLine($"Exception caught in pdfPage.pdfPage(), pageNum {num}");
                System.Console.Error.WriteLine(exc);
                System.Environment.Exit(1);
            }
        }

        private void makeMediaBox()
        {
            pdfDictionary dict = dictionary;
            while(!dict.containsKey("/MediaBox"))
            {
                if(!dict.containsKey("/Parent"))
                {
                    throw new System.Exception("Error in pdfPage.getPageMediaBox(): pdfDictionary has neither /MediaBox nor /Parent!");
                }
                dict = (pdfDictionary)dict.getResolved("/Parent");
            }
            pdfArray mediaBoxPDFArr = (pdfArray)dict.getResolved("/MediaBox");
            mediaBox = new double[4];
            for(int i=0; i<4; i++)
            {
                pdfEntity number = mediaBoxPDFArr.get(i);
                if(number is pdfInteger pdfint)
                {
                    mediaBox[i] = pdfint.value;
                }
                else if(number is pdfReal pdfreal)
                {
                    mediaBox[i] = pdfreal.value;
                }
                else
                {
                    throw new System.Exception("Error in pdPage.getPageMediaBox(): /MediaBox element is neither pdfInteger nor pdfReal!");
                }
            }
        }

        private void makeResources()
        {
            pdfDictionary dict = dictionary;
            while(!dict.containsKey("/Resources"))
            {
                if(!dict.containsKey("/Parent"))
                {
                    throw new System.Exception("Error in pdfPage.getPageResources(): pdfDictionary has neither /Resources nor /Parent!");
                }
                dict = (pdfDictionary)dict.getResolved("/Parent");
            }
            resources = (pdfDictionary)dict.getResolved("/Resources");
        }

        private void makeRotationState()
        {
            pdfDictionary dict = dictionary;
            while(!dict.containsKey("/Rotate"))
            {
                if(!dict.containsKey("/Parent"))
                {
                    rotateState = new pdfInteger(0);
                    dictionary.add("/Rotate",rotateState); //Add the /Rotate flag to the pages pdfDictionary, not its parent's
                    return;
                }
                dict = (pdfDictionary)dict.getResolved("/Parent");
            }
            rotateState = (pdfInteger)dict.getResolved("/Rotate");
        }

        private void makeContents()
        {
            if(!dictionary.containsKey("/Contents"))
            {
                lstContents = new System.Collections.Generic.List<pdfStream>();
                return;
            }
            pdfEntity contentsEntity = dictionary.getResolved("/Contents");
            if(contentsEntity is pdfStream pdfstream)
            {
                lstContents.Add(pdfstream);
            }
            else if(contentsEntity is pdfArray pdfarr)
            {
                for(int i=0; i < pdfarr.count(); i++)
                {
                    lstContents.Add((pdfStream)pdfarr.getResolved(i));
                }
            }
            else
            {
                throw new System.Exception("Error i npdfPage.getContents(): /Contents are neither pdfStream nor pdfArray!");
            }
        }

        public bool checkHasFont(string fontName, ref string fontToken)
        {
            if(!resources.containsKey("/Font"))
            {
                return false;
            }
            pdfDictionary fontDict = (pdfDictionary)resources.getResolved("/Font");
            foreach(string token in fontDict.keys())
            {
                pdfDictionary specificFontDict = (pdfDictionary)fontDict.getResolved(token);
                if(!specificFontDict.containsKey("/Name"))
                {
                    return false;
                }
                pdfName specificFontName = (pdfName)specificFontDict.getResolved("/Name");
                if(specificFontName.value == fontName)
                {
                    fontToken = token;
                    return true;
                }
            }
            return false;
        }
    }
}