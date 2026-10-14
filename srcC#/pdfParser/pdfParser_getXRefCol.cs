using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;

namespace pdfParserByMH
{
    public sealed partial class pdfParser
    {
        private void getXRefCollection()
        {
            System.Collections.Generic.List<xrefEntry> lstNormalEntries = new System.Collections.Generic.List<xrefEntry>(xrefTab.keys().Length);
            System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<int>> dictCompEntries = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<int>>();
            distributeXRefEntries(ref lstNormalEntries, ref dictCompEntries);
            getNormalPDFObjects(lstNormalEntries);
            getCompPDFObjects(dictCompEntries);
            resolveAllObjects(); 
        }
        private void getNormalPDFObjects(System.Collections.Generic.List<xrefEntry> lstNormalEntries)
        //Adding the 'normal' objects (which are not compressed inside an ObjStm) to the xrefCollection,
        //But only adding the 'usefull' normal objects. See below
        {
            ByteSpan span;
            foreach(xrefEntry entry in lstNormalEntries)
            {
                int offset = entry.offset;
                int index = entry.index;
                span = new ByteSpan(arrAllBytes, offset, numAllBytes-offset);
                Utils.readPDFObjectHeader(ref span);
                pdfEntity entity = Utils.readPDFEntity(ref span, xrefTab);
                if(!(entity is pdfStream))  //Any object that is not a pdfStream is added
                {
                    xrefCol.add(index,entity);
                    continue;
                }
                pdfDictionary dict = ((pdfStream)entity).dictionary;
                if(!dict.containsKey("/Type"))  //Any pdfStream that has no explicit '/Type' key is added
                {
                    xrefCol.add(index,entity);
                    continue;
                }
                string type = ((pdfName)dict.getResolved("/Type", xrefTab)).value;
                if(type != "/XRef" && type != "/ObjStm") //Only if a pdfStream's /Type is '/XRef' (meaning contains a compressed part of the PDF's xref),
                                                         //or '/ObjStm' (meanign it contains a bunch of compressed pdf Objects (which we handle extra via 'getCompPDFObjects')),
                                                         //then this stream has no use, since its information is already stored 'directly' in the xrefCollection.
                                                         //Storing such pdfStreams in the xrefCollections would produce mistakes when re-writing the PDF later.
                {
                    xrefCol.add(index,entity);
                    continue;
                }
            }
        }
        
        private void getCompPDFObjects(System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<int>> dictCompEntries)
        //Adding the compressed pdf-Objects to xrefCollection
        //For that, actually only the indices of the various ObjStm-objsts ara required (the keys of dictCompEntries).
        //But we also use the sets of object-indices expected to be found in any ObjStm (the values of dictCompEntries),
        //just for sanity checks.
        {
            foreach(int objStmIndex in dictCompEntries.Keys)
            {
                System.Collections.Generic.HashSet<int> setExpectedObjIndices = dictCompEntries[objStmIndex]; //The object-Indices which, in the xrefTable, were said to be in this ObjStm
                int objStmOffset = xrefTab.get(objStmIndex).offset;
                ByteSpan spanObjStm = new ByteSpan(arrAllBytes, objStmOffset, numAllBytes-objStmOffset);
                Utils.readPDFObjectHeader(ref spanObjStm);  //Move past object Header
                pdfStream objStm = (pdfStream)Utils.readPDFDictOrStream(ref spanObjStm, xrefTab); //Get the ObjStm
                pdfDictionary objStmDict = objStm.dictionary;
                int numObs = ((pdfInteger)objStmDict.getResolved("/N",xrefTab)).value; //Number of Objects the ObjStm itself claims to contain
                if(numObs != setExpectedObjIndices.Count)
                    System.Console.Error.WriteLine("Warning in pdfParser.getCompEntries(): Ambiguous Number of Objects!");
                int firstObjOffset = ((pdfInteger)objStmDict.getResolved("/First", xrefTab)).value; //The Offset of the first Object within the uncompressed ObjStm's data
                ByteSpan objStmData = new ByteSpan(objStm.getUnfilteredData(xrefTab)); //The ObjStm's whole uncompressed data
                ByteSpan objStmDataHeader = objStmData; //Use this to iterate over the sequence of object indices and offsets that make the ObjStm Data's Header
                ByteSpan objStmDataBody = objStmData;   //Use this to jump to any position within the ObjStm's Data
                for(int i=0; i<numObs; i++)
                {
                    int index = Utils.readASCIIInteger(ref objStmDataHeader);
                    if(!setExpectedObjIndices.Contains(index))  //In some malformed PDFs, the objects claimed by the xref to be in a certain ObjStm are 
                                                                //do not match the object indices actually found in that ObjStm.
                                                                //In that case we ignore any objects found in an ObjStm that were not expexted (from the xref) to be there
                    {
                        System.Console.WriteLine("Warning in pdfParser.getCompEntries(): Found object with unexpected index inside ObjStm! -> Ignoring this object.");
                        continue;
                    }
                    int offset = firstObjOffset + Utils.readASCIIInteger(ref objStmDataHeader);
                    objStmDataBody = objStmData.Slice(offset);
                    Utils.skipASCIIWhiteSpaces(ref objStmDataBody); //Skip possible White Spaces before the object (There should be no White Spaces, but just ot be sure)
                    pdfEntity entity = Utils.readPDFEntity(ref objStmDataBody, xrefTab);
                    xrefCol.add(index, entity);
                }
            }
        }

        private void distributeXRefEntries(ref System.Collections.Generic.List<xrefEntry> lstNormalEntries, ref System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<int>> dictCompEntries)
        {
            foreach(xrefEntry entry in xrefTab.values())
            {
                switch(entry.useStatus)
                {
                    case 0:
                        continue;
                    case 1:
                        lstNormalEntries.Add(entry);
                        break;
                    case 2:
                        int objStreamIndex = entry.offset;
                        if(dictCompEntries.ContainsKey(objStreamIndex))
                        {
                            dictCompEntries[objStreamIndex].Add(entry.index);
                        }
                        else
                        {
                            dictCompEntries.Add(objStreamIndex, new System.Collections.Generic.HashSet<int> {entry.index});
                        }
                        break;
                    default:
                        //throw new System.Exception($"Error in pdfParser.distributeXRefEntries(): unknown use Status {entry.useStatus}!"); > C# 5
                        throw new System.Exception(string.Format("Error in pdfParser.distributeXRefEntries(): unknown use Status {0}!", entry.useStatus));
                }
            }
        }

        private void resolveAllObjects()
        {
            System.Collections.Generic.Queue<pdfEntity> queue = new System.Collections.Generic.Queue<pdfEntity>(xrefCol.values());
            System.Collections.Generic.HashSet<pdfEntity> checkedEntities = new System.Collections.Generic.HashSet<pdfEntity>();
            while(queue.Count > 0)
            {
                pdfEntity entity = queue.Dequeue();
                if(!checkedEntities.Add(entity)) //Dont process any entity twice
                {
                    continue;
                }
                if(entity is pdfArray)
                {
                    pdfArray pdfarr = (pdfArray)entity;
                    if(!pdfarr.isFullyResolved())
                        pdfarr.ensureResolvedArray(xrefCol);
                    for(int i=0; i<pdfarr.count(); i++)
                    {
                        pdfEntity item = pdfarr.getResolved(i);
                        if(item is pdfArray || item is pdfDictionary || item is pdfStream)
                        {
                            queue.Enqueue(item);
                        }
                    }
                }
                else if(entity is pdfDictionary)
                {
                    pdfDictionary pdfdict = (pdfDictionary)entity;
                    if(!pdfdict.isFullyResolved())
                        pdfdict.ensureResolvedDict(xrefCol);
                    foreach(string key in pdfdict.keys())
                    {
                        pdfEntity value = pdfdict.getResolved(key);
                        if(value is pdfArray || value is pdfDictionary || value is pdfStream)
                        {
                            queue.Enqueue(value);
                        }
                    }
                }
                else if(entity is pdfStream)
                {
                    pdfStream pdfstream = (pdfStream)entity;
                    pdfDictionary streamdict = pdfstream.dictionary;
                    if(!streamdict.isFullyResolved())
                        streamdict.ensureResolvedDict(xrefCol);
                    foreach(string key in streamdict.keys())
                    {
                        pdfEntity value = streamdict.getResolved(key);
                        if(value is pdfArray || value is pdfDictionary || value is pdfStream)
                        {
                            queue.Enqueue(value);
                        }
                    }
                }
            }
        }
    }
}