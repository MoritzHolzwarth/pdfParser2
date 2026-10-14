using System;
using System.Linq;
namespace pdfParserByMH
{
    public sealed partial class pdfParser
    {
        private int findStartXRef()
        //Find the Byte Offset where the xref Table starts
        {
            string targetString = "startxref";
            int lenTarget = targetString.Length;
            byte[] target = Utils.stringToBytes(targetString);
            int i = lenTarget + 1;                                                                  //+1 because the below spanAllBytes starts at Index 0
            ByteSpan spanAllBytes = new ByteSpan(arrAllBytes);
            ByteSpan span = spanAllBytes.Slice(numAllBytes-i, lenTarget);
            
            while(!span.SequenceEqual(target) && i < 1024)                                      //While the target is not found within the last 1024 Bytes
            {
                i++;
                span = spanAllBytes.Slice(numAllBytes-i, lenTarget);
            }
            if(i >= 1024)                   //In well-produced PDFs, the "startxref" Token should appear within the last 1024 Bytes. But often it doesn't.
            {
                    return troubleShootStartXRef1(spanAllBytes, target, lenTarget);
            }
            span = spanAllBytes.Slice(numAllBytes-i+lenTarget, i-lenTarget);        //This span contains all Bytes that come after the "startxref" Tokem
            Utils.skipASCIIWhiteSpaces(ref span);
            return Utils.readASCIIInteger(ref span);                    //The Integer found behind the "startxref" Token is the xref Table's Byte Offset.
        }

        private int troubleShootStartXRef1(ByteSpan spanAllBytes, byte[] target, int lenTarget)
        //This does the same as findStartXRef(), except that it explicitly skips potential trailing White Spaces, 
        //and also searches the whole PDF-File instead of only the last 1024 Bytes. 
        //The 1 in this function's name is because other troubleshootStartXRef functions may be added.
        {
            int numTrailingWhiteSpaces = 0;                       //PDFs often have useless trailing White Space or 0-Bytes, which we skip here explicitly.
            byte b = spanAllBytes[numAllBytes-numTrailingWhiteSpaces-1]; 
            while(b == 0 || Utils.byteIsASCIIWhitheSpace(b))
            {
                numTrailingWhiteSpaces++;
                b = spanAllBytes[numAllBytes-numTrailingWhiteSpaces-1];
            } 
            int i = lenTarget + numTrailingWhiteSpaces + 1;
            ByteSpan span = spanAllBytes.Slice(numAllBytes-i, lenTarget);
            
            while(!span.SequenceEqual(target) && i < numAllBytes)
            {
                i++;
                span = spanAllBytes.Slice(numAllBytes-i,9);
            }
            if(i >= numAllBytes)
                throw new System.Exception("Error in pdfParser.troubleShootXRef1(): No 'startxref' Token found in the whole file!");
            span = spanAllBytes.Slice(numAllBytes-i+lenTarget, i-lenTarget);
            Utils.skipASCIIWhiteSpaces(ref span);
            return Utils.readASCIIInteger(ref span);
        }

        private void getXRefNormalPart(int startxref)
        //Adds a 'normally' stored Part of the xref Table to the xrefTable Object
        {
            ByteSpan span = new ByteSpan(arrAllBytes, startxref, numAllBytes-startxref);
            Utils.skipASCIIWhiteSpaces(ref span);                                               //move past potential white space before the 'xref' token
            span = span.Slice(4);                                                               //move past the 'xref'token
            Utils.skipASCIIWhiteSpaces(ref span);                                           //move past white spaces between 'xref' token and first number
            while(true) //Loops over all Blocks of this xref Part
            {
                int initObIndex = Utils.readASCIIInteger(ref span);                     //Read the initial Object Index and Object Count of this xref Block
                int numObs = Utils.readASCIIInteger(ref span);
                for(int i = initObIndex; i < initObIndex + numObs; i++)
                {
                    xrefEntry entry = new xrefEntry();
                    entry.index = i;
                    entry.offset = Utils.readASCIIInteger(ref span);
                    entry.generation = Utils.readASCIIInteger(ref span);
                    if(span[0] == 'n')
                    {
                        entry.useStatus = 1;
                    }
                    else if(span[0] == 'f')
                    {
                        entry.useStatus = 0;
                    }
                    else
                    {
                        throw new System.Exception("Error in pdfParser.getXRefNormalPart(): invalid use Status (not 'f' or 'n')!");
                    }
                    span = span.Slice(1); //move past the n/f token
                    Utils.skipASCIIWhiteSpaces(ref span);
                    if(xrefTab.containsKey(i)) //If an object index appears more than once (which happens for some malformed PDFs), only the xref Entry of that index's first appearence will be used
                    {
                        System.Console.WriteLine("Warning in pdfParser.getXRefNormalPart(): Found > 1 Entry with same index in xref Table! -> Only using first occurence!");
                        continue;
                    }
                    if(i!=0 && entry.offset == 0) //Some malformed pdfs have objects in xref table with offset 0, which is not valid for index != 0
                    {
                        System.Console.WriteLine("Warning in pdfParser.getXRefNormalPart(): Found Entry with Offest = 0 but Index != 0 in xref Table! -> Ignoring!");
                        continue;
                    }
                    xrefTab.add(i, entry);
                }
                if(!Utils.byteIsASCIIDigit(span[0])) //A 'normal' xref Part consists of only Integers and White Spaces, and is always followed by the 'trailer' Token
                {
                    break;
                }
            }
            Utils.skipPDFComment(ref span);                                         //Explicitly check for Comments between xref end and 'trailer' token
            if(!Utils.spanStartsWithGivenText(span,"trailer"))
                throw new System.Exception("Error in pdfParser.getXRefNormalPart(): No trailer found after xref table!");
            span = span.Slice(7);                                                   //move past 'trailer' token
            Utils.skipASCIIWhiteSpaces(ref span);                                   //move past potential white spaces
            pdfDictionary trailer = Utils.readPDFDictionary(ref span);
            if(rootObRef == null && trailer.containsKey("/Root"))       //Only the first Root found in the sequence of xref Parts is the valid one.
                                                                        //So rootObRef will is only be assigned when it is still null, i.e. no Root found before
            {
                rootObRef = xrefTab.getFinalRef((pdfObjectReference)trailer.get("/Root"));
            }
            if(infoObRef == null && trailer.containsKey("/Info"))       //Also with Info, only the first found occurence is valid.
            {
                infoObRef = xrefTab.getFinalRef((pdfObjectReference)trailer.get("/Info"));
            }
            if(trailer.containsKey("/Prev"))    //If /Prev is found, that marks the Byte offset of the next xref Part. Otherwhise, this was the last xref Part.
            {
                pdfInteger newstartPDFInt = (pdfInteger)xrefTab.getNonRef(trailer.get("/Prev"));
                int newstartxref = newstartPDFInt.value;
                getXRefPart(newstartxref);
            }
        }

        void getXRefCompressedPart(int startxref)
        //Adds a compressed Part of the xref Table (stored in a pdf-Stream) to the xrefTable Object
        {
            ByteSpan span = new ByteSpan(arrAllBytes,startxref,numAllBytes-startxref);
            Utils.readPDFObjectHeader(ref span);
            pdfStream xrefStream = (pdfStream)xrefTab.getNonRef(Utils.readPDFEntity(ref span, xrefTab));
            pdfDictionary trailer = xrefStream.dictionary;
            ByteSpan data = new ByteSpan(xrefStream.getUnfilteredData(xrefTab));
            System.Collections.Generic.List<int> lstIndex = new System.Collections.Generic.List<int>(); 
            if(trailer.containsKey("/Index"))   //The pdfArray stored with key '/Index' will contain a list of xref-Block-Headers (i.e. initial Object index + number of following Entries )
                                                //just like a normal xref may have several Blocks with Headers
            {
                pdfArray arrIndex = (pdfArray)trailer.getResolved("/Index", xrefTab); //sending xrefTab as secundary argument to resolve pdfEntity if necessary
                for(int i=0; i<arrIndex.count(); i++)
                {
                    lstIndex.Add(((pdfInteger)arrIndex.getResolved(i, xrefTab)).value);
                }
            }
            else                                //If no pdfArray with key '/Index' exists, then there is only one xref-Block stored,
                                                //starting with pdfObject-Index 0 and containing '/Size' many Entries. '/Size' must always be present.
            {
                pdfInteger size = (pdfInteger)trailer.getResolved("/Size",xrefTab);
                lstIndex.AddRange( new int[] {0, size.value});
            }
            if(lstIndex.Count % 2 != 0)
                throw new System.Exception("Error in pdfParser.getXRefCompressedPart(): index array has uneven number of elements!");

            pdfArray arrByteWidths = (pdfArray)trailer.getResolved("/W",xrefTab);
            int wUseStatus = ((pdfInteger)arrByteWidths.getResolved(0,xrefTab)).value;
            int wOffset = ((pdfInteger)arrByteWidths.getResolved(1,xrefTab)).value;
            int wGeneration = ((pdfInteger)arrByteWidths.getResolved(2,xrefTab)).value;
            int wEntry = wUseStatus + wOffset + wGeneration;
            int k=0; //k iterates all object Entries in the xref (like all Rows in a normal xref)
            for(int i=0; i<lstIndex.Count; i+=2)    //i interates all xref-Blocks
            {
                int firstIndex = lstIndex[i];
                int numObs = lstIndex[i+1];
                for(int index = firstIndex; index < firstIndex + numObs; index++) //index iterates all indices in one given xref Block
                {
                    if(xrefTab.containsKey(index))  //If an object index appears more than once (which happens for some malformed PDFs), only the xref Entry of that index's first appearence will be used
                    {
                        System.Console.WriteLine("Warning in pdfParser.getXRefNormalPart(): Found > 1 Entry with same index in xref Table! -> Only using first occurence!");
                        k++;
                        continue;
                    }
                    int useStatus = (int)Utils.bytesToBigEndianInteger(data.Slice(k*wEntry,wUseStatus),wUseStatus);
                    int offset = (int)Utils.bytesToBigEndianInteger(data.Slice(k*wEntry+wUseStatus,wOffset),wOffset);
                    if(index!=0 && offset == 0) //Some malformed pdfs have objects in xref table with offset 0, which is not valid for index != 0
                    {
                        System.Console.WriteLine("Warning in pdfParser.getXRefNormalPart(): Found Entry with Offest = 0 but Index != 0 in xref Table! -> Ignoring!");
                        k++;
                        continue;
                    }
                    //below: Sometimes the Byte Width for generation values is 0, which just means that all generation valus are = 0
                    int generation = (wGeneration == 0)? 0: (int)Utils.bytesToBigEndianInteger(data.Slice(k*wEntry+wUseStatus+wOffset,wGeneration),wGeneration);
                    xrefEntry entry = new xrefEntry(index,generation,useStatus,offset);
                    xrefTab.add(index,entry);
                    k++;
                }
            }
            if(rootObRef == null && trailer.containsKey("/Root")) //Only the first Root found in the sequence of xref Parts is the valid one.
                                                                  //So rootObRef will is only be assigned when it is still null, i.e. no Root found before
            {
                rootObRef = xrefTab.getFinalRef((pdfObjectReference)trailer.get("/Root"));
            }
            if(infoObRef == null && trailer.containsKey("/Info")) //Also with Info, only the first found occurence is valid.
            {
                infoObRef = xrefTab.getFinalRef((pdfObjectReference)trailer.get("/Info"));
            }
            if(trailer.containsKey("/Prev")) //If /Prev is found, that marks the Byte offset of the next xref Part. Otherwhise, this was the last xref Part.
            {
                pdfInteger newstartPDFInt = (pdfInteger)trailer.getResolved("/Prev",xrefTab);
                int newstartxref = newstartPDFInt.value;
                getXRefPart(newstartxref);
            }
        }

        void getXRefPart(int startxref)
        //The whole xref Table of a PDF may be split up into several Parts (usually due to socalled 'incremental updated' of the PDF)
        //Each Part of the whole xref Table may either be a 'normal' or a compressed xref Table.
        //This Function adds one such Part to the global xrefTable object
        {
            ByteSpan span = new ByteSpan(arrAllBytes, startxref, arrAllBytes.Length-startxref);
            Utils.skipASCIIWhiteSpaces(ref span);
            if(Utils.spanStartsWithGivenText(span, "xref"))                                         //Normal xref Parts are indicated by the 'xref' Token
            {
                getXRefNormalPart(startxref);
            }
            else if(Utils.spanStartsWithPDFObject(span))                                   //Compressed xref Parts are just PDF-Objects, usually pdfStreams.
            {
                getXRefCompressedPart(startxref);
            }
            else
            {
                throw new System.Exception("Error in pdfParser.getXRefPart(): neither 'xref' Token nor PDF-Object (i.e. compressed XRef) found after startxref Offset!");
            }
        }

        private void getXRefTable()
        //This is the pdfParser's entry function to read the PDF's xref and store it in an xrefTable object
        {
            int  startxref = findStartXRef();
            getXRefPart(startxref);
        }
    }
}