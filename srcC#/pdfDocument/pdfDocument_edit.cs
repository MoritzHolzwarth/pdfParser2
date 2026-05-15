using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Linq;

namespace pdfParserByMH
{
    public sealed partial class pdfDocument
    {
        public bool stempeln(string headerText, string footerText, PageXPosition headerX, PageXPosition footerX, string fontName, int fontSize, double[] fontColor, 
                            double scaleFactor = 1, PageQuantifiers forceVertical = PageQuantifiers.None, PageQuantifiers forceHorizontal = PageQuantifiers.None, 
                            int[] specificVerticalPages = null, int[] specificHorizontalPages = null, int xDistance = 20, int yDistance = 20)
        {
            System.Console.Error.WriteLine("pdfDocument started stempeln");
            scaleAllPages(scaleFactor);
            addHeaderAndFooterToAllPages(headerText, footerText, headerX, footerX, fontName, fontSize, fontColor, forceVertical, forceHorizontal,
                                            specificVerticalPages, specificHorizontalPages, xDistance, yDistance);
            System.Console.Error.WriteLine("pdfDocument finished stempeln");
            return true;
        }

        public bool removeHeaderAndFooterFromAllPages()
        {
            try
            {
                foreach(pdfPage page in pagesDict.Values)
                {
                    page.removeHeaderAndFooter();
                }
            }
            catch(System.Exception exc)
            {
                System.Console.Error.WriteLine("Exception caught in pdfDocument.removeHeaderAndFooterFromAllPages()");
                System.Console.Error.WriteLine(exc);
                return false;
            }
            return true;
        }


        public pdfObjectReference addFontObjectToDocument(string fontName)
        {
            pdfStream toUnicodeMap = fontData.getToUnicodeMap(fontName);
            int toUnicodeIndex = xref.getMinimalFreeIndex();
            xref.add(toUnicodeIndex,toUnicodeMap);
            pdfDictionary fontDict = new pdfDictionary();
            fontDict.add("/Type",new pdfName("/Font"));
            fontDict.add("/Subtype",new pdfName("/Type1"));
            fontDict.add("/BaseFont",new pdfName(fontName));
            fontDict.add("/Encoding",new pdfName("/WinAnsiEncoding"));
            fontDict.add("/ToUnicode",new pdfObjectReference(toUnicodeIndex,0));
            fontDict.ensureResolvedDict(xref);
            int fontIndex = xref.getMinimalFreeIndex();
            xref.add(fontIndex,fontDict);
            return new pdfObjectReference(fontIndex,0);
        }

        public void scaleAllPages(double scaleFactor = 1)
        {
            string closingCommand = "\nQ"; //This Command shall be set at the end of every single page, 
            // it is the closing couter part to a scaling command (which may differ among pages) at the start of each page.
            //Below is the pdfObRef to a pdfStream which contains the closing command.
            //Instead of writing an extra copy of the closing command for every page, we just reference the same bwlow content stream at the end of every page.
            pdfObjectReference closingStreamObRef = createNewStreamFromUncompressedData(Utils.stringToBytes(closingCommand));
            
            //The scaling command (which shall be at the page's start) may vary among pages due to possibly varying mediaBoxes.
            // Still, all pages with the same mediaBox values can use the same content stream obRef for the starting pdfStream.
            //For tracking such pages with same or different mediaBoxes, the below dictionary is used.
            var dict_MediaBoxToStreamObRef = new System.Collections.Generic.Dictionary<ValueTuple<double,double,double,double>, pdfObjectReference>();
            pdfObjectReference startingStreamObRef;
            foreach(pdfPage page in pagesDict.Values)
            {
                double[] mediaBox = page.mediaBox; //The keys are not mediaBox double[] arrays but tuples because tuples get compared by Value while double[] get compared by reference.
                var mediaBoxTuple = new ValueTuple<double, double, double, double>(mediaBox[0], mediaBox[1], mediaBox[2], mediaBox[3]);
                if(dict_MediaBoxToStreamObRef.ContainsKey(mediaBoxTuple)) //If the current page's media box is already known (from an earlier page)
                {                                                         //use that earlier page's starting stream obRef for this page (i.e. adding it to the start of this page's content stream references)
                    startingStreamObRef = dict_MediaBoxToStreamObRef[mediaBoxTuple];
                    page.addObRefToContent(startingStreamObRef, true);
                }
                else    //If the current page's mediaBox is not yet known, create a new scaling command for this mediaBox ...
                {
                    double tx = 0.5*(mediaBox[2] - mediaBox[0]);
                    double ty = 0.5*(mediaBox[3] - mediaBox[1]);
                    double vx = tx*(1-scaleFactor);
                    double vy = ty*(1-scaleFactor);
                    //string scaleCommand = $"q {scaleFactor} 0 0 {scaleFactor} {vx} {vy} cm\n";
                    string scaleCommand = string.Format("q {0} 0 0 {1} {2} {3} cm\n", scaleFactor, scaleFactor, vx, vy);

                    startingStreamObRef = createNewStreamFromUncompressedData(Utils.stringToBytes(scaleCommand)); //... and pack that scaling command into a new starting stream
                    dict_MediaBoxToStreamObRef.Add(mediaBoxTuple, startingStreamObRef); //Add the new mediaBox and corresponding starting stream to the dictionary
                    page.addObRefToContent(startingStreamObRef, true); //and, of course, add the starting stream to the start of page's content stream refrences
                }
                page.addObRefToContent(closingStreamObRef, false); //Add the same closing stream to the end of each page's content stream references
            }
        }

        public void addHeaderAndFooterToAllPages(string headerText, string footerText, PageXPosition headerX, PageXPosition footerX, string fontName, int fontSize, 
                                                double[] fontColor, PageQuantifiers forceVertical = PageQuantifiers.None, PageQuantifiers forceHorizontal = PageQuantifiers.None, 
                                                int[] specificVerticalPages = null, int[] specificHorizontalPages = null, int xDistance = 20, int yDistance = 20)
        {
            System.Collections.Generic.HashSet<int> setSpecificVerticalPages = new System.Collections.Generic.HashSet<int>();
            System.Collections.Generic.HashSet<int> setSpecificHorizontalPages = new System.Collections.Generic.HashSet<int>();
            if(!(specificVerticalPages == null))
                setSpecificVerticalPages = new System.Collections.Generic.HashSet<int>(specificVerticalPages);
            if(!(specificHorizontalPages == null))
                setSpecificHorizontalPages = new System.Collections.Generic.HashSet<int>(specificHorizontalPages);
            string fontToken = "";
            pdfObjectReference fontObRef = null;
            foreach(int num in pagesDict.Keys)
            {
                pdfPage page = pagesDict[num];
                if(!page.checkHasFont(fontName, ref fontToken))
                {
                    if(fontObRef == null)
                    {
                        fontObRef = addFontObjectToDocument(fontName);
                    }
                    fontToken = page.addFont(fontObRef,xref);
                }
                bool makeVertical = false;
                bool makeHorizontal = false;
                switch (forceVertical)
                {
                    case PageQuantifiers.All:
                        makeVertical = true;
                        break;
                    case PageQuantifiers.AllExceptArray:
                        if(!setSpecificVerticalPages.Contains(num))
                            makeVertical = true;
                        break;
                    case PageQuantifiers.NoneExceptArray:
                        if(setSpecificVerticalPages.Contains(num))
                            makeVertical = true;
                        break;
                    default:
                        break;
                }
                switch (forceHorizontal)
                {
                    case PageQuantifiers.All:
                        makeHorizontal = true;
                        break;
                    case PageQuantifiers.AllExceptArray:
                        if(!setSpecificHorizontalPages.Contains(num))
                            makeHorizontal = true;
                        break;
                    case PageQuantifiers.NoneExceptArray:
                        if(setSpecificHorizontalPages.Contains(num))
                            makeHorizontal = true;
                        break;
                    default:
                        break;
                }
                page.addHeaderAndFooter(headerText, footerText, headerX, footerX, fontToken, fontName, fontSize, fontColor, makeVertical, makeHorizontal, xDistance, yDistance);
            } 
        }

        public pdfObjectReference createNewStreamFromUncompressedData(byte[] uncompressedData)
        {
            pdfDictionary newDict = new pdfDictionary();
            newDict.add("/Length", new pdfInteger(0));
            newDict.add("/Filter", new pdfName("/FlateDecode"));
            pdfStream newStream = new pdfStream(newDict, new byte[0]); //create a pdfStream with empty data (/Length = 0)
            newStream.addDataToBeginning(uncompressedData);            //then add the desired data to the empty stream
            int newStreamObIndex = xref.getMinimalFreeIndex();
            xref.add(newStreamObIndex,newStream);
            return new pdfObjectReference(newStreamObIndex,0);
        }
    }
}
