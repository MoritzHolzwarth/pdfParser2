using System.CodeDom;
using System.Data.Common;
using System.Resources;

namespace pdfParserByMH
{
    public partial class Utils
    {
        static public pdfInteger readPFDInteger(ref ByteSpan span)
        {
            skipPDFComment(ref span);
            pdfInteger pdfInt = new pdfInteger(readASCIIInteger(ref span));
            skipASCIIWhiteSpaces(ref span);
            return pdfInt;
        }

        static public pdfReal readPDFReal(ref ByteSpan span)
        {
            skipPDFComment(ref span);
            pdfReal realNum = new pdfReal(readASCIIReal(ref span));
            skipASCIIWhiteSpaces(ref span);
            return realNum;
        }

        static public pdfName readPDFName(ref ByteSpan span, bool skipTrailingWhiteSpaces = true)
        {
            skipPDFComment(ref span);
            if(!(span[0] == '/'))
                throw new System.Exception("Error in Utils.readPDFName(): span does not start with '/'");
            byte[] forbiddenChars = {(byte)'/', (byte)'\\', (byte)'(', (byte)')', (byte)'[', (byte)']', (byte)'{', (byte)'}', (byte)'<', (byte)'>', (byte)'%'};
            int i = 1;
            byte b = span[i];
            while( System.Array.IndexOf(forbiddenChars, b) < 0 && !byteIsASCIIWhitheSpace(b) && i < span.Length-1)
            {
                i++;
                b = span[i];
            }
            pdfName name = new pdfName(Utils.bytesToString(span.Slice(0,i)));
            span = span.Slice(i);
            if(skipTrailingWhiteSpaces)
                skipASCIIWhiteSpaces(ref span);
            return name;
        }

        static public pdfString readPDFString(ref ByteSpan span)
        {
            skipPDFComment(ref span);
            if(span[0] != '(')
            {
                throw new System.Exception("Error in Utils.readPFDString(): span does not start with '('!");
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
            //Check if the created pdfString seems legit.
            if(checkBehindPDFString1(span.Slice(i)) && checkBehindPDFString2(span.Slice(i)))
            {
                pdfString pdfStr1 = new pdfString(Utils.bytesToString(span.Slice(0,i)));
                span = span.Slice(i);
                skipASCIIWhiteSpaces(ref span);
                return pdfStr1;
            }

            //If the string was not legit, it may be because it included a ')' that was not properly escaped with '\'
            //So now we try a brute force trouble shooting method to extract the true string
            i=1;
            while(i < span.Length-1)
            {
                while(span[i] != ')') //move to the first ')' encounter
                {
                    i++;
                }
                i++; //move past the encountered ')'
                //Check behind that ')' to see if it was a closing token or part of the content
                if(checkBehindPDFString1(span.Slice(i)) && checkBehindPDFString2(span.Slice(i)))
                {
                    break;
                }
            }
            if(i > span.Length)
                throw new System.Exception("Error in Utils.readPDFString(): no valid pdfString could be extracted!");
            pdfString pdfStr2 = new pdfString(Utils.bytesToString(span.Slice(0,i)));
            span = span.Slice(i);
            skipASCIIWhiteSpaces(ref span);
            return pdfStr2;
        }

        static public bool checkBehindPDFString1(ByteSpan span)
        {
            //possible words that are allowed to come after a pdfString
            skipASCIIWhiteSpaces(ref span);
            if(span.Length == 0)
            {
                return true;
            }
            if(spanStartsWithGivenText(span,"null") || spanStartsWithGivenText(span,"true") || spanStartsWithGivenText(span,"false") || spanStartsWithGivenText(span,"endobj"))
            {
                return true;
            }
            //possible special symbols that are allowed to come after a pdfString
            char[] possibleSymbols = new char[] {'/','(','[',']','+','-','.','<','>','%'};
            byte b0 = span[0];
            foreach(char symb in possibleSymbols)
            {
                if(b0 == (byte)symb)
                {
                    return true;
                }
            }
            //the symbols '0' to '9' are also allowes to come after a pdfString
            if(b0 > 47 && b0 < 58)
            {
                return true;
            }
            return false;
        }

        static public bool checkBehindPDFString2(ByteSpan span)
        {
            if(span.Length == 0)
            {
                return true;
            }
            //using Latin1 Encoding, because the span may be much longer than the pdfString and contain non-ASCII bytes
            string spanString = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(span.ToArray()); 
            //The regex checks after the pdfString (after its closing ')'), if there comes another ')' without a '(' or 'endobj' or 'stream' before it.
            //(if there comes an 'endobj' before the next ')', that is fine because it means that this misterious next ')' does (most likely) not belong to the current object and cannto hurt our string's integrity.
            //Similarly, if there comes a 'stream' before the next ')', that is fine because it means that this misterious next ')' does (most likely) belong to a stream-data and cannot hurt our string's integrity.)
            //This can only be the case if the pdfString's assumed closing ')' was not actually a closing token but part of the pdfString's content.
            //Hence it would mean that the pdfString was not correctly parsed.
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^((?!(endobj|stream|\())[\s\S])*\)",System.Text.RegularExpressions.RegexOptions.Compiled);
            return !reg.IsMatch(spanString);
        }

        // static public bool checkBehindPDFString2(ByteSpan span)
        // {
        //     if(span.Length == 0)
        //     {
        //         return true;
        //     }
        //     //using Latin1 Encoding, because the span may be much longer than the pdfString and contain non-ASCII bytes
        //     string spanString = System.Text.Encoding.GetEncoding("iso-8859-1").GetString(span.ToArray()); 
        //     //The regex checks after the pdfString (after its closing ')'), if there comes another ')' without a '(' or 'endobj' or 'stream' before it.
        //     //(if there comes an 'endobj' before the next ')', that is fine because it means that this misterious next ')' does (most likely) not belong to the current object and cannto hurt our string's integrity.
        //     //Similarly, if there comes a 'stream' before the next ')', that is fine because it means that this misterious next ')' does (most likely) belong to a stream-data and cannot hurt our string's integrity.)
        //     //This can only be the case if the pdfString's assumed closing ')' was not actually a closing token but part of the pdfString's content.
        //     //Hence it would mean that the pdfString was not correctly parsed.
        //     System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"^((?!(endobj|stream|\())[\s\S])*\)",System.Text.RegularExpressions.RegexOptions.Compiled);
        //     return !reg.IsMatch(spanString);
        // }

        static public pdfHexString readPDFHexString(ref ByteSpan span)
        {
            skipPDFComment(ref span);
            if(!(span[0] == '<'))
            {
                throw new System.Exception("Error in Utils.readPDFHexString(): span does not start with '<'!");
            }
            int i = 1;
            byte b = span[i];
            while(!(b == '>') && i < span.Length-1)
            {
                i++;
                b = span[i];
            }
            i++; //increment one last time for the final '>'
            pdfHexString hexStr = new pdfHexString(Utils.bytesToString(span.Slice(0,i)));
            span = span.Slice(i);
            skipASCIIWhiteSpaces(ref span);
            return hexStr;
        }

        static public pdfObjectReference readPDFObjectReference(ref ByteSpan span)
        {
            skipPDFComment(ref span);
            int index = readASCIIInteger(ref span);
            skipPDFComment(ref span);
            int gen = readASCIIInteger(ref span);
            skipPDFComment(ref span);
            if(span[0] != 'R')
                throw new System.Exception("Error in pdfParser.readPDFObjectReference(): integers are not followed by 'R'!");
            span = span.Slice(1); //move past 'R'
            skipASCIIWhiteSpaces(ref span);
            return new pdfObjectReference(index, gen);
        }

        static public pdfBool readPDFBool(ref ByteSpan span)
        {
            skipPDFComment(ref span);
            string[] possibleValues = {"null", "true", "false"};
            foreach(string val in possibleValues)
            {
                if(spanStartsWithGivenText(span,val))
                {
                    pdfBool entity = new pdfBool(val);
                    span = span.Slice(val.Length);
                    skipASCIIWhiteSpaces(ref span);
                    return entity;
                }
            }
            throw new System.Exception("Error in Utils.readPDFBool(): span doesn't start with a possible pdfBool value!");
        }

        static public pdfArray readPDFArray(ref ByteSpan span)
        {
            skipPDFComment(ref span);
            if(!(span[0] == '['))
                throw new System.Exception("Error in Utils.readPFDArray(): span does not start with '['!");
            span = span.Slice(1); //move past initial '['
            skipASCIIWhiteSpaces(ref span);
            pdfArray pdfArr = new pdfArray();
            while(!(span[0] == ']'))
            {
                pdfEntity item = readPDFEntity(ref span);
                pdfArr.add(item);
            }
            span = span.Slice(1); //move past final ']'
            skipASCIIWhiteSpaces(ref span);
            return pdfArr;
        }

        static public pdfDictionary readPDFDictionary(ref ByteSpan span)
        {
            skipPDFComment(ref span);
            if(!spanStartsWithGivenText(span,"<<"))
                throw new System.Exception("Error in Utils.readPDFDictionary(): span does not start with '<<'!");
            span = span.Slice(2); //move past '<<'
            skipASCIIWhiteSpaces(ref span);
            pdfDictionary pdfdict = new pdfDictionary();
            int numItems = 0;
            pdfName key = new pdfName();
            pdfEntity val = new pdfEntity();
            while(!(span[0] == '>'))
            {
                if(numItems % 2 == 0)
                {
                    try
                    {
                        key = readPDFName(ref span);
                        numItems++;
                    }
                    catch
                    {
                        skipASCIIWhiteSpaces(ref span);
                        if(spanStartsWithGivenText(span,"endobj"))
                        {
                            System.Console.Error.WriteLine("Warning in Utils.readPFDDictionary(): dictionary was not properly closed.");
                            System.Console.Error.WriteLine("Returning partial dictionary.");
                            return pdfdict;
                        }
                    }
                }
                else
                {
                    val = readPDFEntity(ref span);
                    if(pdfdict.containsKey(key.value))
                    {
                        //System.Console.Error.WriteLine($"Warning in Utils.readPDFDictionary(): Multiple entries with key {key.value} found! Using only the last one."); > C# 5
                        System.Console.Error.WriteLine(string.Format("Warning in Utils.readPDFDictionary(): Multiple entries with key {0} found! Using only the last one.", key.value));
                        pdfdict.set(key.value,val);
                    }
                    else
                    {
                        pdfdict.add(key.value, val);
                    }
                    numItems++; //numItems counts also the double keys-value pairs, because it is only used the above % 2 operation.
                }
            }
            if(!spanStartsWithGivenText(span,">>"))
                throw new System.Exception("Error in Utils.readPDFDictionary(): no '>>' found at end of dictionary!");
            span = span.Slice(2); //move past the final '>>'
            skipASCIIWhiteSpaces(ref span);
            return pdfdict;
        }

        static public pdfEntity readPDFDictOrStream(ref ByteSpan span, xrefTable xref = null)
        {
            skipPDFComment(ref span);
            pdfDictionary dict;
            dict = readPDFDictionary(ref span);
            skipPDFComment(ref span); // check for Comment between end of dictionary and 'stream' token
            if(!spanStartsWithGivenText(span,"stream"))
            {
                return dict;
            }
            span = span.Slice(6); //move past 'stream' keyword
            int dataLen;
            if(dict.get("/Length") is pdfInteger)
            {
                pdfInteger lenPDFInt = (pdfInteger)dict.get("/Length");
                dataLen = lenPDFInt.value;
            }
            else if(xref != null)
            {
                dataLen = ((pdfInteger)xref.getNonRef(dict.get("/Length"))).value;
            }
            else
                throw new System.Exception("Error in Utils.readPDFDictOrStream(): can't read stream /Length without xref!");
            byte[] data = System.Array.Empty<byte>();
            byte[][] possibleBytesBeforeAndAfterData =
            {
                new byte[] {0x0A}, new byte[] {0x0D, 0x0A}, new byte[] {0x0D} //only these are allowed by official pdf specification
            };
            byte[] leadingDelimiter = null;
            byte[] trailingDelimiter = null;
            bool success = false;
            foreach(byte[] bytesBeforeStream in possibleBytesBeforeAndAfterData)
            {
                foreach(byte[] bytesAfterStream in possibleBytesBeforeAndAfterData)
                {
                    if(readStreamDataWithGivenLeadingAndTrailingBytes(ref span, ref data, dataLen, bytesBeforeStream, bytesAfterStream))
                    {
                        leadingDelimiter = bytesBeforeStream;
                        trailingDelimiter = bytesAfterStream;
                        success = true;
                        break; //break out of inner for loop
                    }
                }
                if(success)
                    break; //break out of second for loop
            }
            if(success)
                return new pdfStream(dict, data, leadingDelimiter, trailingDelimiter);
            
            System.Console.Error.WriteLine("Warning in Utils.readPDFStreamOrDict(): No spec-complient stream delimiters found. Starting Trouble Shooting with arbitrary White Space delimiters.");
            troubleShootStreamData1(ref span, ref data, dataLen, ref leadingDelimiter, ref trailingDelimiter);
            return new pdfStream(dict, data, leadingDelimiter, trailingDelimiter);
        }

        static private bool readStreamDataWithGivenLeadingAndTrailingBytes(ref ByteSpan span, ref byte[] data, int dataLen, byte[] bytesBeforeStream, byte[] bytesAfterStream)
        {
            int numBytesBefore = bytesBeforeStream.Length;
            int numBytesAfter = bytesAfterStream.Length;
            int extraStreamLen = numBytesBefore + dataLen + numBytesAfter;
            if(extraStreamLen > span.Length)
            {
                return false;
            }
            if(!spanStartsWithGivenText(span.Slice(extraStreamLen),"endstream"))
            {
                return false;
            }
            bool beforeStreamValid = (numBytesBefore == 0)? true: span.Slice(0, numBytesBefore).SequenceEqual(bytesBeforeStream);
            bool afterStreamValid = (numBytesAfter == 0)? true: span.Slice(numBytesBefore + dataLen, numBytesAfter).SequenceEqual(bytesAfterStream);
            if(!(beforeStreamValid && afterStreamValid))
            {
                return false;
            }
            data = span.Slice(numBytesBefore,dataLen).ToArray();
            span = span.Slice(extraStreamLen+9); //move past data and past "endstream" token (+9)
            return true;
        }

        static private void troubleShootStreamData1(ref ByteSpan span, ref byte[] data, int dataLen, ref byte[] leadingDelimiter, ref byte[] trailingDelimiter)
        {
            //check if behind white space + dataLen + white space, the endstream token is present
            leadingDelimiter = skipAndReturnASCIIWhiteSpaces(ref span);      
            if(dataLen > span.Length)
            {
                throw new System.Exception("Error in Utils.troubleShootStreamData1(): given data /Length goes beyond pdf size!");
            }
            data = span.Slice(0,dataLen).ToArray(); //Already retrieve data, even if not yet clear whether stream is valid
            span = span.Slice(dataLen);             //Move past data
            trailingDelimiter = skipAndReturnASCIIWhiteSpaces(ref span);
            if(!spanStartsWithGivenText(span,"endstream")) //Final check if stream is valid
            {   
                throw new System.Exception("Error in Utils.troubleShootStreamData1(): no 'endstream' token found after byte range of size /Length + White Space!");
            }
            span = span.Slice(9); //move past "endstream" token
        }

        static private void troubleShootStreamData2(ref ByteSpan span, ref byte[] data, int dataLen)
        {
            //check if within dataLen reach the "endstream" token appears. Very risky trouble shooting.
            skipASCIIWhiteSpaces(ref span);  
            ByteSpan span1 = span;
            int i = 0;
            while(i < dataLen)
            {
                if(spanStartsWithGivenText(span1.Slice(i),"endstream"))
                {
                    break;
                }
                i++;
            }
            if(i >= dataLen)
                throw new System.Exception("Error in Utils.troubleShootStreamData1(): no 'endstream' token found!");
            data = span.Slice(0,i).ToArray();
            span = span.Slice(i);
            skipASCIIWhiteSpaces(ref span);
            span = span.Slice(9); //move past "endstream" token
        }

        static public pdfEntity readPDFEntity(ref ByteSpan span, xrefTable xref = null)
        {
            skipPDFComment(ref span);
            byte b0 = span[0];
            pdfEntity entity;
            if(byteIsASCIINumeric(b0))
            {
                entity = readNumericPDFEntity(ref span);
            }
            else if(b0 == '/')
            {
                entity = readPDFName(ref span);
            }
            else if(b0 == '(')
            {
                entity = readPDFString(ref span);
            }
            else if(b0 == 'n' || b0 == 't' || b0 == 'f')
            {
                entity = readPDFBool(ref span);
            }
            else if(b0 == '[')
            {
                entity = readPDFArray(ref span);
            }
            else if(b0 == '<')
            {
                if(span[1] == '<')
                {
  
                    entity = readPDFDictOrStream(ref span, xref);
                }
                else
                {
                    entity = readPDFHexString(ref span); 
                }
            }
            else
                throw new System.Exception("Error in Utils.readPDFEntity(): no valid pdfEntity recognized!");
            return entity;
        }

        static public pdfEntity readNumericPDFEntity(ref ByteSpan span)
        {
            if(!byteIsASCIINumeric(span[0]))
                throw new System.Exception("Error in Utils.readNumericPDFEntity(): span does not start with numeric!");
            ByteSpan tempSpan = span;
            if(tempSpan[0] != '.' && tempSpan[1] != '.') //this excludes numbers like -.1 which would break readASCIIInteger
            {
                readASCIIInteger(ref tempSpan, false); //'false' means dont skip white space after reading
                if(tempSpan[0] == '.')
                {
                    readASCIIReal(ref tempSpan, false);
                    {
                        if(tempSpan[0] == 'E' || tempSpan[0] == 'e')
                        {
                            return new pdfReal(readASCIIScientific(ref span));
                        }
                    }
                    return new pdfReal(readASCIIReal(ref span));
                }
                else if(tempSpan[0] == 'E' || tempSpan[0] == 'e')
                {
                    return new pdfReal(readASCIIScientific(ref span));
                }
                else
                {
                    skipASCIIWhiteSpaces(ref tempSpan); //move past white spaces behind first integer
                    if(byteIsASCIIDigit(tempSpan[0])) //Check for second integer after first integer
                    {
                        readASCIIInteger(ref tempSpan); //read second integer after first and skip whitespaces
                        if(tempSpan[0] == 'R')
                        {
                            return readPDFObjectReference(ref span);
                        }
                    }
                    return new pdfInteger(readASCIIInteger(ref span));
                }
            }
            else
            {   
                double realNUm = readASCIIReal(ref tempSpan, false); //read real number but dont skip white spaces
                if(tempSpan[0] == 'E' || tempSpan[0] == 'e')
                {
                    return new pdfReal(readASCIIScientific(ref span));
                }
                return new pdfReal(readASCIIReal(ref span));
            }
        }

        static public pdfObjectReference readPDFObjectHeader(ref ByteSpan span)
        {
            int index = 0;
            int generation = 0;
            if(byteIsASCIIWhitheSpace(span[0]))
            {
                skipASCIIWhiteSpaces(ref span);
            }
            index = readASCIIInteger(ref span);
            skipPDFComment(ref span);
            generation = readASCIIInteger(ref span);
            skipPDFComment(ref span);
            if(!spanStartsWithGivenText(span,"obj"))
                throw new System.Exception("Error in Utils.readPDFObjectHeader(): no 'obj' token found after two integers!");
            span = span.Slice(3); //move past 'obj' token
            skipASCIIWhiteSpaces(ref span);
            return new pdfObjectReference(index,generation);
        }
    }
}