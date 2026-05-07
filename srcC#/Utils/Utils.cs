using System.Linq;
using System.Net;

namespace pdfParserByMH
{
    public enum PageXPosition {Left, Middle, Right}
    public enum PageQuantifiers {None, All, NoneExceptArray, AllExceptArray}
    public partial class Utils
    {
        public Utils()
        {}

        static public byte[] stringToBytes(string str)
        {
            return System.Text.Encoding.ASCII.GetBytes(str);
        }

        static public string bytesToString(byte[] arr)
        {
            return System.Text.Encoding.ASCII.GetString(arr);
        }

        static public string bytesToString(ByteSpan span)
        {
            return System.Text.Encoding.ASCII.GetString(span.ToArray());
        }

        static public byte[] getAdler32Sum(byte[] arr)
        {
            System.UInt32 modAdler = 65521;
            System.UInt32 x = 1;
            System.UInt32 y = 0;
            foreach(byte b in arr)
            {
                x = (x + b) % modAdler;
                y = (x + y) % modAdler;
            }
            System.UInt32 sum = (y << 16) | x;
            byte[] sumBytes = System.BitConverter.GetBytes(sum);
            System.Array.Reverse(sumBytes);
            return sumBytes;
        }

        static public byte[] bigEndianIntegerToBytes(long num, int width)
        {
            byte[] arr = new byte[width];
            int shift = (width-1)*8;
            for(int i=0; i < width; i++)
            {
                long highestDigit = (num >> shift) & 0xFF;
                arr[i] = (byte)highestDigit;
                shift = shift - 8;
            }
            return arr;
        }

        static public long bytesToBigEndianInteger(byte[] chunk, int width)
        {
            switch (width)
            {
                case 1:
                    return (long)chunk[0];
                case 2:
                    return ((long)chunk[0] << 8) + (long)chunk[1];
                case 3:
                    return ((long)chunk[0] << 16) + ((long)chunk[1] << 8) + (long)chunk[2];
                case 4:
                    return ((long)chunk[0] << 24) + ((long)chunk[1] << 16) + ((long)chunk[2] << 8) + (long)chunk[3];
                default:
                    //throw new System.Exception($"Error in bytesToBigEndianInteger(): Invalid Width {width}"); > C# 5
                    throw new System.Exception(string.Format("Error in bytesToBigEndianInteger(): Invalid Width {0}", 0));
            }
        }

        static public long bytesToBigEndianInteger(ByteSpan chunk, int width)
        {
            switch (width)
            {
                case 1:
                    return (long)chunk[0];
                case 2:
                    return ((long)chunk[0] << 8) + (long)chunk[1];
                case 3:
                    return ((long)chunk[0] << 16) + ((long)chunk[1] << 8) + (long)chunk[2];
                case 4:
                    return ((long)chunk[0] << 24) + ((long)chunk[1] << 16) + ((long)chunk[2] << 8) + (long)chunk[3];
                default:
                    //throw new System.Exception($"Error in bytesToBigEndianInteger(): Invalid Width {width}"); > C# 5
                    throw new System.Exception(string.Format("Error in bytesToBigEndianInteger(): Invalid Width {0}", width));
            }
        }

        static public byte[] numberToASCIIBytes(int num)
        {
            return System.Text.Encoding.ASCII.GetBytes(num.ToString());
        }

        static public byte[] numberToASCIIBytes(double num)
        {
            return System.Text.Encoding.ASCII.GetBytes(num.ToString());
        }

        static public bool arrayInArrayArrayByValue(double[][] arrArrays, double[] arr)
        {
            foreach(double[] arrElement in arrArrays)
            {
                if(arrElement.SequenceEqual(arr))
                {
                    return true;
                }
            }
            return false;
        }

        static public void InitializeCulture()
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;

            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = inv;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = inv;

            System.Threading.Thread.CurrentThread.CurrentCulture = inv;
            System.Threading.Thread.CurrentThread.CurrentUICulture = inv;
        }

        static public int numSigBits(int x)
        {
            if(x == 0)
            {
                return 1;
            }
            return (int)System.Math.Floor(System.Math.Log10(x)/System.Math.Log10(2)) + 1;
        }

        static public uint reverseBits(uint x, int len)
        {
            uint rev = 0;
            for(int i=0; i< len; i++)
            {
                rev <<= 1;
                uint newBit = x & 1u;
                rev |= newBit;
                x >>= 1;
            }
            return rev;
        }
    }
}