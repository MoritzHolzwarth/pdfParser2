using System;
using System.Runtime.InteropServices;

namespace pdfParserByMH
{
    public partial class Inflater
    {
        private uint readBits(int num, bool moveForward = true)
        {
            if(num <= 0)
                throw new System.Exception("Error in Inflater.readBita(): Cannot read <= 0 Bits!");
            if(num > 32)
                throw new System.Exception("Error in Inflater.readBits(): Cannot read more than 32 Bits!");
            if(num > bitCount)
                throw new System.Exception("Error in Inflater.readBits(): Not enough Bits in bitBuffer!");
            uint mask = (1u << num) - 1; // 1u << num would be the numnber 2^num having one 1 at position num and 0 elsewhere.
                                       // 2^num - 1 has 1s at all positions below num.
            uint result = bitBuffer & mask;
            if(!moveForward)
            {
                return result;
            }
            bitBuffer >>= num;
            numBitsRead += num;
            bitCount -= num;

            if(bitCount <= 24 && numBytesLeft > 0)
            {
                int numBytesToFillInBuffer = System.Math.Min((32 - bitCount) / 8, numBytesLeft);
                for(int i=0; i<numBytesToFillInBuffer; i++)
                {
                    int dataPos = numCompressedData - numBytesLeft;
                    bitBuffer |= (uint)deflatedData[dataPos] << bitCount;
                    numBytesLeft--;
                    bitCount += 8;
                }
            }

            return result;
        }

        private void resetBits(int entryByteIndex = 0)
        {
            numBytesLeft = numCompressedData;
            inflatedData = new System.Collections.Generic.List<byte>(deflatedData.Length);
            bitBuffer = 0;
            bitCount = 0;
            int numBytesToFillInBuffer = System.Math.Min(numBytesLeft,4);
            for(int i=0; i<numBytesToFillInBuffer; i++)
            {
                bitBuffer |= (uint)deflatedData[i] << bitCount;
                numBytesLeft--;
                bitCount += 8;
            }
            skipBytes(entryByteIndex);
        }

        private void skipToNextByte()
        {
            int extraBits = bitCount % 8;
            if(extraBits > 0)
            {
                readBits(extraBits);
            }
        }

        private void skipBytes(int num) //number of Bytes to skip refers to the container of buffer and compressedData (above numBytesLeft) combined
        {
            skipToNextByte();
            if(num == 0)
            {
                return;
            }
            if(num > numBytesLeft + (bitCount / 8)) //Total number of available Bytes is the number of Bytes in the buffer + numBytesLeft in compressedData
                throw new System.Exception("Error in Inflater.skipBytes(): Cannot skip more Bytes than Bytes are left in data!");
            
            if(num <= 4) //If less than 5 Bytes shall be skipped, we can just read them from the buffer.
            {
                readBits(num*8);
                return;
            }
            //Otherwise, we flush the buffer and refill it with Bytes from 'num' positions behind the current buffer start
            bitBuffer = 0;
            bitCount = 0;
            numBitsRead += num*8;
            numBytesLeft -= num - 4;
            int numBytesToFillInBuffer = System.Math.Min(numBytesLeft,4);
            for(int i=0; i<numBytesToFillInBuffer; i++)
            {
                int dataPos = numCompressedData - numBytesLeft;
                bitBuffer |= (uint)deflatedData[dataPos] << bitCount;
                numBytesLeft--;
                bitCount += 8;
            }
        }
    }
}