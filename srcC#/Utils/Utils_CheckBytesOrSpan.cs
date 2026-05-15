namespace pdfParserByMH
{
    public partial class Utils
    {
        static public bool byteIsASCIIDigit(byte b)
        {
            if(b >= '0' && b <= '9')
                return true;
            return false;
        }

        static public bool byteIsASCIINumeric(byte b)
        {
            if(byteIsASCIIDigit(b) || b == '-' || b == '+' || b == '.')
                return true;
            return false;
        }

        static public bool byteIsASCIIWhitheSpace(byte b)
        {
            if (b == 0x20 || b == 0x09 || b == 0x0A || b == 0x0D || b == 0x0C)
            {
                return true;
            }
            return false;
        }

        static public bool spanStartsWithGivenText(ByteSpan span, string text)
        {
            if(span.Length < text.Length)
            {
                return false;
            }

            byte[] target = Utils.stringToBytes(text);
            return span.Slice(0,text.Length).SequenceEqual(target);
        }

        static public bool spanStartsWithPDFObject(ByteSpan span)
        {
            if(span.Length == 0)
                return false;
            if(!byteIsASCIIDigit(span[0]))
                return false;
            readASCIIInteger(ref span); //read past index number
            if(span.Length == 0)
                return false;
            if(!byteIsASCIIDigit(span[0])) 
                return false;
            readASCIIInteger(ref span); //read past generation number
            return spanStartsWithGivenText(span, "obj");        
        }
    }
}