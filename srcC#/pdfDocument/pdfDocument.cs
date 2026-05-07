using System.Data;
using System.Dynamic;

namespace pdfParserByMH
{
    public sealed partial class pdfDocument
    {
        private string outputPath = null;
        public xrefCollection xref {get; private set;}
        private pdfObjectReference rootObRef;
        private pdfObjectReference infoObRef;
        private System.Collections.Generic.Dictionary<int,pdfPage> pagesDict; //Dictionary of Page Numbers and pdfPage Objects.
                                                                              //The Page Numbering starts with 1.
        public int numPages {get; private set;}
        public FontData fontData {get; set;}

        public pdfDocument()
        {
            Utils.InitializeCulture();
            fontData = new FontData();
        }
        public pdfDocument(string inputPath)
        {
            Utils.InitializeCulture();
            fontData = new FontData();
            setFilePath(inputPath);
        }

        public bool setFilePath(string inputPath)
        {
            try
            {
                System.Console.Error.WriteLine("pdfDocument started parsing");
                pdfParser parser = new pdfParser(inputPath); //All the Parsing happens via the Constructor of the pdfParser class
                System.Console.Error.WriteLine("pdfDocument finished parsing");
                xref = parser.xrefCol;                      //Retrieve the xref-Collection, the root ObRef, and info obRef from the pdfParser.
                rootObRef = parser.rootObRef;               
                infoObRef = parser.infoObRef;
                createPagesDict();
                numPages = pagesDict.Count;
            }
            catch(System.Exception exc)
            {
                System.Console.Error.WriteLine("Exception caught in pdfDocument.setFilePath()");
                System.Console.Error.WriteLine(exc);
                return false;
            }
            return true;
        }

        public bool setOutputPath(string path)
        {
            outputPath = path;
            return true;
        }

        private void createPagesDict()
        {
            pagesDict = new System.Collections.Generic.Dictionary<int,pdfPage>();
            pdfDictionary rootDict = (pdfDictionary)xref.getNonRef(rootObRef);
            if(!rootDict.containsKey("/Pages"))
                throw new System.Exception("Error in pdfDocument.createPagesDict(): no /Pages in Root Dictionary!");
            pdfDictionary pagesOrigin = (pdfDictionary)rootDict.getResolved("/Pages");
            System.Collections.Generic.Queue<pdfDictionary> queue = new System.Collections.Generic.Queue<pdfDictionary>();
            queue.Enqueue(pagesOrigin);
            int pageNum = 0;
            while(queue.Count > 0)
            {
                pdfDictionary dict = queue.Dequeue();
                if(!dict.containsKey("/Type"))
                    throw new System.Exception("Error in pdfDocument.createPagesDict(): pdfDictionary has no /Type!");
                string dictType = ((pdfName)dict.getResolved("/Type")).value;
                if(dictType == "/Page")
                {
                    pageNum++;
                    pdfPage page = new pdfPage(this ,dict, pageNum);
                    pagesDict.Add(pageNum,page);
                }
                else if(dictType == "/Pages")
                {
                    if(!dict.containsKey("/Kids"))
                        throw new System.Exception("Error in pdfDocument.createPagesDict(): /Pages object has no /Kids!");
                    pdfEntity kidsEntity = dict.getResolved("/Kids");
                    if(kidsEntity is pdfArray)
                    {
                        pdfArray arrKids = (pdfArray)kidsEntity;
                        for(int i=0; i<arrKids.count(); i++)
                        {
                            queue.Enqueue((pdfDictionary)arrKids.getResolved(i));
                        }
                    }
                    else if(kidsEntity is pdfDictionary)
                    {
                        pdfDictionary dictKid = (pdfDictionary)kidsEntity;
                        queue.Enqueue(dictKid);
                    }
                    else
                        throw new System.Exception("Error in pdfDocument.createPagesDict(): /Kids entity of /Pages dict is neither pdfDictionary nor pdfArray!");
                }
            }
        }
    }
}