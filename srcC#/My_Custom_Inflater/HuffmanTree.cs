using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace pdfParserByMH
{
    public class HuffmanTree
    {
        private System.Collections.Generic.Dictionary<int,System.Collections.Generic.Dictionary<uint,int>> dictOfCLs;

        public HuffmanTree()
        {
            dictOfCLs = new System.Collections.Generic.Dictionary<int,System.Collections.Generic.Dictionary<uint,int>>();
        }

        public void add(int codeLength, uint code, int symbol)
        {
            if(dictOfCLs.ContainsKey(codeLength))
            {
                dictOfCLs[codeLength].Add(code,symbol);
            }
            else
            {
                System.Collections.Generic.Dictionary<uint,int> newDict = new System.Collections.Generic.Dictionary<uint,int>();
                newDict.Add(code,symbol);
                dictOfCLs.Add(codeLength,newDict);
            }
        }

        public int getSymbol(int codeLength, uint code)
        {
            return dictOfCLs[codeLength][code];
        }

        public int[] codeLengths()
        {
            int[] cls = dictOfCLs.Keys.ToArray();
            System.Array.Sort(cls);
            return cls;
        }

        public string[] codes(int codeLength)
        {

            uint[] cds = dictOfCLs[codeLength].Keys.ToArray();
            string[] cdsBin = new string[cds.Length];
            for(int i=0; i<cds.Length; i++)
            {
                cdsBin[i] = System.Convert.ToString(cds[i],2);
            }
            return cdsBin;
        }

        public bool containsCode(int codeLength, uint code)
        {
            if(!dictOfCLs.ContainsKey(codeLength))
            {
                return false;
            }
            return dictOfCLs[codeLength].ContainsKey(code);
        }
    }
}