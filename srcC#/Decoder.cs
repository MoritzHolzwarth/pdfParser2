using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace pdfParserByMH
{
    public partial class Decoder
    {
        public string filter {get; set;}
        public pdfDictionary decodeParms {get; set;}
        public byte[] arrBytes;
        public Decoder(byte[] arrBytesVal =null)
        {
            arrBytes = (arrBytesVal == null)? System.Array.Empty<byte>(): arrBytesVal;
        }
        public byte[] decode()
        {
            byte[] decodedBytes;
            switch (filter)
            {
                case "/FlateDecode": case "/Fl":
                    decodedBytes = doFlateDecode();
                    break;
                case "/ASCIIHexDecode": case "/AHx":
                    decodedBytes = doASCIIHexDecode();
                    break;
                case "/ASCII85Decode": case "/A85":
                    decodedBytes = doASCII85Decode();
                    break;
                default:
                    throw new System.Exception(string.Format("Error in Decoder.decode(): unknown filter {0}", filter));
            }
            return decodedBytes;
        }
        public byte[] encode()
        {
            byte[] encodedBytes;
            switch (filter)
            {
                case "/FlateDecode": case "/Fl":
                    encodedBytes = doFlateEncode();
                    break;
                case "/ASCIIHexDecode": case "/AHx":
                    encodedBytes = doASCIIHexEncode();
                    break;
                case "/ASCII85Decode": case "/A85":
                    encodedBytes = doASCII85Encode();
                    break;
                default:
                    throw new System.Exception(string.Format("Error in Decoder.encode(): unknown filter {0}", filter));
            }
            return encodedBytes;
        }
        private byte[] doFlateDecode()
        {
            ByteSpan spanBytesWithoutZLib = dataHasZLibWrapper(arrBytes)? new ByteSpan(arrBytes,2,arrBytes.Length-4): new ByteSpan(arrBytes);
            byte[] arrWithoutZLib = spanBytesWithoutZLib.ToArray();
            System.IO.MemoryStream memStreamOutput = new System.IO.MemoryStream();
            byte[] arrBytesDecompressed;
            try
            {
                System.IO.MemoryStream memStreamInput = new System.IO.MemoryStream(arrWithoutZLib);
                System.IO.Compression.DeflateStream defStream = new System.IO.Compression.DeflateStream(memStreamInput, System.IO.Compression.CompressionMode.Decompress);
                defStream.CopyTo(memStreamOutput);
                arrBytesDecompressed = memStreamOutput.ToArray();
            }
            catch
            {
                throw new System.Exception("Exception in Decoder.DoFlateDeode(): invalid stream encountered!");
            }
            standardizeFlateDecodeParms();
            byte[] arrBytesWithoutPredictor;
            int predictorVal = ((pdfInteger)decodeParms.getResolved("/Predictor")).value;
            if(predictorVal == 1)
            {
                arrBytesWithoutPredictor = arrBytesDecompressed;
            }
            else if(9 < predictorVal && predictorVal < 16)
            {
                arrBytesWithoutPredictor = undoPNGPredictor(arrBytesDecompressed);
            }
            else
            {
                throw new System.Exception(string.Format("Error in Decoder.doFlateDecode(): unknown /Predictor Value {0}!", 0));
            }
            return arrBytesWithoutPredictor;
        }

        private bool dataHasZLibWrapper(byte[] arr)
        {
            byte b0 = arr[0];
            byte b1 = arr[1];
            if ((b0 & 0x0F) != 8) 
                return false;
            if (((b0 << 8) + b1) % 31 != 0) 
                return false;
            return true;
        }

        private byte[] doFlateEncode()
        {
            standardizeFlateDecodeParms();
            byte[] arrBytesWithPredictor;
            int predictorVal = ((pdfInteger)decodeParms.getResolved("/Predictor")).value;
            if(predictorVal == 1)
            {
                arrBytesWithPredictor = arrBytes;
            }
            else if(9 < predictorVal && predictorVal < 16)
            {
                arrBytesWithPredictor = doPNGPredictor(arrBytes);
            }
            else
            {
                throw new System.Exception(string.Format("Error in Decoder.doFlateEncode(): unknown /Predictor Value {0}!", predictorVal));
            }
            System.IO.MemoryStream memStreamOutput = new System.IO.MemoryStream();
            using (System.IO.Compression.DeflateStream defStream = new System.IO.Compression.DeflateStream(memStreamOutput, System.IO.Compression.CompressionMode.Compress, leaveOpen: true))
            {
                defStream.Write(arrBytesWithPredictor,0,arrBytesWithPredictor.Length);
            }
            byte[] arrBytesCompressed = memStreamOutput.ToArray();
            byte[] adlerSum = Utils.getAdler32Sum(arrBytesCompressed);
            System.Collections.Generic.List<byte> lstBytesWithZLib = new System.Collections.Generic.List<byte>(2 + arrBytesCompressed.Length + adlerSum.Length);
            lstBytesWithZLib.AddRange(new byte[] { 0x78, 0x9C });
            lstBytesWithZLib.AddRange(arrBytesCompressed);
            lstBytesWithZLib.AddRange(adlerSum);
            return lstBytesWithZLib.ToArray();
        }

        private void standardizeFlateDecodeParms()
        {
            if(decodeParms.count() == 0)
            {
                decodeParms.add("/Predictor",new pdfInteger(1));
            }
            else if(!decodeParms.containsKey("/Predictor"))
            {
                throw new System.Exception("Error in Decoder.standardizeFlateDecodeParms(): decodeParms is not empty, but lacks /Predictor!");
            }

            if(!decodeParms.containsKey("/Columns"))
            {
                decodeParms.add("/Columns",new pdfInteger(1));
            }
            if(!decodeParms.containsKey("/Colors"))
            {
                decodeParms.add("/Colors",new pdfInteger(1));
            }
            if(!decodeParms.containsKey("/BitsPerComponent"))
            {
                decodeParms.add("/BitsPerComponent",new pdfInteger(8));
            }
        }

        private byte[] doPNGPredictor(byte[] arrBytesWithoutPredictor)
        {
            int numBytesWithoutPredictor = arrBytesWithoutPredictor.Length;
            int numPixelColumns = ((pdfInteger)decodeParms.getResolved("/Columns")).value;
            int numColorComponents = ((pdfInteger)decodeParms.getResolved("/Colors")).value;
            int numBitsPerComponent = ((pdfInteger)decodeParms.getResolved("/BitsPerComponent")).value;
            int numBytesPerPixel = (int)System.Math.Ceiling((float)numColorComponents*(float)numBitsPerComponent/8);
            int numDataBytesPerRow = (int)System.Math.Ceiling((float)numPixelColumns*(float)numColorComponents*(float)numBitsPerComponent/8);
            int numTotalBytesPerRow = numDataBytesPerRow + 1; //The first byte in each row will be the Predictor value for that row. Different rows can have different Predictors
            int numRows = (int)System.Math.Ceiling((float)numBytesWithoutPredictor/(float)numDataBytesPerRow);
            byte[] arrBytesWithPredictor = new byte[numBytesWithoutPredictor + numRows];
            int k = 0;
            //Note: The predictor value, stored in the first byte of each row is independent of the /Predictor entry.
            //There is no difference between the /Predictor entries 10, 11, 12, 13, 14, 15 and no correlation to the actual perdictor algorithm used.
            int predictorVal = 0; //I am giving all rows the trivial predictor 0, If you want you can implement other values 1 to 4.
            for (int i=0; i < numRows; i++)
            {   
                arrBytesWithPredictor[i*numTotalBytesPerRow] = (byte)predictorVal; //First byte in each row is the predictor value
                for(int j=i*numTotalBytesPerRow+1; j < (i+1)*numTotalBytesPerRow; j++) //Other bytes in each row are computed from the given data bytes by the predictor algorithm, which is trivial in this case
                {
                    arrBytesWithPredictor[j] = arrBytesWithoutPredictor[k];
                    k++;
                }
            }
            return arrBytesWithPredictor;
        }

        private byte[] undoPNGPredictor(byte[] arrBytesWithPredictor)
        {
            int numBytesWithPredictor = arrBytesWithPredictor.Length;
            int numPixelColumns = ((pdfInteger)decodeParms.getResolved("/Columns")).value;
            int numColorComponents = ((pdfInteger)decodeParms.getResolved("/Colors")).value;
            int numBitsPerComponent = ((pdfInteger)decodeParms.getResolved("/BitsPerComponent")).value;
            int numBytesPerPixel = (int)System.Math.Ceiling((float)numColorComponents*(float)numBitsPerComponent/8);
            int numDataBytesPerRow = (int)System.Math.Ceiling((float)numPixelColumns*(float)numColorComponents*(float)numBitsPerComponent/8);
            int numTotalBytesPerRow = numDataBytesPerRow + 1; //The first byte in each row is the Predictor value for that row. Different rows can have different Predictors
            int numRows = (int)System.Math.Ceiling((float)numBytesWithPredictor/(float)numTotalBytesPerRow);
            byte[] arrBytesWithoutPredictor = new byte[numBytesWithPredictor - numRows]; //subtract numRows because the 1st byte in each row (the predictor value) will not be transfered
            int k = 0; //index of the arrBytesWithoutPredictor
            for(int i=0; i < numRows; i++)
            {
                int predictorVal = (int)arrBytesWithPredictor[i*numTotalBytesPerRow]; //The predictor value for this row
                switch (predictorVal)
                {
                    case 0:
                        for(int j = i*numTotalBytesPerRow+1; j < (i+1)*numTotalBytesPerRow; j++)
                        {
                            arrBytesWithoutPredictor[k] = arrBytesWithPredictor[j];
                            k++;
                        }
                        break;
                    case 1:
                        for(int j = i*numTotalBytesPerRow+1; j < (i+1)*numTotalBytesPerRow; j++)
                            {
                                byte leftNeighbourByte = (k%numDataBytesPerRow < numBytesPerPixel)? (byte)0: arrBytesWithoutPredictor[k-numBytesPerPixel];
                                arrBytesWithoutPredictor[k] = (byte)((arrBytesWithPredictor[j] + leftNeighbourByte) % 256);
                                k++;
                            }
                        break;
                    case 2:
                        for(int j = i*numTotalBytesPerRow+1; j < (i+1)*numTotalBytesPerRow; j++)
                            {
                                byte upperNeighbourByte = (k < numDataBytesPerRow)? (byte)0: arrBytesWithoutPredictor[k-numDataBytesPerRow];
                                arrBytesWithoutPredictor[k] = (byte)((arrBytesWithPredictor[j] + upperNeighbourByte) % 256);
                                k++;
                            }
                        break;
                    case 3:
                        for(int j = i*numTotalBytesPerRow+1; j < (i+1)*numTotalBytesPerRow; j++)
                            {
                                byte leftNeighbourByte = (k%numDataBytesPerRow < numBytesPerPixel)? (byte)0: arrBytesWithoutPredictor[k-numBytesPerPixel];
                                byte upperNeighbourByte = (k < numDataBytesPerRow)? (byte)0: arrBytesWithoutPredictor[k-numDataBytesPerRow];
                                byte average = (byte)(0.5*(leftNeighbourByte + upperNeighbourByte));
                                arrBytesWithoutPredictor[k] = (byte)((arrBytesWithPredictor[j] + average) % 256);
                                k++;
                            }
                        break;
                    case 4:
                        for(int j = i*numTotalBytesPerRow+1; j < (i+1)*numTotalBytesPerRow; j++)
                            {
                                byte pathNeighbourByte = computePathNeighbourByte(arrBytesWithoutPredictor,numBytesPerPixel,numDataBytesPerRow,k);
                                arrBytesWithoutPredictor[k] = (byte)((arrBytesWithPredictor[j] + pathNeighbourByte) % 256);
                                k++;
                            }
                        break;
                    default:
                        throw new System.Exception(string.Format("Error in Decoder.undoPNGPredictor(): unknown row-Predictor value {0} encountered!", predictorVal));
                }
            }
            return arrBytesWithoutPredictor;
        }

        private byte[] doASCIIHexDecode()
        {
            System.Collections.Generic.List<byte> lstHexBytesWithoutWhiteSpace = new System.Collections.Generic.List<byte>(arrBytes.Length);
            foreach(byte b in arrBytes)
            {
                if(!Utils.byteIsASCIIWhitheSpace(b))
                {
                    lstHexBytesWithoutWhiteSpace.Add(b);
                }
            }
            if(lstHexBytesWithoutWhiteSpace[lstHexBytesWithoutWhiteSpace.Count-1] == '>') //A stream of ASCII Hex Bytes typically ends with a '>' marker
            {
                lstHexBytesWithoutWhiteSpace.RemoveAt(lstHexBytesWithoutWhiteSpace.Count-1);
            }
            if(lstHexBytesWithoutWhiteSpace.Count % 2 != 0)
            {
                lstHexBytesWithoutWhiteSpace.Add((byte)'0');
            }
            int lenHexBytes = lstHexBytesWithoutWhiteSpace.Count;
            byte[] arrDecodedBytes = new byte[lenHexBytes/2];
            byte[] arrHexBytesWithoutWhiteSpace = lstHexBytesWithoutWhiteSpace.ToArray();
            for(int i=0; i < lenHexBytes/2; i++)
            {
                string hexByte = System.Text.Encoding.ASCII.GetString(lstHexBytesWithoutWhiteSpace.GetRange(2*i,2).ToArray());
                arrDecodedBytes[i] = System.Convert.ToByte(hexByte, 16);
            }
            return arrDecodedBytes;
        }

        private byte[] doASCIIHexEncode()
        {
            System.Collections.Generic.List<byte> lstEncodedBytes = new System.Collections.Generic.List<byte>(2*arrBytes.Length+1);
            foreach(byte b in arrBytes)
            {
                byte[] hexBytes = System.Text.Encoding.ASCII.GetBytes(b.ToString("X2"));
                lstEncodedBytes.AddRange(hexBytes);
            }
            lstEncodedBytes.Add((byte)'>');
            return lstEncodedBytes.ToArray();
        }

        private byte[] doASCII85Encode()
        {
            long intialPowerOf85 = 85*85*85*85;
            int numTrueQuartets = arrBytes.Length / 4;
            int numPadding = (4 - (arrBytes.Length - 4*numTrueQuartets)) % 4; //If there are extra bytes left in arrBytes, find the difference to complete the last Quartet
            System.Collections.Generic.List<byte> lstPaddedBytes = new System.Collections.Generic.List<byte>(arrBytes.Length + numPadding);
            lstPaddedBytes.AddRange(arrBytes);
            for(int i=0; i < numPadding; i++)
            {
                lstPaddedBytes.Add(0); //Padd last Quartet with 0s
            }

            System.Collections.Generic.List<byte> lstASCII85Result = new System.Collections.Generic.List<byte>((int)(1.2*arrBytes.Length)+4); //Because 1.2 = 5/4
            
            for(int i=0; i < numTrueQuartets; i++)
            {
                byte[] quartet = lstPaddedBytes.GetRange(4*i,4).ToArray();
                long value = Utils.bytesToBigEndianInteger(quartet,4);
                if(value == 0)
                {
                    lstASCII85Result.Add(122); //Special abbreviation case: A quartet of value 0 gets the extra symbol 'z' (i.e. 122) instead of '!!!!!'
                    continue;
                }
                long powerOf85 = intialPowerOf85;
                for(int j=0; j < 5; j++)
                {
                    long digitVal = value / powerOf85;
                    value = value % powerOf85;
                    byte ascii85Digit = (byte)(digitVal + 33); //Compute the ASCII character of the base 85 digit, directly as byte, no need to create a char.
                    lstASCII85Result.Add(ascii85Digit);
                    powerOf85 = powerOf85 / 85;
                }
            }

            if(numPadding > 0)
            {
                byte[] extraQuartet = lstPaddedBytes.GetRange(4*numTrueQuartets,4).ToArray();
                long value = Utils.bytesToBigEndianInteger(extraQuartet,4);
                long powerOf85 = 85*85*85*85;
                for(int i=0; i < 5-numPadding; i++) //Add only the last (5-numPadding) ASCII85 digits of this quartet. Also no 0-z-Abbreviation here.
                {
                    long digitVal = value / powerOf85;
                    value = value % powerOf85;
                    byte ascii85Digit = (byte)(digitVal + 33);
                    lstASCII85Result.Add(ascii85Digit);
                    powerOf85 = powerOf85 / 85;
                }
            }
            //Note: According to PDF Specs, ASCII85 streams must end with ~>, but are not allowed to start with <~!
            lstASCII85Result.AddRange(new byte[] {126,62}); //Add trailing ~>
            return lstASCII85Result.ToArray();
        }

        private byte[] doASCII85Decode()
        {
            long intialPowerOf85 = 85*85*85*85;
            ByteSpan span = new ByteSpan(arrBytes);
            if(span[0] == 60 && span[1] == 126)
            {
                span = span.Slice(2); //Remove possible initial <~
            }
            if(span[span.Length-2] == 126 && span[span.Length-1] == 62)
            {
                span = span.Slice(0,span.Length-2); //Remove trailing ~>
            }

            System.Collections.Generic.List<byte> lstBytesWithoutWhiteSpaces = new System.Collections.Generic.List<byte>(span.Length);
            foreach(byte b in span.ToArray())
            {
                if(Utils.byteIsASCIIWhitheSpace(b)) //Ignore White Spaces
                {
                    continue;
                }
                else if(b == 122)
                {
                    lstBytesWithoutWhiteSpaces.AddRange(new byte[] {33,33,33,33,33}); //Replace 'z' by '!!!!!' wich becomes 0000.
                }
                else
                {
                    lstBytesWithoutWhiteSpaces.Add(b);
                }
            }

            int numTrueQuintets = lstBytesWithoutWhiteSpaces.Count / 5;
            int numPadding = (5 - (lstBytesWithoutWhiteSpaces.Count - 5*numTrueQuintets)) % 5;
            if(numPadding == 4)
                throw new System.Exception("Error in Decode.doASCII85Decode(): invalid Padding value 4!");

            System.Collections.Generic.List<byte> lstPaddedBytes = new System.Collections.Generic.List<byte>(lstBytesWithoutWhiteSpaces.Count + numPadding);
            lstPaddedBytes.AddRange(lstBytesWithoutWhiteSpaces);
            for(int i=0; i < numPadding; i++)
            {
                lstPaddedBytes.Add(117);
            }

            System.Collections.Generic.List<byte> lstDecodedBytes = new System.Collections.Generic.List<byte>((int)(0.8*lstBytesWithoutWhiteSpaces.Count)); //Because 0.8 = 4/5

            int k =0;
            while(k < 5*numTrueQuintets)
            {
                byte[] quintet = lstPaddedBytes.GetRange(k,5).ToArray();
                long value = 0;
                long powerOf85 = intialPowerOf85;
                for(int j=0; j<5; j++)
                {
                    value += (quintet[j]-33)*powerOf85;
                    powerOf85 = powerOf85 / 85;
                }
                lstDecodedBytes.AddRange(Utils.bigEndianIntegerToBytes(value,4));
                k += 5;
            }

            if(numPadding > 0)
            {
                byte[] extraQuintet = lstPaddedBytes.GetRange(5*numTrueQuintets,5).ToArray();
                long value = 0;
                long powerOf85 = intialPowerOf85;
                for(int j=0; j<5; j++)
                {
                    value += (extraQuintet[j]-33)*powerOf85;
                    powerOf85 = powerOf85 / 85;
                }
                ByteSpan extraQuartet = new ByteSpan(Utils.bigEndianIntegerToBytes(value,4));
                lstDecodedBytes.AddRange(extraQuartet.Slice(0,4-numPadding).ToArray());
            }
            return lstDecodedBytes.ToArray();
        }

        private byte computePathNeighbourByte(byte[] arr, int numBytesPerPixel, int numDataBytesPerRow, int index)
        {
            byte left = (index % numDataBytesPerRow < numBytesPerPixel)? (byte)0: arr[index-numBytesPerPixel];
            byte upper = (index < numDataBytesPerRow)? (byte)0: arr[index-numDataBytesPerRow];
            byte upperLeft = ((index % numDataBytesPerRow < numBytesPerPixel) || (index < numDataBytesPerRow))? (byte)0: arr[index-numBytesPerPixel-numDataBytesPerRow];

            int x = left + upper - upperLeft;
            int xleft = System.Math.Abs(x-left);
            int xupper = System.Math.Abs(x-upper);
            int xupperleft = System.Math.Abs(x-upperLeft);

            if(xleft < xupper && xleft < xupperleft)
            {
                return left;
            }
            else if(xupper < xupperleft)
            {
                return upper;
            }
            else
            {
                return upperLeft;
            }
        }
    }
}