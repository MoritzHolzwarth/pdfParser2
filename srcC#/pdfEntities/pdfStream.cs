using System.CodeDom;
using System.Linq;

namespace pdfParserByMH
{
    public class pdfStream : pdfEntity
    {
        public pdfDictionary dictionary {get;} //Every pdfStream consists of a pdfDictionary, followed by a binary, possible compressed data stream
        public byte[] data {get; private set;}
        private System.Collections.Generic.List<string> lstFilters; //The list of Filters that were used (in reverse order than in this list) to compress the data.
                                                                    //To uncompress the data, apply each inverse filter to the data, in the order of this list
        private System.Collections.Generic.List<pdfDictionary> lstDecodeParms; //Some Filters (like /FlateDecode) can depend on extra Parameters, which are given by a pdfdictionary
                                                                               //If one of the used Filters has a parameter-dictionary, then all Filters must have a param-dictionary
                                                                               //In that case, filters which usually dont allow extra paramenters just get an empty dictionary.
        private bool canReadWrite; //Checks if the Filter-Information is sufficient to read from, or write to the stream data (this matters when a pdfStream is extracted not completely at once, but incrementally from a PDF file).
        public pdfStream(pdfDictionary dictVal, byte[] dataval)
        {
            canReadWrite = false;
            data = dataval;
            dictionary = dictVal;
            lstFilters = new System.Collections.Generic.List<string>();
            lstDecodeParms = new System.Collections.Generic.List<pdfDictionary>();
        }

        public byte[] getUnfilteredData(xrefTable xrefTab = null)
        {
            if(!canReadWrite)
            {
                makeFilters(xrefTab);
                canReadWrite = true;
            }
            byte[] unfilteredData = data; //assigning the 'still filtered' data to the variable unfilteredData which will actually become unfiltered in the next steps
            if(unfilteredData.Length == 0)
                return unfilteredData;
            Decoder decoder = new Decoder();
            for(int i=0; i < lstFilters.Count; i++) //Applying all inverse Filters in sequence
            {
                decoder.arrBytes = unfilteredData;
                decoder.filter = lstFilters[i];
                decoder.decodeParms = lstDecodeParms[i];
                unfilteredData = decoder.decode();
            }
            return unfilteredData;
        }

        private void filterData(byte[] rawData, xrefTable xrefTab = null)
        {
            if(!canReadWrite)
            {
                makeFilters(xrefTab);
                canReadWrite = true;
            }
            byte[] filteredData = rawData; //rawData is unfiltered, filteredData will become filtered in the next steps
            Decoder decoder = new Decoder();
            for(int i=lstFilters.Count-1; i > -1; i--) //Applying all Filters in sequence (reverse order as given by the /Filter array)
            {
                decoder.arrBytes = filteredData;
                decoder.filter = lstFilters[i];
                decoder.decodeParms = lstDecodeParms[i];
                filteredData = decoder.encode();
            }
            data = filteredData; //Updating this pdfStream's data field to the new data
            pdfInteger length = (pdfInteger)dictionary.getResolved("/Length"); //This is a reference type, so I can change its properties ..
            length.value = data.Length; //updating the /Length entry in this pdfStream's dictionary
        }

        public void addDataToEnd(byte[] newData) //newData is expected to be unfiltered
        {
            byte[] unfilteredData = getUnfilteredData(); //get this pdfStream's aleady present data, unfilter it, ..
            byte[] extendedData = unfilteredData.Concat(newData).ToArray(); //.. and combine with the newData
            filterData(extendedData); //Then filter the whole, new unfiltered data and store it
        }

        public void addDataToBeginning(byte[] newData) //Analogous to above
        {
            byte[] unfilteredData = getUnfilteredData();
            byte[] extendedData = newData.Concat(unfilteredData).ToArray();
            filterData(extendedData);
        }

        public void setNewData(byte[] newData) //newData is expected to be filtered
        {
            filterData(newData);
        }

        private void makeFilters(xrefTable xrefTab = null)
        {
            if(!dictionary.containsKey("/Filter")) //Means the data is completely unfiltered/uncompressed -> lstFilters and lstDecodeParams can stay empty.
            {
                return;
            }
            pdfEntity filterEntity;
            if(dictionary.containsKeyResolved("/Filter"))
            {
                filterEntity = dictionary.getResolved("/Filter");
            }
            else if(xrefTab != null)
            {
                filterEntity = xrefTab.getNonRef(dictionary.get("/Filter"));
            }
            else
                throw new System.Exception("Error in pdfStream.makeFilters(): Neither does the pdfStream's dictionary provide the resolved /Filter, nor is an xrefTable given to resolve it!");
            
            if(filterEntity is pdfName pdfnam) //Only one Filter present without wrapper Array
            {
                lstFilters.Add((pdfnam).value);
            }
            else if(filterEntity is pdfArray arrFilters)
            {
                if(arrFilters.isFullyResolved())
                {
                    for(int i=0; i < arrFilters.count(); i++)
                    {
                        lstFilters.Add(((pdfName)arrFilters.getResolved(i)).value);
                    }
                }
                else if(xrefTab != null)
                {
                    for(int i=0; i < arrFilters.count(); i++)
                    {
                        pdfName filterNam = (pdfName)xrefTab.getNonRef(arrFilters.get(i));
                        lstFilters.Add(filterNam.value);
                    }
                }
                else
                    throw new System.Exception("Error in pdfStream.makeFilters(): Neither is the /Filter pdfArray resolved, nor is an xrefTable given to resolve it!");
            }
            else
                throw new System.Exception("Error in pdfStream.makeFilters(): /Filter Entity is neither pdfName nor pdfArray!");

            if(!dictionary.containsKey("/DecodeParms")) //If no /DecodeParams are present, just give every Filter an empty dictionary of decodeParams
            {
                for(int i=0; i < lstFilters.Count; i++)
                {
                    lstDecodeParms.Add(new pdfDictionary());
                }
                return;
            }
            pdfEntity decodeParmsEntity;
            if(dictionary.containsKeyResolved("/DecodeParms"))
            {
                decodeParmsEntity = dictionary.getResolved("/DecodeParms");
            }
            else if(xrefTab != null)
            {
                decodeParmsEntity = xrefTab.getNonRef(dictionary.get("/DecodeParms"));
            }
            else
                throw new System.Exception("Error in pdfStream.makeFilters(): Neither does the pdfStream's dictionary provide the resolved /DecodeParams, nor is an xrefTable given to resolve it!");

            if(decodeParmsEntity is pdfDictionary dictDecodeParms) //Only one decodeParams dict present, without wrapper array
            {
                if(!dictDecodeParms.isFullyResolved()) //At this point, the decodeParams dict is required to be fully resolved. If it isnt yet, it must be resolved now via the xrefTable
                {
                    if(xrefTab == null)
                        throw new System.Exception("Error in pdfStream.makeFilters(): decodeParms dictionary is not fully resolved and No xrefTable is provided!");
                    dictDecodeParms.ensureResolvedDict(xrefTab);
                }
                lstDecodeParms.Add(dictDecodeParms);
            }
            else if(decodeParmsEntity is pdfArray arrDecodeDicts) //If an array of decodeParam-dictionaries is provided, then that array as well as the dictionaries it contains must be fully resolved
            {
                if(arrDecodeDicts.isFullyResolved())
                {
                    for(int i=0; i < arrDecodeDicts.count(); i++)
                    {
                        dictDecodeParms = (pdfDictionary)arrDecodeDicts.getResolved(i); //each decodeParam-dictionary is then handeled analogous to above
                        if(!dictDecodeParms.isFullyResolved())
                        {
                            if(xrefTab == null)
                                throw new System.Exception("Error in pdfStream.makeFilters(): decodeParms dictionary is not fully resolved and No xrefTable is provided!");
                            dictDecodeParms.ensureResolvedDict(xrefTab);
                        }
                        lstDecodeParms.Add(dictDecodeParms);
                    }
                }
                else if(xrefTab != null)
                {
                    for(int i=0; i < arrDecodeDicts.count(); i++)
                    {
                        dictDecodeParms = (pdfDictionary)xrefTab.getNonRef(arrDecodeDicts.get(i));
                        if(!dictDecodeParms.isFullyResolved())
                        {
                            if(xrefTab == null)
                                throw new System.Exception("Error in pdfStream.makeFilters(): decodeParms dictionary is not fully resolved and No xrefTable is provided!");
                            dictDecodeParms.ensureResolvedDict(xrefTab);
                        }
                        lstDecodeParms.Add(dictDecodeParms);
                    }
                }
                else
                    throw new System.Exception("Error in pdfStream.makeFilters(): cant resolve /DecodeParms Array without xrefTab!");
            }
            else
                throw new System.Exception("Error in pdfStream.makeFilters(): /DecodeParms Entity is neither pdfName nor pdfArray!");
            if(lstFilters.Count != lstDecodeParms.Count)
                throw new System.Exception("Error in pdfStream.makeFilters(): numbers of Filters and DecodeParms dont match!");
        }
    }
}