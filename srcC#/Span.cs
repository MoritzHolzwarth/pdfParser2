using System;
using System.CodeDom;
using System.IO;

namespace pdfParserByMH
{
    public sealed class ByteSpan
    {
        private byte[] Buffer;
        private int Offset;
        public int Length {get; private set;}

        public ByteSpan(byte[] arr, int offset, int length)
        {
            if(arr == null)
                throw new System.Exception("Error in ByteSpan.ByteSpan(): arr parameter is null!");
            if(offset >= arr.Length || offset < 0)
                throw new System.Exception("Error in ByteSpan.ByteSpan(): offset parameter out of bounds!");
            if(length > arr.Length - offset || length < 0)
                throw new System.Exception("Error in ByteSpan.ByteSpan(): length parameter out of bounds!");
            Buffer = arr;
            Offset = offset;
            Length = length;
        }
        
        public ByteSpan(byte[] arr)
        {
            if(arr == null)
                throw new System.Exception("Error in ByteSpan.ByteSpan(): parameter is null!");
            Buffer = arr;
            Offset = 0;
            Length = Buffer.Length;
        }

        public byte this[int index]
        {
            get
            {
                if(index >= Length || index < 0)
                    throw new System.Exception("Error in ByteSpan[]: index out of bounds!");
                return Buffer[Offset+index];    
            }
        }

        public ByteSpan Slice(int offset, int length)
        {
            if(offset == Length && length == 0)
            {
                return new ByteSpan(new byte[0]);
            }
            if(offset >= Length || offset < 0)
                throw new System.Exception("Error in ByteSpan.Slice() offset parameter out of bounds!");
            if(length > Length-offset || length < 0)
                throw new System.Exception("Error in ByteSpan.Slice() length parameter out of bounds!");
            return new ByteSpan(Buffer,Offset+offset,length);
        }

        public ByteSpan Slice(int offset)
        {
            return Slice(offset,Length-offset);
        }

        public bool SequenceEqual(byte[] arr)
        {
            if(arr.Length != Length)
                return false;
            for(int i=0; i<Length; i++)
            {
                if(this[i] != arr[i])
                    return false;
            }
            return true;
        }

        public bool SequenceEqual(ByteSpan span)
        {
            if(span.Length != Length)
                return false;
            for(int i=0; i<Length; i++)
            {
                if(this[i] != span[i])
                    return false;
            }
            return true;
        }

        public byte[] ToArray()
        {
            byte[] arr = new byte[Length];
            Array.Copy(Buffer,Offset,arr,0,Length);
            return arr;
        }
    }
}