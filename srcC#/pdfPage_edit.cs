using System;
using System.CodeDom;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;

namespace pdfParserByMH
{
    public partial class pdfPage
    {
        public string addFont(pdfObjectReference fontObref, xrefCollection xrefCol) //Important. This method requires a xrefCollection
        {
            if(!resources.containsKey("/Font")) //If the page's resources don't yet have a /Font dict, create a new one as a resource item
            {
                pdfDictionary newFontDict = new pdfDictionary();
                resources.add("/Font",newFontDict);
            }
            pdfDictionary fontDict = (pdfDictionary)resources.getResolved("/Font");
            string token = getFreeFontToken(fontDict.keys());
            fontDict.add(token,fontObref,xrefCol); //the resource's /Font dictionary (here called fontDict) only contains the font token 
            return token;                          //and an object reference to the 'actual' font dictionary (Which defines the font).
                                                   //that 'actual' font dict is a pdf Object by itself and may be referenced by several /Resources dicts
                                                   //When adding a new object reference to a pdfDictionary, we must also provied a xrefCol, 
                                                   // such that the dictionary can internally resolve the object reference.
        }

        private string getFreeFontToken(string[] arrTokens)
        {
            int i = 1;
            string token = "/F" + i.ToString();
            while(arrTokens.Contains(token))
            {
                i++;
                token = "/F" + i.ToString();
            }
            return token;
        }

        public void addObRefToContent(pdfObjectReference streamObRef, bool atBeginning = false)
        {
            if(dictionary.containsKey("/Contents")) //Usually, all pages of a pdf should contains a "/Contents" entry
            {
                pdfEntity contentEnt = dictionary.getResolved("/Contents");  //The usual procedure: dictionary.get() might just return an obRef, 
                                                                            //which may just lead to another obRef, which must eventually lead to some not-obRef.
                                                                            //via .getResolved() we get that final non-obRef entity directly.
                if(contentEnt is pdfArray) //That content-Entity should usually be a pdfArray of obRefs, each leading so some contnet-pdfStream
                {
                    pdfArray contArr = (pdfArray)contentEnt;
                    if(atBeginning)
                    {
                        contArr.InsertAtBeginning(streamObRef, document.xref); //we add the new streamObRef to the pdfArray, and also pass the document's xrefCollection (i.e. xref)
                    }                                                          //so that the pdfArray can by itself resolve the 'thing' whose obRef we are adding.
                    else //If not atBeginning then at End
                    {
                        contArr.add(streamObRef, document.xref);
                    }
                }                                        
                else if(contentEnt is pdfStream) //Sometimes, if the page only has one content stream, the /Contents entity might directly be that stream, without being wrapped in a pdfArray
                {
                    pdfArray newContArr = new pdfArray(); //in that case, we create a new pdfArray to contain all content streams
                    newContArr.add(dictionary.get("/Contents")); //This is the objectReference of that one already existing content Stream (note: dicttionary.get() instead of .getResolved())
                    if(atBeginning)
                    {
                        newContArr.InsertAtBeginning(streamObRef, document.xref); //and add the new content stream obRef
                    }
                    else //If not atBeginning then at End
                    {
                        newContArr.add(streamObRef,document.xref);
                    }
                    newContArr.ensureResolvedArray(document.xref);
                    dictionary.set("/Contents",newContArr);
                }
                else
                    throw new System.Exception("Error in pdfPage.addContentStream(): /Contents entity is neither pdfObjectReference nor pdfStream!");
            }
            else //but if not, we create a new /Contents pdfArray (not a new pdf object, only a new dictionary entry)
            {
                pdfArray newContArr = new pdfArray();
                newContArr.add(streamObRef);
                dictionary.add("/Contents",newContArr);
            }
            //Below is just for this program's bookkeeping. We keep and maintain a list of all of this page's content streams.
            pdfStream newStream = (pdfStream)document.xref.get(streamObRef.index);
            if(atBeginning)
            {
                lstContents.Insert(0,newStream);
            }
            else
            {
                lstContents.Add(newStream);
            }
        }

        private bool makePageVertical()
        {
            bool defaultIsVertical = mediaBox[2] < mediaBox[3];
            bool rotateStateIsEven = rotateState.value == 0 || rotateState.value == 180;
            if(defaultIsVertical)
            {
                if(rotateStateIsEven)
                {
                    return false; //page is already vertical, no adjustment needed
                }
                rotateState.value = (rotateState.value + 90) % 360;
                return true; //page vas not vertical. Adjustment applied
            }
            if(rotateStateIsEven)
            {
                rotateState.value = (rotateState.value + 90) % 360;
                return true; //page vas not vertical. Adjustment applied
            }
            return false; //page is already vertical, no adjustment needed
        }

        private bool makePageHorizontal()
        {
            bool defaultIsHorizontal = mediaBox[2] > mediaBox[3];
            bool rotateStateIsEven = rotateState.value == 0 || rotateState.value == 180;
            if(defaultIsHorizontal)
            {
                if(rotateStateIsEven)
                {
                    return false; //page is already horizonzal, no adjustment needed
                }
                rotateState.value = (rotateState.value + 90) % 360;
                return true; //page vas not horizonzal. Adjustment applied
            }
            if(rotateStateIsEven)
            {
                rotateState.value = (rotateState.value + 90) % 360;
                return true; //page vas not horizonzal. Adjustment applied
            }
            return false; //page is already horizonzal, no adjustment needed
        }

        public void addHeaderAndFooter(string headerText, string footerText, PageXPosition headerX, PageXPosition footerX, string fontToken, string fontName, int fontSize, double[] fontColor, 
                                        bool makeVertical =false, bool makeHorizontal =false, int xDistance = 20, int yDistance = 20)
        {
            bool rotationApplied = enforcePageOriantation(makeVertical, makeHorizontal);
            ValueTuple<string[], double[]> formatHeaderResult = formatStringForPDFStream(headerText, fontName, fontSize);
            ValueTuple<string[], double[]> formatFooterResult = formatStringForPDFStream(footerText, fontName, fontSize);
            string[] arrHeaderText = formatHeaderResult.Item1;
            double[] arrHeaderTextWidths = formatHeaderResult.Item2;
            string[] arrFooterText = formatFooterResult.Item1;
            double[] arrFooterTextWidths = formatFooterResult.Item2;
            double headerXPos = getTextLineXPosValue(headerX, mediaBox, arrHeaderTextWidths[0], xDistance, rotationApplied);
            double footerXPos = getTextLineXPosValue(footerX, mediaBox, arrFooterTextWidths[0], xDistance, rotationApplied);
            double headerYPos = mediaBox[1] + mediaBox[3] - yDistance;
            double footerYPos = mediaBox[1] + yDistance + (rotationApplied? (mediaBox[3] - mediaBox[2]): 0);
            double[] header_relXPos = getTextLinesRelativeXPosValues(headerX, arrHeaderTextWidths, arrHeaderTextWidths[0]);
            double[] footer_relXPos = getTextLinesRelativeXPosValues(footerX, arrFooterTextWidths, arrFooterTextWidths[0]);
            
            string headerCommand = getTextBlockStreamCommand(arrHeaderText, headerXPos, header_relXPos, headerYPos, fontSize, fontToken, fontColor, rotationApplied);
            string footerCommand = getTextBlockStreamCommand(arrFooterText, footerXPos, footer_relXPos, footerYPos, fontSize, fontToken, fontColor, rotationApplied);
            string fullCommand = string.Format("\n/HeaderAndFooterByMH BMC\n{0}\n{1}\nEMC\n", headerCommand, footerCommand);
            
            pdfObjectReference headerFooterStreamObRef = document.createNewStreamFromUncompressedData(Utils.stringToBytes(fullCommand));
            addObRefToContent(headerFooterStreamObRef, false); //'false' meand 'add at end of /Contents'
        }

        private string getTextBlockStreamCommand(string[] arrText, double xpos, double[] rel_xpos, double ypos, int fontSize, string fontToken, double[] fontColor, bool rotate)
        {
            int lineHeight = -Convert.ToInt32(fontSize*1.2);
            string cmCommand = rotate? string.Format("0 1 -1 0 {0} 0 cm", mediaBox[3]): "";
            string rgCommand = string.Format("{0} {1} {2} rg", fontColor[0], fontColor[1], fontColor[2]);
            string tfCommand = string.Format("{0} {1} Tf", fontToken, fontSize);
            string tlCommand = string.Format("{0} TL", lineHeight); 
            System.Text.StringBuilder strBuild = new System.Text.StringBuilder();
            strBuild.Append(string.Format("{0} {1} Td", xpos + rel_xpos[0], ypos));
            strBuild.Append(string.Format("\n{0} Tj", arrText[0]));
            if(arrText.Length > 1)
            {
                for(int i=1; i<arrText.Length; i++)
                {
                    strBuild.Append(string.Format("\n{0} -{1} Td", rel_xpos[i], lineHeight));
                    strBuild.Append(string.Format("\n{0} Tj", arrText[i]));
                }
            }
            string tdtjCommand = strBuild.ToString();
            string fullCommand = string.Format("q\n{0}\n BT \n{1}\n{2}\n{3}\n{4}\n ET \nQ", cmCommand, rgCommand, tfCommand, tlCommand, tdtjCommand);
            return fullCommand;
        }

        private double getTextLineXPosValue(PageXPosition xpos, double[] mediaBox, double textWidth, double xBoundary, bool rotationApplied)
        {
            switch (xpos)
            {
                case PageXPosition.Left:
                    return mediaBox[0] + xBoundary;
                case PageXPosition.Middle:
                    return 0.5*(-textWidth + (rotationApplied? mediaBox[3] + mediaBox[1]: mediaBox[2] + mediaBox[0]));
                case PageXPosition.Right:
                    return -textWidth - xBoundary + (rotationApplied? mediaBox[3] + mediaBox[1]: mediaBox[2] + mediaBox[0]);
                default:
                    //throw new System.Exception($"Error in pdfPage.getHeaderFooterXPositionValue(): unknown PageXPosition Value {xpos}!"); > C# 5
                    throw new System.Exception(string.Format("Error in pdfPage.getHeaderFooterXPositionValue(): unknown PageXPosition Value {0}!", xpos));
            }
        }

        private double[] getTextLinesRelativeXPosValues(PageXPosition xpos, double[] textWidths, double referenceWidth)
        {
            switch (xpos)
            {
                case PageXPosition.Left:
                    return new double[textWidths.Length];   //All 0, i.e. left alligned
                case PageXPosition.Middle:
                    return textWidths.Select(s => 0.5*(referenceWidth - s)).ToArray();
                case PageXPosition.Right:
                    return textWidths.Select(s => referenceWidth - s).ToArray();
                default:
                    throw new System.Exception(string.Format("Error in pdfPage.getHeaderFooterRelativeXPosValues(): unknown PageXPosition Value {0}!", xpos));
            }
        }

        private bool enforcePageOriantation(bool makeVertical, bool makeHorizontal)
        {
            if(makeVertical && makeHorizontal)
                throw new System.Exception("Error in pdfPage.enforcePageOrientation(): ivalid Page Orientation request: makeVertical && makeHorizontal!");
            if(makeVertical)
                return makePageVertical();
            if(makeHorizontal)
                return makePageHorizontal();

            return false;
        }




        public bool removeHeaderAndFooter()  //Never used this Function so far. Dont know if it works correctly
        {
            byte[] arrContiguosData = new byte[0];
            byte[][] arrDatasets = new byte[0][];
            getContiguousDataAndDatasetArray(ref arrContiguosData, ref arrDatasets);
            string fullPageCode = Utils.bytesToString(arrContiguosData);
            string pattern = "/HeaderAndFooterByMH\\s+BMC((?!EMC)[\\s\\S])*EMC";
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(pattern);
            if(!reg.IsMatch(fullPageCode))
            {
                return false;
            }
            var matches = reg.Matches(fullPageCode);
            if(matches.Count > 1)
                //throw new System.Exception($"Error in pdfPage.undo_Stempeln(): more than one Stempel marker found in page {this.number}");
                throw new System.Exception(string.Format("Error in pdfPage.undo_Stempeln(): more than one Stempel marker found in page {0}", this.number));
            int markedContStartPos_global = matches[0].Index;
            int posBehindMarkedCont_global = markedContStartPos_global + matches[0].Length + 1; //Note: +1 (Behind)
            int startStreamIndex = -1;
            int endStreamIndex = -1;
            int markedContStartPos_local = -1;
            int posBehindMarkedCont_local = -1;
            int lenBeforeMarkedContent = -1;
            int lenBehindMarkedContent = -1;
            int offset = 0;
            for(int i=0; i<arrDatasets.Length; i++)
            {
                int datasetLen = arrDatasets[i].Length;
                if((offset +  datasetLen > markedContStartPos_global) && (startStreamIndex == -1))
                {
                    startStreamIndex = i;
                    markedContStartPos_local = markedContStartPos_global - offset;
                    lenBeforeMarkedContent = markedContStartPos_local;
                }
                if((offset + datasetLen >= posBehindMarkedCont_global) && (endStreamIndex == -1)) //Note: >=
                {
                    endStreamIndex = i;
                    posBehindMarkedCont_local = posBehindMarkedCont_global - offset;
                    lenBehindMarkedContent = datasetLen - posBehindMarkedCont_local;
                    break;
                }
                offset += datasetLen;
            }
            if(startStreamIndex == endStreamIndex)
            {
                byte[] markedStreamData = arrDatasets[startStreamIndex];
                int markedStreamDataLen = markedStreamData.Length;
                byte[] newStreamData = new byte[lenBeforeMarkedContent + lenBehindMarkedContent];
                System.Array.Copy(markedStreamData,0,newStreamData,0,lenBeforeMarkedContent);
                System.Array.Copy(markedStreamData,posBehindMarkedCont_local,newStreamData,lenBeforeMarkedContent,lenBehindMarkedContent);
                lstContents[startStreamIndex].setNewData(newStreamData);
                return true;
            }
            byte[] startStreamData = arrDatasets[startStreamIndex];
            byte[] endStreamData = arrDatasets[endStreamIndex];
            byte[] newStartStreamData = new byte[lenBeforeMarkedContent];
            byte[] newEndStreamData = new byte[lenBehindMarkedContent];
            System.Array.Copy(startStreamData,0,newStartStreamData,0,lenBeforeMarkedContent);
            System.Array.Copy(endStreamData,posBehindMarkedCont_local,newEndStreamData,0,lenBehindMarkedContent);
            lstContents[startStreamIndex].setNewData(newStartStreamData);
            lstContents[endStreamIndex].setNewData(newEndStreamData);
            for(int i=startStreamIndex+1; i<endStreamIndex; i++)
            {
                lstContents[i].setNewData(new byte[0]);
            }
            return true;
        }

        private void getContiguousDataAndDatasetArray(ref byte[] arrContiguosData, ref byte[][] arrDatasets)
        {
            System.Collections.Generic.List<byte> lstContiguousData = new System.Collections.Generic.List<byte>();
            System.Collections.Generic.List<byte[]> lstDatasets = new System.Collections.Generic.List<byte[]>();
            for(int i=0; i < lstContents.Count; i++)
            {
                pdfStream pdfstream = lstContents[i];
                byte[] streamData = pdfstream.getUnfilteredData();
                lstContiguousData.AddRange(streamData);
                lstDatasets.Add(streamData);
            }
            if(arrContiguosData != null)
            {
                arrContiguosData = lstContiguousData.ToArray();
            }
            if(arrDatasets != null)
            {
                arrDatasets = lstDatasets.ToArray();
            }
        }

        public ValueTuple<string[], double[]> formatStringForPDFStream(string str, string fontName, int fontSize)
        {
            string str1 = System.Text.RegularExpressions.Regex.Replace(str, "/PagesCount", document.numPages.ToString());
            string str2 = System.Text.RegularExpressions.Regex.Replace(str1, "/PageNum", number.ToString());
            str2 = str2.Replace("\r", "");
            string[] arrStrings = str2.Split('\n');
            double[] arrTrueStringLengths = new double[arrStrings.Length];
            for(int i=0; i<arrStrings.Length; i++)
            {
                string subStr = arrStrings[i];
                arrTrueStringLengths[i] = document.fontData.getStandardizedTextLineWidth(subStr, fontName) * fontSize;
                byte[] subStrBytes = Encoding.GetEncoding("Windows-1252").GetBytes(subStr);
                System.Text.StringBuilder strBuild = new System.Text.StringBuilder();
                strBuild.Append('(');
                int j = 0;
                while(j < subStr.Length)
                {
                    byte b = subStrBytes[j];
                    if(b == '(' || b == ')'|| b == '\\')
                    {
                        strBuild.Append('\\');
                        strBuild.Append((char)b);
                        j++;
                    }
                    else if(b > 127)
                    {
                        strBuild.Append("\\" + Convert.ToString(b, 8).PadRight(3,'0'));
                        j++;
                    }
                    else
                    {
                        strBuild.Append((char)b);
                        j++;
                    }
                }
                strBuild.Append(')');
                arrStrings[i] = strBuild.ToString();
            }
            return new ValueTuple<string[],double[]>(arrStrings, arrTrueStringLengths);
        }
    }
}
