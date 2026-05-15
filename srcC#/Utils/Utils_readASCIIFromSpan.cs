using System.Data.Odbc;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace pdfParserByMH
{
    public partial class Utils
    {
        static public void skipASCIIWhiteSpaces(ref ByteSpan span)
        {
            if(span.Length == 0)
            {
                return;
            }

            int i = 0;
            byte b = span[i];
            while (byteIsASCIIWhitheSpace(b) && i < span.Length-1)
            {
                i++;
                b = span[i];
            }
            span = span.Slice(i);
        }

        static public void skipPDFComment(ref ByteSpan span)
        {
            if(span.Length == 0)
            {
                return;
            }
            if(span[0] != '%')
            {
                return;
            }
            int i=1;
            byte b = span[i];
            while(b != 0x0A && b != 0x0D && i < span.Length-1)
            {
                i++;
                b = span[i];
            }
            span = span.Slice(i);
            skipASCIIWhiteSpaces(ref span);
            if(span[0] == '%')
            {
                skipPDFComment(ref span);
            }
        }

        static public int readASCIIInteger(ref ByteSpan span, bool skipTrailingWhiteSpaces = true)
        {
            if(span.Length == 0)
                throw new System.Exception("Error in Utils.readASCIIInteger(): span is empty!");
            byte b0 = span[0];
            if(!byteIsASCIIDigit(b0) && b0 != '-' && b0 != '+')
                throw new System.Exception("Error in Utils.readASCIIInteger(): span does not start with a digit or '-' or '+'!");
            int sign = 1;
            if(b0 == '-' || b0 == '+')
            {
                if(span.Length < 2)
                    throw new System.Exception("Error in Utils.readASCIIInteger(): span is only '-' or '+'");
                sign = (b0 == '-')? -1: 1;
                if(!byteIsASCIIDigit(span[1]))
                    throw new System.Exception("Error in Utils.readASCIIInteger(): span is '-' or '+' but not followed by digit");
                span = span.Slice(1); //move past '-' or '+'
            }

            int i = 0;
            byte b = span[i];
            int value = 0;
            while(i < span.Length-1 && byteIsASCIIDigit(b))
            {
                value = 10*value + (b - '0');
                i++;
                b = span[i];
            }
            span = span.Slice(i);
            if(skipTrailingWhiteSpaces)
                skipASCIIWhiteSpaces(ref span);
            return sign*value;
        }

        static public double readASCIIReal(ref ByteSpan span, bool skipTrailingWhiteSpaces = true)
        {
            if(span.Length == 0)
                throw new System.Exception("Error in Utils.readASCIIReal(): span is empty!");
            byte b0 = span[0];
            if(!byteIsASCIINumeric(b0))
                throw new System.Exception("Error in Utils.readASCIIReal(): span does not start numeric symbol!");
            int sign = 1;
            if(b0 == '-' || b0 == '+')
            {
                if(span.Length < 2)
                    throw new System.Exception("Error in Utils.readASCIIReal(): span is only '.', '-', '+' or integer!");
                sign = (b0 == '-')? -1: 1;
                if(!(byteIsASCIIDigit(span[1]) || span[1] == '.'))
                    throw new System.Exception("Error in Utils.readASCIIReal(): span is '-' or '+' but not followed by digit");
                span = span.Slice(1); //move past '-' or '+'
            }
            if(!(byteIsASCIIDigit(span[0]) || byteIsASCIIDigit(span[1]))) //at this point it should be either span[0] = digit, or span[0] = '.' & span[1] = digit
                throw new System.Exception("Error in Utils.readASCIIReal(): span has no digits!");
            int i = 0;
            byte b = span[i];
            double value = 0d;
            while(i < span.Length-1 && byteIsASCIIDigit(b))
            {
                value = 10*value + (b - '0');
                i++;
                b = span[i];
            }
            span = span.Slice(i);
            if(span[0] != '.')
            {
                return sign*value;
            }
            span = span.Slice(1); //move past '.'
            i = 0;
            b = span[i];
            while(i < span.Length-1 && byteIsASCIIDigit(b))
            {
                value += (b - '0')*System.Math.Pow(10,-(i+1));
                i++;
                b = span[i];
            }
            span = span.Slice(i);
            if(skipTrailingWhiteSpaces)
                skipASCIIWhiteSpaces(ref span);
            return sign*value;
        }

        static public double readASCIIScientific(ref ByteSpan span, bool skipTrailingWhiteSpaces = true)
        {
            if(span.Length == 0)
                throw new System.Exception("Error in Utils.readASCIIScientific(): span is empty!");
            if(!byteIsASCIINumeric(span[0]))
                throw new System.Exception("Error in Utils.readASCIIScientific(): span doesn't start with numeric symbol!");
            double value1 = readASCIIReal(ref span,false); //false means: dont skip white spaces behind reading
            if(span[0] != 'E' && span[0] != 'e')
            {
                return value1;
            }
            span = span.Slice(1); //move past 'E'
            if(!byteIsASCIINumeric(span[0]))
                throw new System.Exception("Error in Utils.readASCIIScientific(): no numeric symbol behind 'E'!");
            double exponent = readASCIIReal(ref span, false);
            double value2 = value1 * System.Math.Pow(10,exponent);
            if(skipTrailingWhiteSpaces)
                skipASCIIWhiteSpaces(ref span);
            return value2;
        }
    }
}
        