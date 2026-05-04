namespace pdfParserByMH
{
    public sealed partial class pdfDocument
    {
        private byte[] writePDFObjectReference(pdfObjectReference obRef)
        {
            System.Collections.Generic.List<byte> lstBytes = new System.Collections.Generic.List<byte>(8);
            lstBytes.AddRange(Utils.numberToASCIIBytes(obRef.index));
            lstBytes.Add(0x20);
            lstBytes.AddRange(Utils.numberToASCIIBytes(obRef.generation));
            lstBytes.Add(0x20);
            lstBytes.Add((byte)'R');
            return lstBytes.ToArray();
        }

        private byte[] writePDFDictionary(pdfDictionary dict)
        {
            System.Collections.Generic.List<byte> lstBytes = new System.Collections.Generic.List<byte>();
            lstBytes.AddRange(new byte[] {0x3C, 0x3C}); // 0x3C = '<'
            foreach(string key in dict.keys())
            {
                lstBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(key));
                lstBytes.Add(0x20);
                lstBytes.AddRange(writePDFEntity(dict.get(key)));
                lstBytes.Add(0x20);
            }
            lstBytes.AddRange(new byte[] {0x3E, 0x3E}); // 0x3E = '>'
            return lstBytes.ToArray();
        }

        private byte[] writePDFArray(pdfArray arr)
        {
            System.Collections.Generic.List<byte> lstBytes = new System.Collections.Generic.List<byte> ();
            lstBytes.Add((byte)'[');
            for(int i=0; i<arr.count(); i++)
            {
                lstBytes.AddRange(writePDFEntity(arr.get(i)));
                lstBytes.Add(0x20);  // 0x20 = space bar
            }
            lstBytes.Add((byte)']');
            return lstBytes.ToArray();
        }

        private byte[] writePDFStream(pdfStream stream)
        {
            System.Collections.Generic.List<byte> lstBytes = new System.Collections.Generic.List<byte>();
            lstBytes.AddRange(writePDFDictionary(stream.dictionary));
            lstBytes.AddRange(System.Text.Encoding.ASCII.GetBytes("\nstream\n"));
            lstBytes.AddRange(stream.data);
            lstBytes.AddRange(System.Text.Encoding.ASCII.GetBytes("\nendstream"));
            return lstBytes.ToArray();
        }

        private byte[] writePDFEntity(pdfEntity entity)
        {
            switch(entity)
            {
                case pdfStringEntity strEntity:
                    return System.Text.Encoding.ASCII.GetBytes(strEntity.value);
                case pdfInteger num:
                    return Utils.numberToASCIIBytes(num.value);
                case pdfReal num:
                    return Utils.numberToASCIIBytes(num.value);
                case pdfObjectReference obRef:
                    return writePDFObjectReference(obRef);
                case pdfArray arr:
                    return writePDFArray(arr);
                case pdfDictionary dict:
                    return writePDFDictionary(dict);
                case pdfStream stream:
                    return writePDFStream(stream);
                default:
                    throw new System.Exception($"Error in pdfDocument.writePDFEntity(): unknown pdfEntityType {entity.GetType()}");
            }
        }

        

        
    }
}