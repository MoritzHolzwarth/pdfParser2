using System.CodeDom;
using System.Configuration;
using System.Security.AccessControl;
using System.Windows.Forms.Layout;

namespace pdfParserByMH
{
    public partial class Utils
    {
        
        public static int readMarkedContentInStream(ByteSpan span)
        {
            int len = 0;
            len += skipAndCountASCIIWhiteSpaces(ref span);

            len += readPDFName(ref span, false).value.Length;
            len += skipAndCountASCIIWhiteSpaces(ref span);
            if(!spanStartsWithGivenText(span, "BMC"))
                throw new System.Exception("Error in readMarkedContentInStream(): Expected 'BMC' Token!");
            span = span.Slice(3);
            len += 3;
            len += skipAndCountASCIIWhiteSpaces(ref span);
            while(!spanStartsWithGivenText(span, "EMC"))
            {
                len += readStreamEntity(ref span);
                len += skipAndCountASCIIWhiteSpaces(ref span);
            }
            span = span.Slice(3);
            len += 3;
            return len;

        }



    public static int readStreamEntity(ref ByteSpan span)
    {
        if(span.Length == 0)
            throw new System.Exception("Error in readStreamEntity(): Reached End of Stream!");
            
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
            pdfString pdfStr = readStreamString(ref span);
            int len = pdfStr.value.Length;
            len += skipAndCountASCIIWhiteSpaces(ref span);
            if(!spanStartsWithGivenText(span, "Tj"))
                throw new System.Exception("Error in Utils.readStreamEntity(): No 'Tj' Token afer closing ')' in stream-literal-string!");
            span = span.Slice(2);
            len += 2;
            if(!checkBehindStreamString(span))
                throw new System.Exception("Error in Utils.readStreamEntity(): Ambiguous Closing of stream-literal-string!");
            return len;
        }
        if(span[0] == '<')
        {
            pdfHexString pdfHexStr = readPDFHexString(ref span, false);
            int len = pdfHexStr.value.Length;
            return len;
        }
        if(span[0] == '[')
        {
            span = span.Slice(1);
            int len = 1;
            len += skipAndCountASCIIWhiteSpaces(ref span);
            while(span[0] != ']')
            {
                if(span[0] == '(')  //Treat it extra, not via readStreamEntity like next line, because that would expect String to be followed by Tj
                    len += readStreamString(ref span).value.Length;
                else if (span[0] == '<' || byteIsASCIINumeric(span[0]))
                    len += readStreamEntity(ref span);
                else
                    throw new System.Exception(string.Format("Error in Utils.readStreamEntity(): Unexpected Element in assumed TJ-Array, starting with {0}!", (char)span[0]));
                
                len += skipAndCountASCIIWhiteSpaces(ref span);
            }
            span = span.Slice(1);   // for the closing ]
            len += 1;
            len += skipAndCountASCIIWhiteSpaces(ref span);
            if(!spanStartsWithGivenText(span, "TJ"))
                throw new System.Exception("Error in Utils.readStreamEntity(): Array, assumed in BT-ET-Block, not followed by TJ!");
            span = span.Slice(2);
            len += 2;
            return len;
        }

        string[] arrStreamTokens = new string[] {"q", "Q", "BT", "ET", "cm", "rg", "gs", "g", "re", "n", "W", "W*", "TL", "TD", "Td", "Tc", "Tw", "Tz", "Tr", "Tf", "Ts", "Tm"};
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