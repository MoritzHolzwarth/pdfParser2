using System;
using System.CodeDom;
using System.Collections;
using System.Data.Common;
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
                rotateState.value = (rotateState.value + 270) % 360;
                return true; //page vas not vertical. Adjustment applied
            }
            if(rotateStateIsEven)
            {
                rotateState.value = (rotateState.value + 270) % 360;
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

        //addHeaderAndFooter includes some possible page rotation in order to assure header and footer are always placed on the page vertically.
        //This includes the decision of whether to turn a horizontal page by +90 degrees or -90 degrees (counter clockwise).
        //Here we chose the +90 degreed convention.
        //The commented functions below handle the analogous choice for -90 degrees.
        public void addHeaderAndFooter(string headerText, string footerText, PageXPosition headerX, PageXPosition footerX, string fontToken, string fontName, int fontSize, double[] fontColor, 
                                        bool makeVertical =false, bool makeHorizontal =false, int xDistance = 20, int yDistance = 20)
        {
        //If the page is by default Horizontal but is rendered vertical due to a 90° or 270° Rotate State, the header/footer text must be flipped
            bool defaultIsHorizontal = mediaBox[2] > mediaBox[3];   //This flag is used below to ensure the header and footer are always placed on the page vertically, regardless of the pages rendering.
            enforcePageOriantation(makeVertical, makeHorizontal);   //This is only a sigle flag for the renderer. Does nothing to any stream or mediaBox.
            ValueTuple<string[], double[]> formatHeaderResult = formatStringForPDFStream(headerText, fontName, fontSize);
            ValueTuple<string[], double[]> formatFooterResult = formatStringForPDFStream(footerText, fontName, fontSize);
            string[] arrHeaderText = formatHeaderResult.Item1;
            double[] arrHeaderTextWidths = formatHeaderResult.Item2;
            string[] arrFooterText = formatFooterResult.Item1;
            double[] arrFooterTextWidths = formatFooterResult.Item2;
            double headerXPos = getTextLineXPosValue(headerX, mediaBox, arrHeaderTextWidths[0], xDistance, defaultIsHorizontal);
            double footerXPos = getTextLineXPosValue(footerX, mediaBox, arrFooterTextWidths[0], xDistance, defaultIsHorizontal);
            double headerYPos = mediaBox[1] + mediaBox[3] - yDistance + (defaultIsHorizontal? mediaBox[2] - mediaBox[3]: 0);
            double footerYPos = mediaBox[1] + yDistance;
            double[] header_relXPos = getTextLinesRelativeXPosValues(headerX, arrHeaderTextWidths, arrHeaderTextWidths[0]);
            double[] footer_relXPos = getTextLinesRelativeXPosValues(footerX, arrFooterTextWidths, arrFooterTextWidths[0]);
            
            string headerCommand = getTextBlockStreamCommand(arrHeaderText, headerXPos, header_relXPos, headerYPos, fontSize, fontToken, fontColor, defaultIsHorizontal);
            string footerCommand = getTextBlockStreamCommand(arrFooterText, footerXPos, footer_relXPos, footerYPos, fontSize, fontToken, fontColor, defaultIsHorizontal);
            string fullCommand = string.Format("\n/HeaderAndFooterByMH BMC\n{0}\n{1}\nEMC\n", headerCommand, footerCommand);
            
            pdfObjectReference headerFooterStreamObRef = document.createNewStreamFromUncompressedData(Utils.stringToBytes(fullCommand));
            addObRefToContent(headerFooterStreamObRef, false); //'false' means 'add at end of /Contents'
        }

        public void addHeaderAndFooter_test(string headerText, string footerText, PageXPosition headerX, PageXPosition footerX, string fontToken, string fontName, int fontSize, double[] fontColor, 
                                        bool makeVertical =false, bool makeHorizontal =false, int xDistance = 20, int yDistance = 20)
        {
        //If the page is by default Horizontal but is rendered vertical due to a 90° or 270° Rotate State, the header/footer text must be flipped
            bool defaultIsHorizontal = mediaBox[2] > mediaBox[3];   //This flag is used below to ensure the header and footer are always placed on the page vertically, regardless of the pages rendering.
            enforcePageOriantation(makeVertical, makeHorizontal);   //This is only a sigle flag for the renderer. Does nothing to any stream or mediaBox.
            ValueTuple<string[], double[]> formatHeaderResult = formatStringForPDFStream(headerText, fontName, fontSize);
            ValueTuple<string[], double[]> formatFooterResult = formatStringForPDFStream(footerText, fontName, fontSize);
            string[] arrHeaderText = formatHeaderResult.Item1;
            double[] arrHeaderTextWidths = formatHeaderResult.Item2;
            string[] arrFooterText = formatFooterResult.Item1;
            double[] arrFooterTextWidths = formatFooterResult.Item2;
            double headerXPos = getTextLineXPosValue(headerX, mediaBox, arrHeaderTextWidths[0], xDistance, false);
            double footerXPos = getTextLineXPosValue(footerX, mediaBox, arrFooterTextWidths[0], xDistance, false);
            double headerYPos = mediaBox[1] + mediaBox[3] - yDistance + (false? mediaBox[2] - mediaBox[3]: 0);
            double footerYPos = mediaBox[1] + yDistance;
            double[] header_relXPos = getTextLinesRelativeXPosValues(headerX, arrHeaderTextWidths, arrHeaderTextWidths[0]);
            double[] footer_relXPos = getTextLinesRelativeXPosValues(footerX, arrFooterTextWidths, arrFooterTextWidths[0]);
            
            string headerCommand = getTextBlockStreamCommand_testHeader(arrHeaderText, headerX, headerXPos, header_relXPos, headerYPos, fontSize, fontToken, fontColor, defaultIsHorizontal);
            string footerCommand = getTextBlockStreamCommand_testFooter(arrFooterText, footerX, footerXPos, footer_relXPos, footerYPos, fontSize, fontToken, fontColor, defaultIsHorizontal);
            string fullCommand = string.Format("\n/HeaderAndFooterByMH BMC\n{0}\n{1}\nEMC\n", headerCommand, footerCommand);
            
            pdfObjectReference headerFooterStreamObRef = document.createNewStreamFromUncompressedData(Utils.stringToBytes(fullCommand));
            addObRefToContent(headerFooterStreamObRef, false); //'false' means 'add at end of /Contents'
        }

        private string getTextBlockStreamCommand_testHeader(string[] arrText, PageXPosition xType, double xpos, double[] rel_xpos, double ypos, int fontSize, string fontToken, double[] fontColor, bool defaultIsHorizontal)
        {
            int lineHeight = -Convert.ToInt32(fontSize*1.2);
            string cmCommand = null;

            if(rotateState.value == 0)
            {
                if(defaultIsHorizontal)
                {
                    double dx = mediaBox[2] - mediaBox[3];
                    double dy = 0;
                    switch (xType)
                    {
                        case PageXPosition.Left:
                            dy = mediaBox[3];
                            break;
                        case PageXPosition.Middle:
                            dy = 0.5*(mediaBox[2] + mediaBox[3]);
                            break;
                        case PageXPosition.Right:
                            dy = mediaBox[2];
                            break;
                    }
                    cmCommand = string.Format("0 -1 1 0 {0} {1} cm", dx, dy);
                }
                else
                    cmCommand = "";
            }
            else if(rotateState.value == 90)
            {
                if(defaultIsHorizontal)
                {
                    double dx = mediaBox[3];
                    double dy = 0;
                    switch (xType)
                    {
                        case PageXPosition.Left:
                            dy = 0;
                            break;
                        case PageXPosition.Middle:
                            dy = -0.5*(mediaBox[2] - mediaBox[3]);
                            break;
                        case PageXPosition.Right:
                            dy = -(mediaBox[2] - mediaBox[3]);
                            break;
                    }
                    cmCommand = string.Format("0 1 -1 0 {0} {1} cm", dx, dy);
                }
                else
                    cmCommand = "";
            }
            else if(rotateState.value == 180)
            {

                if(defaultIsHorizontal)
                {
                    double dx = mediaBox[3];
                    double dy = 0;
                    switch (xType)
                    {
                        case PageXPosition.Left:
                            dy = 0;
                            break;
                        case PageXPosition.Middle:
                            dy = -0.5*(mediaBox[2] - mediaBox[3]);
                            break;
                        case PageXPosition.Right:
                            dy = -(mediaBox[2] - mediaBox[3]);
                            break;
                    }
                    cmCommand = string.Format("0 1 -1 0 {0} {1} cm", dx, dy);
                }
                else
                    cmCommand = string.Format("-1 0 0 -1 {0} {1} cm", -mediaBox[2], mediaBox[3]);
            }
            else if(rotateState.value == 270)
            {
                if(defaultIsHorizontal)
                {
                    double dx = mediaBox[2] - mediaBox[3];
                    double dy = 0;
                    switch (xType)
                    {
                        case PageXPosition.Left:
                            dy = mediaBox[3];
                            break;
                        case PageXPosition.Middle:
                            dy = 0.5*(mediaBox[2] + mediaBox[3]);
                            break;
                        case PageXPosition.Right:
                            dy = mediaBox[2] + mediaBox[3];
                            break;
                    }
                    cmCommand = string.Format("0 -1 1 0 {0} {1} cm", dx, dy);
                }
                else
                    cmCommand = string.Format("-1 0 0 -1 {0} {1} cm", -mediaBox[2], mediaBox[3]);
            }
            else
                throw new Exception(string.Format("Invalid rotateState! {}", rotateState.value));

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
                    strBuild.Append(string.Format("\n{0} {1} Td", rel_xpos[i], lineHeight));
                    strBuild.Append(string.Format("\n{0} Tj", arrText[i]));
                }
            }
            string tdtjCommand = strBuild.ToString();
            string fullCommand = string.Format("q\n{0}\n BT \n{1}\n{2}\n{3}\n{4}\n ET \nQ", cmCommand, rgCommand, tfCommand, tlCommand, tdtjCommand);
            return fullCommand;
        }

        private string getTextBlockStreamCommand_testFooter(string[] arrText, PageXPosition xType, double xpos, double[] rel_xpos, double ypos, int fontSize, string fontToken, double[] fontColor, bool defaultIsHorizontal)
        {
            int lineHeight = -Convert.ToInt32(fontSize*1.2);
            string cmCommand = null;

            if(rotateState.value == 0)
            {
                if(defaultIsHorizontal)
                {
                    double dx = 0;
                    double dy = 0;
                    switch (xType)
                    {
                        case PageXPosition.Left:
                            dy = mediaBox[3];
                            break;
                        case PageXPosition.Middle:
                            dy = 0.5*(mediaBox[2] + mediaBox[3]);
                            break;
                        case PageXPosition.Right:
                            dy = mediaBox[2];
                            break;
                    }
                    cmCommand = string.Format("0 -1 1 0 {0} {1} cm", dx, dy);
                }
                else
                    cmCommand = "";
            }
            else if(rotateState.value == 90)
            {
                if(defaultIsHorizontal)
                {
                    double dx = mediaBox[2];
                    double dy = 0;
                    switch (xType)
                    {
                        case PageXPosition.Left:
                            dy = 0;
                            break;
                        case PageXPosition.Middle:
                            dy = -0.5*(mediaBox[2] - mediaBox[3]);
                            break;
                        case PageXPosition.Right:
                            dy = -(mediaBox[2] - mediaBox[3]);
                            break;
                    }
                    cmCommand = string.Format("0 1 -1 0 {0} {1} cm", dx, dy);
                }
                else
                    cmCommand = "";
            }
            else if(rotateState.value == 180)
            {

                if(defaultIsHorizontal)
                {
                    double dx = mediaBox[2];
                    double dy = 0;
                    switch (xType)
                    {
                        case PageXPosition.Left:
                            dy = 0;
                            break;
                        case PageXPosition.Middle:
                            dy = -0.5*(mediaBox[2] - mediaBox[3]);
                            break;
                        case PageXPosition.Right:
                            dy = -(mediaBox[2] - mediaBox[3]);
                            break;
                    }
                    cmCommand = string.Format("0 1 -1 0 {0} {1} cm", dx, dy);
                }
                else
                    cmCommand = string.Format("-1 0 0 -1 {0} {1} cm", -mediaBox[2], 0);
            }
            else if(rotateState.value == 270)
            {
                if(defaultIsHorizontal)
                {
                    double dx = 0;
                    double dy = 0;
                    switch (xType)
                    {
                        case PageXPosition.Left:
                            dy = mediaBox[3];
                            break;
                        case PageXPosition.Middle:
                            dy = 0.5*(mediaBox[2] + mediaBox[3]);
                            break;
                        case PageXPosition.Right:
                            dy = mediaBox[2] + mediaBox[3];
                            break;
                    }
                    cmCommand = string.Format("0 -1 1 0 {0} {1} cm", dx, dy);
                }
                else
                    cmCommand = string.Format("-1 0 0 -1 {0} {1} cm", -mediaBox[2], 0);
            }
            else
                throw new Exception(string.Format("Invalid rotateState! {}", rotateState.value));

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
                    strBuild.Append(string.Format("\n{0} {1} Td", rel_xpos[i], lineHeight));
                    strBuild.Append(string.Format("\n{0} Tj", arrText[i]));
                }
            }
            string tdtjCommand = strBuild.ToString();
            string fullCommand = string.Format("q\n{0}\n BT \n{1}\n{2}\n{3}\n{4}\n ET \nQ", cmCommand, rgCommand, tfCommand, tlCommand, tdtjCommand);
            return fullCommand;
        }

        private string getTextBlockStreamCommand(string[] arrText, double xpos, double[] rel_xpos, double ypos, int fontSize, string fontToken, double[] fontColor, bool rotate)
        {
            int lineHeight = -Convert.ToInt32(fontSize*1.2);
            string cmCommand = rotate? string.Format("0 -1 1 0 0 0 cm"): "";
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
                    strBuild.Append(string.Format("\n{0} {1} Td", rel_xpos[i], lineHeight));
                    strBuild.Append(string.Format("\n{0} Tj", arrText[i]));
                }
            }
            string tdtjCommand = strBuild.ToString();
            string fullCommand = string.Format("q\n{0}\n BT \n{1}\n{2}\n{3}\n{4}\n ET \nQ", cmCommand, rgCommand, tfCommand, tlCommand, tdtjCommand);
            return fullCommand;
        }

        private double getTextLineXPosValue(PageXPosition xpos, double[] mediaBox, double textWidth, double xBoundary, bool rotated)
        {
            switch (xpos)
            {
                case PageXPosition.Left:
                    return mediaBox[0] + xBoundary - (rotated? mediaBox[3]: 0);
                case PageXPosition.Middle:
                    return 0.5*(-textWidth + (rotated? mediaBox[1] - mediaBox[3] : mediaBox[2] + mediaBox[0]));
                case PageXPosition.Right:
                    return -textWidth - xBoundary + (rotated? mediaBox[1]: mediaBox[2] + mediaBox[0]);
                default:
                    //throw new System.Exception($"Error in pdfPage.getHeaderFooterXPositionValue(): unknown PageXPosition Value {xpos}!"); > C# 5
                    throw new System.Exception(string.Format("Error in pdfPage.getHeaderFooterXPositionValue(): unknown PageXPosition Value {0}!", xpos));
            }
        }

        //Same as the 3 above functions but with vertical rotation being done by -90 degrees instead of +90 degrees.
        // public void addHeaderAndFooter_2(string headerText, string footerText, PageXPosition headerX, PageXPosition footerX, string fontToken, string fontName, int fontSize, double[] fontColor, 
        //                                 bool makeVertical =false, bool makeHorizontal =false, int xDistance = 20, int yDistance = 20)
        // {
        //     enforcePageOriantation(makeVertical, makeHorizontal);   //This is only a sigle flag for the renderer. Does nothing to any stream or mediaBox.
        //     bool defaultIsHorizontal = mediaBox[2] > mediaBox[3];   //This flag is used below to ensure the header and footer are always placed on the page vertically, regardless of the pages rendering.
        //     ValueTuple<string[], double[]> formatHeaderResult = formatStringForPDFStream(headerText, fontName, fontSize);
        //     ValueTuple<string[], double[]> formatFooterResult = formatStringForPDFStream(footerText, fontName, fontSize);
        //     string[] arrHeaderText = formatHeaderResult.Item1;
        //     double[] arrHeaderTextWidths = formatHeaderResult.Item2;
        //     string[] arrFooterText = formatFooterResult.Item1;
        //     double[] arrFooterTextWidths = formatFooterResult.Item2;
        //     double headerXPos = getTextLineXPosValue_2(headerX, mediaBox, arrHeaderTextWidths[0], xDistance, defaultIsHorizontal);
        //     double footerXPos = getTextLineXPosValue_2(footerX, mediaBox, arrFooterTextWidths[0], xDistance, defaultIsHorizontal);
        //     double headerYPos = mediaBox[1] + mediaBox[3] - yDistance - (defaultIsHorizontal? mediaBox[3]: 0);
        //     double footerYPos = mediaBox[1] + yDistance - (defaultIsHorizontal? mediaBox[2]: 0);
        //     double[] header_relXPos = getTextLinesRelativeXPosValues(headerX, arrHeaderTextWidths, arrHeaderTextWidths[0]);
        //     double[] footer_relXPos = getTextLinesRelativeXPosValues(footerX, arrFooterTextWidths, arrFooterTextWidths[0]);
            
        //     string headerCommand = getTextBlockStreamCommand_2(arrHeaderText, headerXPos, header_relXPos, headerYPos, fontSize, fontToken, fontColor, defaultIsHorizontal);
        //     string footerCommand = getTextBlockStreamCommand_2(arrFooterText, footerXPos, footer_relXPos, footerYPos, fontSize, fontToken, fontColor, defaultIsHorizontal);
        //     string fullCommand = string.Format("\n/HeaderAndFooterByMH BMC\n{0}\n{1}\nEMC\n", headerCommand, footerCommand);
            
        //     pdfObjectReference headerFooterStreamObRef = document.createNewStreamFromUncompressedData(Utils.stringToBytes(fullCommand));
        //     addObRefToContent(headerFooterStreamObRef, false); //'false' means 'add at end of /Contents'
        // }

        // private string getTextBlockStreamCommand_2(string[] arrText, double xpos, double[] rel_xpos, double ypos, int fontSize, string fontToken, double[] fontColor, bool rotate)
        // {
        //     int lineHeight = -Convert.ToInt32(fontSize*1.2);
        //     string cmCommand = rotate? string.Format("0 1 -1 0 0 0 cm"): "";
        //     string rgCommand = string.Format("{0} {1} {2} rg", fontColor[0], fontColor[1], fontColor[2]);
        //     string tfCommand = string.Format("{0} {1} Tf", fontToken, fontSize);
        //     string tlCommand = string.Format("{0} TL", lineHeight); 
        //     System.Text.StringBuilder strBuild = new System.Text.StringBuilder();
        //     strBuild.Append(string.Format("{0} {1} Td", xpos + rel_xpos[0], ypos));
        //     strBuild.Append(string.Format("\n{0} Tj", arrText[0]));
        //     if(arrText.Length > 1)
        //     {
        //         for(int i=1; i<arrText.Length; i++)
        //         {
        //             strBuild.Append(string.Format("\n{0} {1} Td", rel_xpos[i], lineHeight));
        //             strBuild.Append(string.Format("\n{0} Tj", arrText[i]));
        //         }
        //     }
        //     string tdtjCommand = strBuild.ToString();
        //     string fullCommand = string.Format("q\n{0}\n BT \n{1}\n{2}\n{3}\n{4}\n ET \nQ", cmCommand, rgCommand, tfCommand, tlCommand, tdtjCommand);
        //     return fullCommand;
        // }

        // private double getTextLineXPosValue_2(PageXPosition xpos, double[] mediaBox, double textWidth, double xBoundary, bool rotated)
        // {
        //     switch (xpos)
        //     {
        //         case PageXPosition.Left:
        //             return mediaBox[0] + xBoundary;
        //         case PageXPosition.Middle:
        //             return 0.5*(-textWidth + (rotated? mediaBox[3] + mediaBox[1]: mediaBox[2] + mediaBox[0]));
        //         case PageXPosition.Right:
        //             return -textWidth - xBoundary + (rotated? mediaBox[3] + mediaBox[1]: mediaBox[2] + mediaBox[0]);
        //         default:
        //             //throw new System.Exception($"Error in pdfPage.getHeaderFooterXPositionValue(): unknown PageXPosition Value {xpos}!"); > C# 5
        //             throw new System.Exception(string.Format("Error in pdfPage.getHeaderFooterXPositionValue(): unknown PageXPosition Value {0}!", xpos));
        //     }
        // }

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


        public bool removeHeaderAndFooter_new()  //Never used this Function so far. Dont know if it works correctly
        {
            byte[] arrContiguosData = new byte[0];  //data of a streams in this page, glued together
            byte[][] arrDatasets = new byte[0][];   //all individual stream datas in page sequencial order
            getContiguousDataAndDatasetArray(ref arrContiguosData, ref arrDatasets);
            string fullPageCode = Utils.bytesToString(arrContiguosData);    //represent bytes as Latin1 string for use of regex
            string pattern = "/HeaderAndFooterByMH\\s+BMC";
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(pattern);
            if(!reg.IsMatch(fullPageCode))
            {
                return false;
            }
            var matches = reg.Matches(fullPageCode);
            
            foreach(System.Text.RegularExpressions.Match match in matches)
            {
                int markedContStartPos_global = match.Index;   //starting position of marekd content in contiguous stream data
                int lenMarkedContent = -1;
                ByteSpan markedContentSpan = new ByteSpan(arrContiguosData);
                markedContentSpan = markedContentSpan.Slice(markedContStartPos_global);
                if(!Utils.readMarkedContentInStream(markedContentSpan, ref lenMarkedContent))
                {
                    Console.Error.WriteLine("Note in pdfPage.removeHeaderAndFooter(): found seemingly marked Content that didn't validate!");
                    continue;
                }
                int posBehindMarkedCont_global = markedContStartPos_global + lenMarkedContent + 1; //position behind marekd content in contiguous stream data

                int startStreamIndex = -1;
                int endStreamIndex = -1;
                int markedContStartPos_local = -1;
                int posBehindMarkedCont_local = -1;
                int lenBeforeMarkedContent = -1;
                int lenBehindMarkedContent = -1;

                int offset = 0; //measured how far we move in the contiguous stream data when iterating over individual streams
                for(int i=0; i<arrDatasets.Length; i++) //Find the individual streams in which the marked content starts and ends
                {
                    int datasetLen = arrDatasets[i].Length;
                    if((offset +  datasetLen > markedContStartPos_global) && (startStreamIndex == -1))  //check if the marked content starts before this stream's end
                    {                                                                                   //If that is the case, and this is the first such stream, then the marked content starts inside this stream
                        startStreamIndex = i;
                        markedContStartPos_local = markedContStartPos_global - offset;
                        lenBeforeMarkedContent = markedContStartPos_local;
                    }
                    if((offset + datasetLen >= posBehindMarkedCont_global) && (endStreamIndex == -1))   //check if the marked content ends before this stream's end
                    {                                                                                   //If that is the case, and this is the first such stream, then the marked content ends inside this stream
                        endStreamIndex = i;
                        posBehindMarkedCont_local = posBehindMarkedCont_global - offset;
                        lenBehindMarkedContent = datasetLen - posBehindMarkedCont_local;
                        break;
                    }
                    offset += datasetLen;
                }
                if(startStreamIndex == endStreamIndex)  //If the marked content starts and ends within the same stream
                {
                    byte[] markedStreamData = arrDatasets[startStreamIndex];    //all data of the stream with marked content
                    int markedStreamDataLen = markedStreamData.Length;
                    byte[] newStreamData = new byte[lenBeforeMarkedContent + lenBehindMarkedContent];   //the new stream data shall exclude the marked content, hence include only the data before and after the marked content.
                    System.Array.Copy(markedStreamData,0,newStreamData,0,lenBeforeMarkedContent);       //copy the data before marked content from the original stream data to new stream data.
                    System.Array.Copy(markedStreamData,posBehindMarkedCont_local,newStreamData,lenBeforeMarkedContent,lenBehindMarkedContent);  //copy the data behind the marked content from otiginal stream data to new stream data
                    lstContents[startStreamIndex].setNewData(newStreamData);    //simply replace the old stream data (with marked content) with the new stream data (without marked content)
                    return true;
                }
                else
                {
                    byte[] startStreamData = arrDatasets[startStreamIndex]; //all data of the stream in which marked content starts
                    byte[] endStreamData = arrDatasets[endStreamIndex];     //all data of the stream in which marked content ends
                    byte[] newStartStreamData = new byte[lenBeforeMarkedContent];   //The stream where the marked content starts shall keep only its data before the marked content
                    byte[] newEndStreamData = new byte[lenBehindMarkedContent];     //The stream where the marked content ends shall keep only its data behind the marked content *..
                    System.Array.Copy(startStreamData,0,newStartStreamData,0,lenBeforeMarkedContent);
                    System.Array.Copy(endStreamData,posBehindMarkedCont_local,newEndStreamData,0,lenBehindMarkedContent);
                    lstContents[startStreamIndex].setNewData(newStartStreamData);
                    lstContents[endStreamIndex].setNewData(newEndStreamData);
                    for(int i=startStreamIndex+1; i<endStreamIndex; i++)
                    {
                        lstContents[i].setNewData(new byte[0]); //*.. and all inbetween streams shall have no data at all, since all their data was purely marked content
                    }
                }

            }
            return true;
        }

        public bool removeHeaderAndFooter()  //Never used this Function so far. Dont know if it works correctly
        {
            byte[] arrContiguosData = new byte[0];  //data of a streams in this page, glued together
            byte[][] arrDatasets = new byte[0][];   //all individual stream datas in page sequencial order
            getContiguousDataAndDatasetArray(ref arrContiguosData, ref arrDatasets);
            string fullPageCode = Utils.bytesToString(arrContiguosData);    //represent bytes as Latin1 string for use of regex
            string pattern = "/HeaderAndFooterByMH\\s+BMC((?!EMC)[\\s\\S])*EMC";
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(pattern);
            if(!reg.IsMatch(fullPageCode))
            {
                return false;
            }
            var matches = reg.Matches(fullPageCode);
            if(matches.Count > 1)
                throw new System.Exception(string.Format("Error in pdfPage.undo_Stempeln(): more than one Stempel marker found in page {0}", this.number));
            int markedContStartPos_global = matches[0].Index;   //starting position of marekd content in contiguous stream data
            int posBehindMarkedCont_global = markedContStartPos_global + matches[0].Length + 1; //position behind marekd content in contiguous stream data
            int startStreamIndex = -1;
            int endStreamIndex = -1;
            int markedContStartPos_local = -1;
            int posBehindMarkedCont_local = -1;
            int lenBeforeMarkedContent = -1;
            int lenBehindMarkedContent = -1;
            int offset = 0; //measured how far we move in the contiguous stream data when iterating over individual streams
            for(int i=0; i<arrDatasets.Length; i++) //Find the individual streams in which the marked content starts and ends
            {
                int datasetLen = arrDatasets[i].Length;
                if((offset +  datasetLen > markedContStartPos_global) && (startStreamIndex == -1))  //check if the marked content starts before this stream's end
                {                                                                                   //If that is the case, and this is the first such stream, then the marked content starts inside this stream
                    startStreamIndex = i;
                    markedContStartPos_local = markedContStartPos_global - offset;
                    lenBeforeMarkedContent = markedContStartPos_local;
                }
                if((offset + datasetLen >= posBehindMarkedCont_global) && (endStreamIndex == -1))   //check if the marked content ends before this stream's end
                {                                                                                   //If that is the case, and this is the first such stream, then the marked content ends inside this stream
                    endStreamIndex = i;
                    posBehindMarkedCont_local = posBehindMarkedCont_global - offset;
                    lenBehindMarkedContent = datasetLen - posBehindMarkedCont_local;
                    break;
                }
                offset += datasetLen;
            }
            if(startStreamIndex == endStreamIndex)  //If the marked content starts and ends within the same stream
            {
                byte[] markedStreamData = arrDatasets[startStreamIndex];    //all data of the stream with marked content
                int markedStreamDataLen = markedStreamData.Length;
                byte[] newStreamData = new byte[lenBeforeMarkedContent + lenBehindMarkedContent];   //the new stream data shall exclude the marked content, hence include only the data before and after the marked content.
                System.Array.Copy(markedStreamData,0,newStreamData,0,lenBeforeMarkedContent);       //copy the data before marked content from the original stream data to new stream data.
                System.Array.Copy(markedStreamData,posBehindMarkedCont_local,newStreamData,lenBeforeMarkedContent,lenBehindMarkedContent);  //copy the data behind the marked content from otiginal stream data to new stream data
                lstContents[startStreamIndex].setNewData(newStreamData);    //simply replace the old stream data (with marked content) with the new stream data (without marked content)
                return true;
            }
            else
            {
                byte[] startStreamData = arrDatasets[startStreamIndex]; //all data of the stream in which marked content starts
                byte[] endStreamData = arrDatasets[endStreamIndex];     //all data of the stream in which marked content ends
                byte[] newStartStreamData = new byte[lenBeforeMarkedContent];   //The stream where the marked content starts shall keep only its data before the marked content
                byte[] newEndStreamData = new byte[lenBehindMarkedContent];     //The stream where the marked content ends shall keep only its data behind the marked content *..
                System.Array.Copy(startStreamData,0,newStartStreamData,0,lenBeforeMarkedContent);
                System.Array.Copy(endStreamData,posBehindMarkedCont_local,newEndStreamData,0,lenBehindMarkedContent);
                lstContents[startStreamIndex].setNewData(newStartStreamData);
                lstContents[endStreamIndex].setNewData(newEndStreamData);
                for(int i=startStreamIndex+1; i<endStreamIndex; i++)
                {
                    lstContents[i].setNewData(new byte[0]); //*.. and all inbetween streams shall have no data at all, since all their data was purely marked content
                }
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
            string str1 = System.Text.RegularExpressions.Regex.Replace(str, "{pagecount}", document.numPages.ToString());
            string str2 = System.Text.RegularExpressions.Regex.Replace(str1, "{pagenum}", number.ToString());
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
