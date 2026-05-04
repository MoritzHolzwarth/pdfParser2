using System;
using System.CodeDom;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Linq.Expressions;
using System.Resources;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace pdfParserByMH
{
    public partial class Inflater
    {
        private HuffmanTree fixedLitLenHuffTree;
        private HuffmanTree fixedDistHuffTree;
        private int[] arrCLSymbolsForDynHuff = new int[] {16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15};
        
        private byte[] deflatedData;
        private int numCompressedData;
        private int numBytesLeft;
        private System.Collections.Generic.List<byte> inflatedData;
        private int numBitsRead;
        private uint bitBuffer;
        private int bitCount;

        public Inflater(byte[] data = null)
        {
            if(data == null)
            {
                setData(new byte[0]);
            }
            else
            {
                setData(data);
            }
            makeFixedHuffTrees();
        }


        public void setData(byte[] data)
        {
            deflatedData = data;
            numCompressedData = deflatedData.Length;
            resetBits();
        }
        
        private void makeFixedHuffTrees()
        {
            fixedLitLenHuffTree = new HuffmanTree();
            fixedDistHuffTree = new HuffmanTree();
            int[] arrLitLenSymbolIntervalls = new int[8] {256, 279, 0, 143, 280, 287, 144, 255};
            int[] arrLitLenCodeLengths = new int [4] {7,8,8,9};
            uint code = 0;
            int prevCL = arrLitLenCodeLengths[0];
            for(int i=0; i<arrLitLenCodeLengths.Length; i++)
            {
                int cl = arrLitLenCodeLengths[i];
                if(cl > prevCL)
                {   //in this case, the current code has either length prevCL or length prevCL+1
                    //The number of significant bits will be prevCL+1 Iff the current code has length prevCL+1
                    int numSigBits = Utils.numSigBits((int)code);
                    if(numSigBits < prevCL+1) //If the code still has length prevCL, we shift it all the way to the new code length
                    {
                        code <<= cl - prevCL;
                    }
                    else //If the last code-increment has already made the code one bit longer, we must shift it one bit less
                    {
                        code <<= cl - numSigBits;
                    }
                    prevCL = cl;
                }
                int firstLitLenSymbol = arrLitLenSymbolIntervalls[2*i];
                int lastLitLenSymbol = arrLitLenSymbolIntervalls[2*i+1];
                for(int symbol = firstLitLenSymbol; symbol <= lastLitLenSymbol; symbol++)
                {
                    fixedLitLenHuffTree.add(cl,Utils.reverseBits(code,cl),symbol);
                    code++;
                }
            }

            //All distance codes have length 5, so there are 2^5 = 32 distance codes
            code = 0;
            for(int symbol = 0; symbol <= 31; symbol++)
            {
                fixedDistHuffTree.add(5,Utils.reverseBits(code,5),symbol);
                code++;
            }
        }

        public byte[] inflate_normal()
        {
            while(numBytesLeft > 0)
            {
                
                uint BFINAL = readBits(1);
                uint BTYPE = readBits(2);
                if(BTYPE == 0)
                {
                    readUncompressedBlock();
                }
                else if(BTYPE == 1)
                {
                    readFixedHuffmanBlock();
                }
                else if(BTYPE == 2)
                {
                    readDynamicHuffmanBlock();
                }
                else
                {
                    throw new System.Exception($"Error in Inflater.inflate(): invalid BTYPE value {BTYPE}!");
                }
                if(BFINAL == 1)
                {
                    break;
                }
            }
            return inflatedData.ToArray();
        }
        
        private void readUncompressedBlock()
        {
            skipToNextByte();
            ushort len = (ushort)readBits(16); //ushort is 16 bit unsigned integer
            uint nlen = (ushort)readBits(16); 
            if(nlen != ~len)                    // ~ is bitwise 'not'
                throw new System.Exception("Error in Inflater.readUncompressedBlock(): LEN and NLEN are not compatible!");
            
            byte[] uncompressedData = new byte[len];
            System.Array.Copy(deflatedData, numCompressedData-numBytesLeft, uncompressedData, 0, len);
            inflatedData.AddRange(uncompressedData);
            skipBytes(len);
        }

        void readFixedHuffmanBlock()
        {
            readHuffmanBlock(fixedLitLenHuffTree, fixedDistHuffTree);
        }

        void readDynamicHuffmanBlock()
        {
            uint HLIT = readBits(5); //Number of Literal/Length codes - 257
            uint HDIST = readBits(5); //Number of Distance codes - 1
            uint HCLEN = readBits(4); //Number of Code Length Codes - 4
            HuffmanTree clHuffTree = makeCLHuffTree(HCLEN+4);
            HuffmanTree litlenHuffTree = makeLitLenOrDistHuffTree(HLIT+257, clHuffTree, true);
            HuffmanTree distHuffTree = makeLitLenOrDistHuffTree(HDIST+1, clHuffTree, false);
            readHuffmanBlock(litlenHuffTree, distHuffTree);
        }

        private void readHuffmanBlock(HuffmanTree litlenHuffTree, HuffmanTree distHuffTree)
        {
            bool foundEOB = false;
            while(numBytesLeft*8 +bitCount > 0)
            {
                int litlenCodeLength = -1;
                uint litlenCode = 0;
                foreach(int codeLength in litlenHuffTree.codeLengths())
                {
                    litlenCode = readBits(codeLength, false); //false <=> read without moving forward
                    if(litlenHuffTree.containsCode(codeLength,litlenCode))
                    {   
                        litlenCodeLength = codeLength;
                        readBits(codeLength); //In order to move forward
                        break;
                    }
                }
                if(litlenCodeLength == -1)
                    throw new System.Exception("Error in Inflater.readFixedHuffmanBlock(): no Lit/Len Code found!");
                    
                int litlenSymbol = litlenHuffTree.getSymbol(litlenCodeLength,litlenCode);
                if(litlenSymbol == 256) //256 = End Of Block
                {
                    foundEOB = true;
                    break;
                }
                if(litlenSymbol < 256)
                {
                    inflatedData.Add((byte)litlenSymbol);
                }
                else
                {
                    int lenValue = getLengthValue(litlenSymbol);

                    int distCodeLength = -1;
                    uint distCode = 0;
                    foreach(int cl in distHuffTree.codeLengths())
                    {
                        distCode = readBits(cl, false); //false <=> read without moving forward
                        if(distHuffTree.containsCode(cl,distCode))
                        {   
                            distCodeLength = cl;
                            readBits(cl); //In order to move forward
                            break;
                        }
                    }
                    if(distCodeLength == -1)
                        throw new System.Exception("Error in Inflater.readFixedHuffmanBlock(): no Distance Code found!");

                    int distSymbol = distHuffTree.getSymbol(distCodeLength,distCode);
                    int distvalue = getDistanceValue(distSymbol);
                    int currentInflatePosition = inflatedData.Count;
                    for(int i=0; i < lenValue; i++)
                    {
                        byte newByte = inflatedData[currentInflatePosition - distvalue + i];
                        inflatedData.Add(newByte);
                    }
                }
            }
            if(!foundEOB)
                throw new System.Exception("Error in Inflater.readHuffmanBlock(): no End-Of-Block token found!");
        }

        private HuffmanTree makeLitLenOrDistHuffTree(uint numExpectedCLs, HuffmanTree clHuffTree, bool litlen)
        {
            int numTotalSymbols = litlen ? 288 : 32;

            int[] arrCLs = new int[numTotalSymbols];
            int numFoundCLs = 0;
            while(numFoundCLs < numExpectedCLs)
            {
                int foundCLCL = -1;
                uint foundCLCode = 0;
                foreach(int clcl in clHuffTree.codeLengths())
                {
                    foundCLCode = readBits(clcl, false); //read without moving forward
                    if(clHuffTree.containsCode(clcl,foundCLCode))
                    {
                        foundCLCL = clcl;
                        readBits(clcl); //move forward
                        break;
                    }
                } 
                if(foundCLCL == -1)
                    throw new System.Exception("Error in Inflater.makeLitLenOrDistHuffTree(): no matching Cl-code found!");
                
                int clSymbol = clHuffTree.getSymbol(foundCLCL,foundCLCode);
                numFoundCLs += processCLSymbol(clSymbol, ref arrCLs, numFoundCLs);
            }
            if(numFoundCLs != numExpectedCLs)
                throw new System.Exception($"Error in Inflater.makeLitlenOrDistanceHuffTree(): number of nonzero CLs are not matching ({numFoundCLs} and {numExpectedCLs})!");

            int[] arrSymbols = System.Linq.Enumerable.Range(0,numTotalSymbols).ToArray();
            int[] arrOrderedSymbols;
            int[] arrOrderedCLs;
            (arrOrderedSymbols, arrOrderedCLs) = orderSymbolsAndCLs(arrSymbols,arrCLs);

            HuffmanTree huffTree = new HuffmanTree();
            uint code = 0;
            int prevCL = arrOrderedCLs[0];
            for(int i=0; i < arrOrderedCLs.Length; i++)
            {
                int cl = arrOrderedCLs[i];
                if(cl > prevCL)
                {
                    int numSigBits = Utils.numSigBits((int)code);
                    if(numSigBits < prevCL+1)
                    {
                        code <<= cl - prevCL;
                    }
                    else 
                    {
                        code <<= cl - numSigBits;
                    }
                    prevCL = cl;
                }
                int symbol = arrOrderedSymbols[i];
                huffTree.add(cl,Utils.reverseBits(code,cl),symbol);
                code++;
            }
            return huffTree;
        }

        public int processCLSymbol(int clSymbol, ref int[]arrCLs, int numFoundCLs)
        {
            if(clSymbol < 0 || clSymbol > 18)
                throw new System.Exception($"Error in Inflater.processCLSymbol(): invalid CL-Symbol {clSymbol}!");
            
            if(clSymbol <= 15)
            {
                arrCLs[numFoundCLs] = clSymbol;
                return 1; //found 1 CL
            }
            if(clSymbol == 16)
            {
                int lastCL = arrCLs[numFoundCLs-1];
                int numCopiesOfLastCL = 3 + (int)readBits(2);
                for(int i=0; i<numCopiesOfLastCL; i++)
                {
                    arrCLs[numFoundCLs+i] = lastCL;
                }
                return numCopiesOfLastCL;
            }
            if(clSymbol == 17)
            {
                int numZeros = 3 + (int)readBits(3);
                for(int i=0; i<numZeros; i++)
                {
                    arrCLs[numFoundCLs+i] = 0;
                }
                return numZeros;
            }
            else //clSymbol == 18
            {
                int numZeros = 11 + (int)readBits(7);
                for(int i=0; i<numZeros; i++)
                {
                    arrCLs[numFoundCLs+i] = 0;
                }
                return numZeros;
            }
        }


        private HuffmanTree makeCLHuffTree(uint numCLCodes)
        {
            if(numCLCodes > 19 || numCLCodes < 0)
                throw new System.Exception($"Error in Inflater.makeCLHuffTree(): Invalid number of CL codes {numCLCodes}!");

            int[] arrCLCLs = new int[19];
            for(int i=0; i<numCLCodes; i++)
            {
                arrCLCLs[i] = (int)readBits(3);
            }
            if(numCLCodes < 19)
            {
                for(int i=(int)numCLCodes; i<19; i++)
                {
                    arrCLCLs[i] = 0;
                }
            }

            int[] arrOrderedSymbols;
            int[] arrOrderedCLs;
            (arrOrderedSymbols, arrOrderedCLs) = orderSymbolsAndCLs(arrCLSymbolsForDynHuff,arrCLCLs);

            HuffmanTree clHuffTree = new HuffmanTree();
            uint code = 0;
            int prevCL = arrOrderedCLs[0];
            for(int i=0; i<arrOrderedCLs.Length; i++)
            {
                int cl = arrOrderedCLs[i];
                if(cl > prevCL)
                {
                    int numSigBits = Utils.numSigBits((int)code);
                    if(numSigBits < prevCL+1)
                    {
                        code <<= cl - prevCL;
                    }
                    else 
                    {
                        code <<= cl - numSigBits;
                    }
                    prevCL = cl;
                }
                int symbol = arrOrderedSymbols[i];
                clHuffTree.add(cl,Utils.reverseBits(code,cl),symbol);
                code++;
            }
            return clHuffTree;
        }

        private (int[],int[]) orderSymbolsAndCLs(int[] arrSymbols, int[] arrCLs)
        {
            if(arrSymbols.Length != arrCLs.Length)
                throw new System.Exception("Error in Inflater.orderSymbolsAndCLs(): unequal numbers of Symbols and CLs!");

            System.Collections.Generic.List<int> lstNonzeroSymbols = new System.Collections.Generic.List<int>();
            System.Collections.Generic.List<int> lstNonzeroCLs = new System.Collections.Generic.List<int>();
            for(int i=0; i<arrSymbols.Length; i++)
            {
                if(arrCLs[i] > 0)
                {
                    lstNonzeroCLs.Add(arrCLs[i]);
                    lstNonzeroSymbols.Add(arrSymbols[i]);
                }
            }
            System.Collections.Generic.List<int> lstOrderedSymbols = new System.Collections.Generic.List<int>();
            System.Collections.Generic.List<int> lstOrderedCLs = new System.Collections.Generic.List<int>();
            System.Collections.Generic.List<int> tempList = new System.Collections.Generic.List<int>();
            while(lstNonzeroCLs.Count > 0)
            {
                int minCL = lstNonzeroCLs.Min();
                tempList.Clear();
                int i=0;

                while(i < lstNonzeroSymbols.Count)
                {
                    if(lstNonzeroCLs[i] == minCL)
                    {
                        tempList.Add(lstNonzeroSymbols[i]); //Filling Symbols of given CL in tempList ...
                        lstOrderedCLs.Add(minCL);
                        lstNonzeroCLs.RemoveAt(i);
                        lstNonzeroSymbols.RemoveAt(i);
                    }
                    else 
                    {
                        i++;
                    }
                }
                tempList.Sort(); //... then sorting tempList numerically...
                lstOrderedSymbols.AddRange(tempList); //.. and using this as final symbol order
            }
            return (lstOrderedSymbols.ToArray(), lstOrderedCLs.ToArray());
        }

        private int getLengthValue(int lengthSymbol)
        {
            if(lengthSymbol < 257 || lengthSymbol > 288)
                throw new System.Exception($"Error in Inflate.readLengthValue(): invalid Length Symbol {lengthSymbol}!");
            if(lengthSymbol < 265)
            {
                return lengthSymbol - 254;
            }
            int initSmallValue = 11;
            int initSymbol = 265;
            int numBits = 1;
            for(int i=0; i < 5; i++)
            {
                if(lengthSymbol < initSymbol+4)
                {
                    int smallValue = initSmallValue + (int)System.Math.Pow(2,numBits)*(lengthSymbol - initSymbol);
                    return smallValue + (int)readBits(numBits);
                }
                initSmallValue += 4*(int)System.Math.Pow(2,numBits);
                initSymbol += 4;
                numBits++;
            }
            if(lengthSymbol == 285)
            {
                return 258;
            }
            throw new System.Exception($"Error in Inflate.readLengthValue(): the length Symbol {lengthSymbol} exists, but is not meant to ever be used!");
        }

        private int getDistanceValue(int distSymbol)
        {
            if(distSymbol < 0 || distSymbol > 31)
                throw new System.Exception($"Error in Inflator.getDistanceValue(): invalid Distance Symbol {distSymbol}");
            
            if(distSymbol <=3)
            {
                return distSymbol + 1;
            }
            int initSmallValue = 5;
            int initSymbol = 4;
            int numBits = 1;
            for(int i=0; i < 13; i++)
            {
                if(distSymbol < initSymbol + 2)
                {
                    int smallValue = initSmallValue + (int)System.Math.Pow(2,numBits)*(distSymbol - initSymbol);
                    return smallValue + (int)readBits(numBits);
                }
                initSmallValue += 2*(int)System.Math.Pow(2,i+1);
                initSymbol += 2;
                numBits++;
            }
            throw new System.Exception($"Error in Inflator.getDistanceValue(): the distance Symbol {distSymbol} exists but is not meant to ever be used!");
        }
    }
}

