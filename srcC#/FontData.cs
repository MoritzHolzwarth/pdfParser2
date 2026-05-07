using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Forms;
namespace pdfParserByMH
{

    public sealed class FontData
    {
        //the fontDicts have unicode Code points (Hex numbers as strings) as keys
        //the values are arrays containting integers which are:
        //the charcode of the character that is used in pdfs, the characters width in units fontsize/1000, and the characters minimal bounding box.

        //This fontDataDictDict maps a Font Name String (like "/Helvetica") to a Dictionary, which maps a Unicode Point String (like "0041", i.e. 'A')
        //To an int[] Array, which contains: 
        //1. that Symbol's pdf-Charcode (a code used within PDF to represent various Charackters (some Charackters are not known to PDF, they have Charcode -1))
        //2. that Symbol's standardized Width w in Units of 1000 / Font-Size (meaning the 'actual' Width is obtained by (w/1000)*Font-Size)
        //3. that Symbol's minimal Bounding Box in same Units as the standardized Width
        private System.Collections.Generic.Dictionary<string,System.Collections.Generic.Dictionary<string,int[]>> fontDataDictDict;
        //This fontUnicodeMapStreamDict maps a Font Name String (like "/Helvetica") to a pdfStream, which contains the socalled '/ToUnicode' Map.
        //That is a specific Map between Unicode Points and PDF-Style Charcodes of certain Charackters in a given Font.
        //The Presence of a /ToUnicode Map allows Text-Selection in the pdf-Document for Text of that Font.
        private System.Collections.Generic.Dictionary<string,pdfStream> fontUnicodeMapStreamDict;
        public FontData()
        {
            try
            {
                fontDataDictDict = new System.Collections.Generic.Dictionary<string,System.Collections.Generic.Dictionary<string,int[]>>();
                fontUnicodeMapStreamDict = new System.Collections.Generic.Dictionary<string,pdfStream>();
                var assembly = System.Reflection.Assembly.GetCallingAssembly(); //I have no Idea what this 'assembly' thing actually is
                //During Compilation of this Program, several .json files were stored in this Program's Binary 
                //(actuall not Binary, but 'Intermediate Language Code' which is jit-converted to Binary by the .NET-Runtime)
                //Anyway, these .json files are now in this Program's Ressources, from where they can be obtained anytime.
                //Each .json file contains Data for one of the PDF-Standart-Fonts
                foreach(string res in assembly.GetManifestResourceNames().Where(n => n.EndsWith(".json")))
                {
                    string fontName = "/" + getFontName(System.IO.Path.GetFileNameWithoutExtension(res));
                    var resStream = assembly.GetManifestResourceStream(res);
                    var memStream = new System.IO.MemoryStream();
                    resStream.CopyTo(memStream);
                    byte[] fontData = memStream.ToArray(); //This is the origial textual content of the .json file, stored in a long array of ASCII-Bytes. 
                    fontDataDictDict.Add(fontName,makeFontDataDict(fontData));  //We create our two desired Dictonaries from this "raw" fontData
                    fontUnicodeMapStreamDict.Add(fontName, makeFontUnicodeMapStream(fontName));
                }
            }
            catch(System.Exception exc)
            {
                System.Console.Error.WriteLine("Exception caught in FontData.FontData()!");
                System.Console.Error.WriteLine(exc);
                System.Environment.Exit(1);
            }
        }

        public pdfStream getToUnicodeMap(string fontName)
        {
            return fontUnicodeMapStreamDict[fontName];
        }

        public double getStandardizedTextLineWidth(string str, string fontName)
        {
            var dict = fontDataDictDict[fontName];
            double pointWidth = 0;
            foreach(char ch in str.ToArray())
            {
                string unicode = ((int)ch).ToString("X4");
                pointWidth += dict[unicode][1];
            }
            return pointWidth / 1000;
        }

        private string getFontName(string fontPath)
        {
            int i=fontPath.Length-1;
            while(fontPath[i] != '.' && i>0)
            {
                i--;
            }
            i++;
            return fontPath.Substring(i,fontPath.Length-i);
        }

        private System.Collections.Generic.Dictionary<string,int[]> makeFontDataDict(byte[] fontData)
        {
            var dict = new System.Collections.Generic.Dictionary<string,int[]>();
            string unicode = "";
            int[] charData = new int[6];
            int k=0; //k counts the Fields in a charData Array: 0 -> pdf-Charcode, 1 -> charWidth, 2 -> BB_LLx, 3 -> BB_LLy, 4 -> BB_URx, 5 -> BB_URy.
            int i=0; //i counts the Bytes in the fontData Array (or Span), all Bytes are ASCII.
            ByteSpan dataSpan = new ByteSpan(fontData);
            while(i<dataSpan.Length-1)
            {
                byte b0 = dataSpan[i];
                if(b0 == 0x22) //0x22 is ASCII for '"'. This meand a 4-Byte-Unicode-Point (like "0041") follows, followed by another '"'-
                {
                    byte[] hex = dataSpan.Slice(i+1,4).ToArray();
                    unicode = Utils.bytesToString(hex);
                    dataSpan = dataSpan.Slice(i+6); //Move past the 6-Byte-Unicode-Point (like "0041"). 
                    i = 0; //Setting i=0 corresponds to placing i to the Start of the new, translated dataSpan.
                    continue;
                }
                if(Utils.byteIsASCIINumeric(b0)) //This means that some numeric Field is reached in the Data. //Note: checking 'only' for numeric but assuming integer
                {
                    dataSpan = dataSpan.Slice(i); //So we move the Span so it starts at that Data Field
                    charData[k] = Utils.readASCIIInteger(ref dataSpan); //Read the Data Field (assuming all Data is Integer) and automatically move the Span past the read Bytes
                    if(k==5) //Every Unicode-Point has a 6 Data-Fields, so k=5 means the Data Array for the current Unicode-Point is complete
                    {
                        dict.Add(unicode,charData); //Store the current Data Array, and create a new one.
                        charData = new int[6];
                    }
                    k = (k + 1) % 6; 
                    i = 0;
                    continue;
                }
                i++; //Default: just increment through the dataSpan's Byte until either '"' or ASCIINumeric is found
            }
            return dict;
        }
        
        private pdfStream makeFontUnicodeMapStream(string fontName)
        {
            var origianlFontDataDict = fontDataDictDict[fontName];
            var usableFontDataDict = new System.Collections.Generic.Dictionary<string,int[]>(origianlFontDataDict.Count);
            System.Collections.Generic.List<byte> lstBytes = new System.Collections.Generic.List<byte>();
            foreach(string key in origianlFontDataDict.Keys)
            {
                int[] entry = origianlFontDataDict[key]; //Only some of the unicode points have a valid pdf-Charcode (valid meaning >= 0)
                if(entry[0] >= 0)
                {
                    usableFontDataDict.Add(key,entry);
                }
            }
            string[] arrUnicodes = usableFontDataDict.Keys.ToArray();
            int numFullSubTables = usableFontDataDict.Count/100; //automatically applying Floor() by dividing two integers
            int remainingSubTableLength = usableFontDataDict.Count % 100;
            string toUnicodeHeader = "/CIDInit /ProcSet findresource begin\n12 dict begin\nbegincmap\n/CIDSystemInfo << /Registry (Adobe) /Ordering (UCS) /Supplement 0 >> def\n/CMapName /CustomToUnicode def\n/CMapType 2 def\n\n1 begincodespacerange\n<00> <FF>\nendcodespacerange\n\n";
            lstBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(toUnicodeHeader));

            for(int i=0; i<numFullSubTables; i++)
            {
                lstBytes.AddRange(Utils.stringToBytes("100 beginbfchar\n"));
                for(int j=0; j<100; j++)
                {
                    string unicode = arrUnicodes[100*i+j];
                    int charCode = usableFontDataDict[unicode][0];
//                    string entry = $"<{unicode}> {charCode.ToString("X2")}\n";
                    string entry = string.Format("<{0}> <{1}>\n", unicode, charCode.ToString("X2"));
                    lstBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(entry));
                }
                lstBytes.AddRange(Utils.stringToBytes("endbfchar\n\n"));
            }
            lstBytes.AddRange(Utils.stringToBytes("100 beginbfchar\n"));
            for(int i=0; i <remainingSubTableLength; i++)
            {
                string unicode = arrUnicodes[100*numFullSubTables+i];
                int charCode = usableFontDataDict[unicode][0];
//                string entry = $"<{unicode}> {charCode.ToString("X2")}\n";
                string entry = string.Format("<{0}> <{1}>\n", unicode, charCode.ToString("X2"));
                lstBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(entry));
            }
            lstBytes.AddRange(Utils.stringToBytes("endbfchar\n\n"));
            lstBytes.AddRange(Utils.stringToBytes("endcmap\nCMapName currentdict /CMap defineresource pop\nend"));

            pdfDictionary dict = new pdfDictionary();
            dict.add("/Type",new pdfName("/CMap"));
            dict.add("/Filter",new pdfName("/FlateDecode"));
            dict.add("/Length",new pdfInteger(0)); 
            pdfStream stream = new pdfStream(dict,new byte[0]);
            stream.addDataToBeginning(lstBytes.ToArray()); //This will add the acutal content to the stream, compress it, and adjust the /Length
            return stream;
        }
    }
}