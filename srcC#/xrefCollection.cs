using System;
using System.Linq;
namespace pdfParserByMH
{
    public class xrefCollection
    {
        private System.Collections.Generic.Dictionary<int,pdfEntity> dict;
        public xrefCollection()
        {
            dict = new System.Collections.Generic.Dictionary<int,pdfEntity>();
        }
        public void add(int key, pdfEntity entry)
        {
            dict.Add(key, entry);
        }
        public pdfEntity get(int key)
        {
            return dict[key];
        }
        public void set(int key, pdfEntity entry)
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
        public pdfEntity[] values()
        {
            return dict.Values.ToArray();
        }
        public int count()
        {
            return dict.Count;
        }
        public pdfEntity getNonRef(pdfEntity ent)
        {
            if(!(ent is pdfObjectReference))
            {
                return ent;
            }
            int key = ((pdfObjectReference)ent).index;
            int gen = ((pdfObjectReference)ent).generation;
            if(!dict.ContainsKey(key))
            {
                throw new Exception(string.Format("Error in xrefCollection.getNonRef(): initial was pdfObjectReference {0} {1} R, but it's index was not valid!", key, gen));
            }
            pdfEntity entity = dict[key];
            int safetyCounter = 0;
            while(entity is pdfObjectReference && safetyCounter < 1000)
            {
                safetyCounter++;
                key = ((pdfObjectReference)entity).index;
                if(!dict.ContainsKey(key))
                {
                    throw new Exception(string.Format("Error in xrefCollection.getNonRef(): intermediate pdfObjectReference {0} {1} R, but it's index was not valid!", key, gen));
                }
                entity = dict[key];
            }
            if(safetyCounter > 1000)
            {
                throw new System.Exception("Error in xreCollection.getNonRef(): while-loop exploded!");
            }
            return entity;
        }
        public pdfObjectReference getFinalRef(pdfObjectReference obRef)
        {
            if(!dict.ContainsKey(obRef.index))
                throw new System.Exception("Erron in xrefCollection.getFinalRef(): given obRef.index does not exist in xrefCol!");
            pdfEntity entity = dict[obRef.index];
            int safetyCounter = 0;
            while(entity is pdfObjectReference)
            {
                safetyCounter++;
                obRef = (pdfObjectReference)entity;
                if(!dict.ContainsKey(obRef.index))
                    throw new System.Exception("Erron in xrefCollection.getFinalRef(): given obRef.index does not exist in xrefCol!");
                entity = dict[obRef.index];
            }
            if(safetyCounter > 1000)
            {
                throw new System.Exception("Error in xrefTable.getFinalRef(): while-loop exploded!");
            }
            return obRef;
        }

        public int getMinimalFreeIndex()
        {
            int minPresentIndex = dict.Keys.Min();
            return (minPresentIndex > 1)? (minPresentIndex-1): (dict.Keys.Max()+1);
        }
    }
}