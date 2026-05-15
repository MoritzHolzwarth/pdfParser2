namespace pdfParserByMH
{
    public struct xrefEntry
    {
        public int index;
        public int generation;
        public int useStatus;
        public int offset;
        public xrefEntry(int newIndex = 0, int newGen = 0, int newUse = 0, int newOffset = 0)
        {
            index = newIndex;
            generation = newGen;
            useStatus = newUse;
            offset = newOffset;
        }
    }
}