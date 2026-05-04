using System.Drawing;

namespace pdfParserByMH
{
    public class pdfArray : pdfEntity
    {
        private System.Collections.Generic.List<pdfEntity> items; //This contains the true (immideate) array items ..
        private System.Collections.Generic.List<pdfEntity> resolvedItems; //.. wile in this, all items that are pdfObRefs are replaced by the non-ObRef pdfEntity that the obRef points to
        public pdfArray()
        {
                items = new System.Collections.Generic.List<pdfEntity>();
                resolvedItems = new System.Collections.Generic.List<pdfEntity>();
        }
        
        public pdfEntity get(int i)
        {
            if(i >= items.Count)
                throw new System.Exception("Error in pdfArray.get(): index out of Range!");
            return items[i];
        }
        public void InsertAtBeginning(pdfEntity val, xrefCollection xrefCol = null)
        {
            items.Insert(0, val);
            if(!(val is pdfObjectReference) && isFullyResolved()) //Easiest case: new item is already resolved (i.e. not an obRef) and this pdfArray is currently fully resolved:
            {
                resolvedItems.Insert(0, val);                     //-> Just add the new item to the resolved list directly.
            }
            else if(xrefCol != null)        //In any other case the xref will be needed for resolving, so we just resolve all
                ensureResolvedArray(xrefCol);
        }
        public void add(pdfEntity val, xrefCollection xrefCol = null)
        {
            items.Add(val);
            if(!(val is pdfObjectReference) && isFullyResolved())   //same as with InsertAtBeginning ...
            {
                resolvedItems.Add(val);
            }
            if(xrefCol != null)
                ensureResolvedArray(xrefCol);
        }
        public int count()
        {
            return items.Count;
        }

        public void ensureResolvedArray(xrefCollection xrefCol) //This function uses an xrefCollection (which is usually complete) for the Resolving ...
        {
            if(isFullyResolved())
                return;
            resolvedItems.Clear(); //Create a new list of resolved Items from scratch.
            for(int i=0; i < items.Count; i++)
            {
                pdfEntity item = items[i];
                if(item is pdfObjectReference)
                {
                    pdfEntity resolvedItem = xrefCol.getNonRef(item);
                    resolvedItems.Add(resolvedItem);
                }
                else
                {
                    resolvedItems.Add(item);
                }
            }
        }

        public void ensureResolvedArray(xrefTable xrefTab) //... while this function uses an xrefTable (which is not always complete but should be for this particular purpose) for Resolving.
        {
            if(isFullyResolved())
                return;
            resolvedItems.Clear();
            for(int i=0; i < items.Count; i++)
            {
                pdfEntity item = items[i];
                if(item is pdfObjectReference)
                {
                    pdfEntity resolvedItem = xrefTab.getNonRef(item);
                    resolvedItems.Add(resolvedItem);
                }
                else
                {
                    resolvedItems.Add(item);
                }
            }
        }

        public pdfEntity getResolved(int i, xrefTable xrefTab=null) //Note: if given, we only use the xrefTab to get one specific resolved item, not to resolve all of the array's items.
        {                                                           //That is because at the point this method is called, the xrefTable may be not yet complete!
            if(i >= items.Count)
                throw new System.Exception($"Error in pdfArray.getResolved(): index {i} out of Range!");
            if(!(items[i] is pdfObjectReference))
                return items[i];
            if(!isFullyResolved())
            {
                if(xrefTab == null)
                    throw new System.Exception("Error in pdfArray.getResolved(): this pdfArray is not fully resolved and no xrefTab was provided!");
                return xrefTab.getNonRef(items[i]);
            }
            return resolvedItems[i]; //Due to the Order of items, the lst resolvedItems can only be safely used if is is complete
        }

        public bool isFullyResolved()
        {
            return resolvedItems.Count == items.Count;
        }
    }
}