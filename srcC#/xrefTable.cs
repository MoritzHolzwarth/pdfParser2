using System.Linq;
using System.Security;
namespace pdfParserByMH
{
    public class xrefTable
    {
        private byte[] arrAllBytes;
        private System.Collections.Generic.Dictionary<int,xrefEntry> dict;
        public xrefTable(byte[] arr)
        {
            arrAllBytes = arr;
            dict = new System.Collections.Generic.Dictionary<int,xrefEntry>();
        }
        public void add(int key, xrefEntry entry)
        {
            dict.Add(key, entry);
        }
        public xrefEntry get(int key)
        {
            if(!dict.ContainsKey(key))
                throw new System.Exception("Error in xrefTable.get(): key doesn't exist!");
            return dict[key];
        }
        public void set(int key, xrefEntry entry)
        {
            if(dict.ContainsKey(key))
            {
                dict[key] = entry;
            }
            else
            {
                dict.Add(key,entry);
            }
        }
        public bool containsKey(int key)
        {
            return dict.ContainsKey(key);
        }
        public int[] keys()
        {
            return dict.Keys.ToArray();
        }
        public xrefEntry[] values()
        {
            return dict.Values.ToArray();
        }
        
        public pdfEntity getNonRef(pdfEntity ent)
        {
            ByteSpan spanAllBytes = new ByteSpan(arrAllBytes);
            int safetyCounter = 0;
            while(ent is pdfObjectReference && safetyCounter < 1000)
            {
                safetyCounter++;
                int key = ((pdfObjectReference)ent).index;
                ent = getPdfEntity(key);
            }
            if(safetyCounter > 1000)
            {
                throw new System.Exception("Error in xrefTable.getNonRef(): while-loop exploeded!");
            }
            return ent;
        }
        
        public pdfObjectReference getFinalRef(pdfObjectReference obRef)
        {
            if(!dict.ContainsKey(obRef.index)) //When calling this function during extraction of the xrefTable, we do generally not expect the xrefTable to be already completely filled
                                               //So it may be the case that here or below, the obRef references an object that is not yet known to the xrefTable                    
                                               //That is usually not an error, it just means that the given obRef is already the final one
                                               //Because an obRef can absolutely be encountered before its referenced object.
                return obRef;
            pdfEntity ent = getPdfEntity(obRef.index);
            int safetyCounter = 0;
            while(ent is pdfObjectReference)
            {
                safetyCounter++;
                obRef = (pdfObjectReference)ent;
                if(!dict.ContainsKey(obRef.index)) //Same check as above
                    return obRef;
                if(dict[obRef.index].useStatus == 2) //If the referenced object is in an ObjStm, we must explicitly check if that ObjStm does already exist in the xrefTable
                {                                    //This is realted to the unlikely case where a chain ob obRefs inculdes obRefs within an ObjStm
                    if(!dict.ContainsKey(dict[obRef.index].offset))
                        return obRef;
                }
                ent = getPdfEntity(obRef.index);
            }
            if(safetyCounter > 1000)
            {
                throw new System.Exception("Error in xrefTable.getFinalRef(): while-loop exploeded!");
            }
            return obRef;
        }

        private pdfEntity getPdfEntity(int index)
        {
            if(!dict.ContainsKey(index))
                throw new System.Exception("Error in xrefTable.getPFDEntity(): xrefTable does not contain index!");
            xrefEntry entry = dict[index];
            switch(entry.useStatus)
            {
                case 0:
                    throw new System.Exception("Error in xrefTable.getPDFEntity(): xrefEntry has useStatus 0!");
                case 1:
                    return getPDFEntityNormal(entry);
                case 2:
                    return getPDFEntityFromObjStream(entry);
                default:
                    //throw new System.Exception($"Error in xrefTable.getPDFEntity(): invalid useStatus {entry.useStatus}!");
                    throw new System.Exception(string.Format("Error in xrefTable.getPDFEntity(): invalid useStatus {0}!", entry.useStatus));
            }
        }

        private pdfEntity getPDFEntityFromObjStream(xrefEntry entry)
        {
            int objStmIndex = entry.offset;
            if(!dict.ContainsKey(objStmIndex))
                throw new System.Exception("Error in xrefTable.getPDFEntityFromObjStream(): xrefTable does not contain objStm index!");
            int objStmOffset = get(objStmIndex).offset;
            ByteSpan span = new ByteSpan(arrAllBytes).Slice(objStmOffset);
            Utils.readPDFObjectHeader(ref span);
            pdfStream objStm = (pdfStream)Utils.readPDFDictOrStream(ref span, this);
            pdfDictionary objStmDict = objStm.dictionary;
            ByteSpan objStmData1 = new ByteSpan(objStm.getUnfilteredData(this));
            ByteSpan objStmData2 = objStmData1;
            int firstObjOffset = ((pdfInteger)getNonRef(objStmDict.get("/First"))).value;
            int numObs = ((pdfInteger)getNonRef(objStmDict.get("/N"))).value;
            for(int i=0; i<numObs; i++)
            {
                int index = Utils.readASCIIInteger(ref objStmData1);
                int offset = firstObjOffset + Utils.readASCIIInteger(ref objStmData1);
                if(index == entry.index)
                {
                    span = objStmData2.Slice(offset);
                    Utils.skipASCIIWhiteSpaces(ref span);
                    pdfEntity entity = Utils.readPDFEntity(ref span, this);
                    return entity;
                }
            }
            throw new System.Exception("Error in xref.getEntityFromObjStream(): object index not found in ObjStm-Header");
        }

        private pdfEntity getPDFEntityNormal(xrefEntry entry)
        {
            int offset = entry.offset;
            ByteSpan span = new ByteSpan(arrAllBytes).Slice(offset);
            Utils.readPDFObjectHeader(ref span);
            return Utils.readPDFEntity(ref span, this);
        }
    }
}