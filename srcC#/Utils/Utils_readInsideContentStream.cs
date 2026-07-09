using System.CodeDom;
using System.Configuration;
using System.Security.AccessControl;

namespace pdfParserByMH
{
    public partial class Utils
    {
        
        public static bool readMarkedContentInStream(ByteSpan span, ref int lenMarkedContent)
        {
            int len = 0;
            len += skipAndCountASCIIWhiteSpaces(ref span);

            if(span[0] != '/')
                return false;
            len += readPDFName(ref span, false).value.Length;
            len += skipAndCountASCIIWhiteSpaces(ref span);

            if(!spanStartsWithGivenText(span, "BMC"))
                return false;
            span = span.Slice(3);
            len += 3;
            len += skipAndCountASCIIWhiteSpaces(ref span);
            
            try
            {
                while(!spanStartsWithGivenText(span, "EMC"))
                {
                    len += readSteamEntity(ref span);
                    len += skipAndCountASCIIWhiteSpaces(ref span);
                }
            }
            catch
            {
                return false;
            }
            span = span.Slice(3);
            len += 3;
            lenMarkedContent = len;
            return true;
        }


    public static int readSteamEntity(ref ByteSpan span)
    {
        if(byteIsASCIINumeric(span[0]))
        {
            double num = readASCIIReal(ref span, false);
            return numberToASCIIBytes(num).Length;
        }
        if(span[0] == '/')
        {
            pdfName name = readPDFName(ref span, false);
            return name.value.Length;
        }
        if(span[0] == '(')
        {
            int len = 0;
            pdfString pdfStr = readStreamString(ref span);
            len += pdfStr.value.Length;
            len += skipAndCountASCIIWhiteSpaces(ref span);
            if(!spanStartsWithGivenText(span, "Tj"))
                throw new System.Exception("Error in Utils.readStreamEntity(): No 'Tj' Token afer closing ')' in stream-literal-string!");
            span = span.Slice(2);
            len += 2;
            if(!checkBehindStreamString(span))
                throw new System.Exception("Error in Utils.readStreamEntity(): Ambiguous Closing of stream-literal-string!");
            return len;
        }
        string[] arrStreamTokens = new string[] {"q", "Q", "BT", "ET", "cm", "rg", "Tf", "TL", "Td", "Tj"};
        foreach(string token in arrStreamTokens)
        {
            if(spanStartsWithGivenText(span, token))
            {
                span = span.Slice(token.Length);
                return token.Length;
            }
        }
        throw new System.Exception("Error in Utils.readStreamEntity(): No valid next Symbol found for Content Stream!");
    }


    public static pdfString readStreamString(ref ByteSpan span)
        {
            if(span[0] != '(')
            {
                throw new System.Exception("Error in Utils.readStreamString(): span does not start with '('!");
            }
            int i = 1;
            byte b = span[i];
            while(i < span.Length)
            {
                if(b == ')') //This is the string-end symbol. But only if it is not preceeded by an odd number of '\'
                {
                    int numofBacksash = 0;
                    int j = i-1;
                    byte bj = span[j];
                    while(j > 0 && bj == '\\') //j>0 because span[0] = '('
                    {
                        numofBacksash++;
                        j--;
                        bj = span[j];
                    }
                    if(numofBacksash % 2 == 0) //If number of '\' was even, the string ends => quit loop
                    {
                        i++; //increment one last time for the final ')'
                        break;
                    }
                }
                i++;
                b = span[i];
            }
            pdfString pdfStr = new pdfString(Utils.bytesToString(span.Slice(0,i)));
            span = span.Slice(i);
            return pdfStr;
        }



        static public bool checkBehindStreamString(ByteSpan span)
        {
            if(span.Length == 0)
            {
                return true;
            }
            //using Latin1 Encoding, because the span may be much longer than the pdfString and contain non-ASCII bytes
            string spanString = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(span.ToArray()); 
            //The regex checks after the pdfString (after its closing ') Tj'), if there comes another ') Tj' without a '(' before it.
            //This can only be the case if the pdfString's assumed closing ')' was not actually a closing token but part of the pdfString's content.
            //Hence it would mean that the pdfString was not correctly parsed.
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^((?!(\())[\s\S])*\)\s*Tj",System.Text.RegularExpressions.RegexOptions.Compiled);
            return !reg.IsMatch(spanString);
        }
    }
}