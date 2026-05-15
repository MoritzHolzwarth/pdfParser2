using System.Linq;

namespace pdfParserByMH
{
    public class pdfDictionary : pdfEntity
    {
        private System.Collections.Generic.Dictionary<string,pdfEntity> dict; //This contains the true (immideate) key-value pairs
        private System.Collections.Generic.Dictionary<string,pdfEntity> resolvedDict; //.. wile in this, all values that are pdfObRefs are replaced by the non-ObRef pdfEntity that the obRef points to
        //Note: in pdfArray, the list resolvedItems is only of use when it is complete, due to the order of Array-Items.
        //But here we can in principle make also use of a only partiall resolvedDict, because only the presence of a resolved item matters, not the position.
        public pdfDictionary()
        {
            dict = new System.Collections.Generic.Dictionary<string,pdfEntity>();
            resolvedDict = new System.Collections.Generic.Dictionary<string,pdfEntity>();
        }
        public pdfEntity get(string str)
        {
            if(!dict.ContainsKey(str))
                //throw new System.Exception($"Error in pdfDictionary.get(): key {str} does not exist!"); > C# 5
                throw new System.Exception(string.Format("Error in pdfDictionary.get(): key {0} does not exist!", str));
            return dict[str];
        }
        public void add(string str, pdfEntity val)
        {
            dict.Add(str,val);
            if(!(val is pdfObjectReference))
                resolvedDict.Add(str,val);
        }

        public void add(string str, pdfEntity val, xrefTable xrefTab)
        {
            dict.Add(str,val);
            if(!(val is pdfObjectReference))
                resolvedDict.Add(str,val);
            else
                resolvedDict.Add(str,xrefTab.getNonRef(val));
        }

        public void add(string str, pdfEntity val, xrefCollection xrefCol)
        {
            dict.Add(str,val);
            if(!(val is pdfObjectReference))
                resolvedDict.Add(str,val);
            else
                resolvedDict.Add(str,xrefCol.getNonRef(val));
        }

        public void set(string str, pdfEntity val)
        {
            if(!dict.ContainsKey(str))
                throw new System.Exception("Error in pdfDictionary.set(): key doesn't exits!");
            dict[str] = val;
            if(!(val is pdfObjectReference))
                resolvedDict[str] = val;
        }

        public void set(string str, pdfEntity val, xrefCollection xrefCol)
        {
            if(!dict.ContainsKey(str))
                throw new System.Exception("Error in pdfDictionary.set(): key doesn't exits!");
            dict[str] = val;
            if(!(val is pdfObjectReference))
                resolvedDict[str] = val;
            else
                resolvedDict[str] = xrefCol.getNonRef(val);
        }

        public int count()
        {
            return dict.Count;
        }
        public bool containsKey(string str)
        {
            return dict.ContainsKey(str);
        }

        public bool containsKeyResolved(string str)
        {
            return resolvedDict.ContainsKey(str);
        }
        public string[] keys()
        {
            return dict.Keys.ToArray();
        }
        public void ensureResolvedDict(xrefCollection xrefCol) //analogous to pdfArray
        {
            if(isFullyResolved())
                return;
            resolvedDict = new System.Collections.Generic.Dictionary<string,pdfEntity>(dict.Count);
            foreach(string key in dict.Keys)
            {
                pdfEntity item = dict[key];
                if(item is pdfObjectReference)
                {
                    pdfEntity resolvedItem = xrefCol.getNonRef(item);
                    resolvedDict.Add(key,resolvedItem);
                }
                else
                {
                    resolvedDict.Add(key,item);
                }
            }
        }

        public void ensureResolvedDict(xrefTable xrefTab)
        {
            if(isFullyResolved())
                return;
            resolvedDict = new System.Collections.Generic.Dictionary<string,pdfEntity>(dict.Count);
            foreach(string key in dict.Keys)
            {
                pdfEntity item = dict[key];
                if(item is pdfObjectReference)
                {
                    pdfEntity resolvedItem = xrefTab.getNonRef(item);
                    resolvedDict.Add(key,resolvedItem);
                }
                else
                {
                    resolvedDict.Add(key,item);
                }
            }
        }

        public pdfEntity getResolved(string str, xrefTable xrefTab = null)
        {
            if(!dict.ContainsKey(str))
                //throw new System.Exception($"Error in pdfDictionary.getResolved(): key {str} does not exist!"); > C# 5
                throw new System.Exception(string.Format("Error in pdfDictionary.getResolved(): key {0} does not exist!", str));
            if(!(dict[str] is pdfObjectReference))
                return dict[str];
            if(!resolvedDict.ContainsKey(str))
            {
                if(xrefTab == null)
                    //throw new System.Exception($"Error in pdfDictionary.getResolved(): key {str} does not exist in resolvedDict and no xrefTab was provided!"); > C# 5
                    throw new System.Exception(string.Format("Error in pdfDictionary.getResolved(): key {0} does not exist in resolvedDict and no xrefTab was provided!", str));
                return xrefTab.getNonRef(dict[str]);
            }
            return resolvedDict[str];
        }

        public bool isFullyResolved()
        {
            return resolvedDict.Count == dict.Count;
        }
    }
}

