namespace pdfParserByMH
{
    public sealed partial class pdfDocument
    {
        public bool write(bool compress = false)
        {
            System.Console.Error.WriteLine("pdfDocument started writing");
            if(compress)
                throw new System.Exception("Error in pdfDocument.write(): option 'Compress' is not yet implemented!");
            else
                writeNormal();
            System.Console.Error.WriteLine("pdfDocument finished writing");
            return true;
        }
        private void writeNormal()
        {   
            if(outputPath == null)
            {
                throw new System.Exception("Error in pdfDocument.writeNormal(): outputPath is null!");
            }
            System.IO.FileStream fileStream = new System.IO.FileStream(outputPath,System.IO.FileMode.Create);
            fileStream.Write(Utils.stringToBytes("%PDF-1.6\n"),0,9);
            fileStream.Write(new byte[] {(byte)'%',(byte)'ä', (byte)'ö', (byte)'ü', 0x0A},0,5); //A bunch of random non-ASCII bytes at the top is just pdf-Standard
 
            int[] arrObjIndices = xref.keys();
            System.Array.Sort(arrObjIndices); //Lets write the objects in numerically sorted order, makes things easier
            int maxIndex = arrObjIndices[arrObjIndices.Length-1];
            byte[] endobjToken = getEndObjToken();
            //We will be writing the xref as one large, conventional Table of possible several blocks.
            System.Collections.Generic.Dictionary<int,xrefEntry> newXRefDict = new System.Collections.Generic.Dictionary<int,xrefEntry>();
            newXRefDict.Add(0,new xrefEntry(0,65535,0,0)); //The standart zero object of every pdf
            System.Collections.Generic.List<int[]> xrefBlockHeaders = new System.Collections.Generic.List<int[]>(); //each xref block, has a header, i.e. two numbers: 'first object index in block', 'number of objects in block'
                                                                                                                    //Note though: this list instread stored the number pairs: 'first object index in block', 'last object index in block'
            int latestIndex = 0; //Keeps track of the object that was last stored in the file (important for contiguous xref-Blocks)
            int[] firstAndLastIndex = new int[] {0,0};
            foreach(int index in arrObjIndices)
            {
                if(index > latestIndex + 1 || index == maxIndex) //'index > latestIndex + 1' means that the latest contiguous xref-Block is finished. Same for the special case that the maxIndex is reached.
                {                                                //Note: this strategy only works because the object indices are stored in a numerically sorted order!
                    firstAndLastIndex[1] = (index == maxIndex)? index: latestIndex; //In that case, close the latest xref-Block's header with the latestIndex (or with the current index in the special case of maxIndex (i.e. xref completely finished))
                    xrefBlockHeaders.Add(firstAndLastIndex); //Store the latest xref-Block-Header
                    firstAndLastIndex = new int[] {index, 0}; //Create a new xref-Block-Header from the current index (if index == maxIndex, this doenst matter)
                }
                latestIndex = index;
                int offset = (int)fileStream.Length; //The current size of the file will be the position of the next object
                int generation = 0; //We just give all objects generation 0, because generations are nowadays useless and only present for historical consistency
                byte[] objHeader = getObjectHeader(index, generation);
                fileStream.Write(objHeader,0,objHeader.Length); //write the object's header
                byte[] objBody = writePDFEntity(xref.get(index));
                fileStream.Write(objBody,0,objBody.Length); //write the object's body
                fileStream.Write(endobjToken,0,endobjToken.Length); //write the endobj token
                xrefEntry entry = new xrefEntry(index,generation,1,offset); //use status of all objects will be 1, except for the standart zero object
                newXRefDict.Add(index,entry); //store the newly written object in the new document's xref
            }
            int startxref = (int)fileStream.Length; //get the startxref position before writing the xref ..
            byte[] xrefAndTrailer = getXRefAndTrailerBytes(newXRefDict, xrefBlockHeaders.ToArray());
            fileStream.Write(xrefAndTrailer, 0, xrefAndTrailer.Length); //.. then write the xref
            //byte[] fileclosing = Utils.stringToBytes($"\nstartxref {startxref}\n%%EOF"); > C# 5 
            byte[] fileclosing = Utils.stringToBytes(string.Format("\nstartxref {0}\n%%EOF", startxref)); //then write the startxref and EOF
            fileStream.Write(fileclosing,0,fileclosing.Length);
            fileStream.Close();
        }

        private byte[] getObjectHeader(int index, int generation)
        {
            //just a standart, generic pdf-Object header
            System.Collections.Generic.List<byte> lst = new System.Collections.Generic.List<byte>(10);
            lst.AddRange(Utils.numberToASCIIBytes(index));
            lst.Add(0x20);
            lst.AddRange(Utils.numberToASCIIBytes(generation));
            lst.Add(0x20);
            lst.AddRange(Utils.stringToBytes("obj"));
            lst.Add(0x0A);
            return lst.ToArray();
        }

        private byte[] getEndObjToken()
        {
            //just the standart pdf-object endobj token
            System.Collections.Generic.List<byte> lst = new System.Collections.Generic.List<byte>(8);
            lst.Add(0x0A); //0x0A = LF (line feed)
            lst.AddRange(Utils.stringToBytes("endobj"));
            lst.Add(0x0A);
            return lst.ToArray();
        }

        private byte[] getXRefAndTrailerBytes(System.Collections.Generic.Dictionary<int,xrefEntry> xrefDict, int[][] xrefBlockHeaders)
        {
            //store a given xref-dictionary in an ASCII-byte-array
            System.Collections.Generic.List<byte> xrefTextBytes = new System.Collections.Generic.List<byte>();
            xrefTextBytes.AddRange(Utils.stringToBytes("xref\n")); //starting with the 'xref' token
            foreach(int[] firstAndLastIndex in xrefBlockHeaders)
            {
                int firstIndex = firstAndLastIndex[0];
                int latestIndex = firstAndLastIndex[1];
                //xrefTextBytes.AddRange(System.Text.Encoding.ASCII.GetBytes($"{firstIndex} {latestIndex-firstIndex+1}"));  > C# 5
                xrefTextBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(string.Format("{0} {1}", firstIndex, latestIndex-firstIndex+1))); //the block's header ('init index' and 'number of block-entries')
                xrefTextBytes.Add(0x0A);
                for(int i = firstIndex; i <= latestIndex; i++)
                {
                    xrefEntry entry = xrefDict[i];
                    char useStatusText = (entry.useStatus == 1)? 'n': 'f'; //we onyl consider normal xrefTables and no ObjStms, so useStatus = 2 is not possible
                    //string entryText = $"{entry.offset} {entry.generation} {useStatusText}"; > C# 5
                    string entryText = string.Format("{0} {1} {2}", entry.offset, entry.generation, useStatusText);
                    xrefTextBytes.AddRange(System.Text.Encoding.ASCII.GetBytes(entryText));
                    xrefTextBytes.Add(0x0A);
                }
            }
            xrefTextBytes.AddRange(Utils.stringToBytes("trailer\n")); //writing the trailer token after the xrefTable
            pdfDictionary trailer = new pdfDictionary(); //the trailer is just a pdfDictionary, which we can then write like any other pdfDictionary in the pdf's body
            trailer.add("/Root", rootObRef); //trailer needs a Root obRef..
            if(infoObRef != null)
                trailer.add("/Info",infoObRef);
            trailer.add("/Size",new pdfInteger(xrefDict.Count)); //.. and a /Size value
            xrefTextBytes.AddRange(writePDFDictionary(trailer));
            xrefTextBytes.Add(0x0A);
            return xrefTextBytes.ToArray();
        }
    }
}