namespace pdfParserByMH
{
    public sealed partial class pdfParser
    //The 'sealed' keyword means that this class cannot inherit to other classes
    {
        //public string filePath {get;} > C# 5
        public string filePath {get; private set;}
        //public byte[] arrAllBytes {get;} > C# 5
        public byte[] arrAllBytes {get; private set;}
        //public int numAllBytes {get;} > C# 5
        public int numAllBytes {get; private set;}
        public pdfObjectReference rootObRef {get; private set;}
        public pdfObjectReference infoObRef {get; private set;}
        private xrefTable xrefTab;
        public xrefCollection xrefCol {get; private set;}
        public pdfParser(string path)
        {
            filePath = path;
            arrAllBytes = System.IO.File.ReadAllBytes(filePath);
            numAllBytes = arrAllBytes.Length;
            xrefTab = new xrefTable(arrAllBytes);
            xrefCol = new xrefCollection();
            rootObRef = null;
            infoObRef = null;
            getXRefTable();
            getXRefCollection();
        }
    }
}