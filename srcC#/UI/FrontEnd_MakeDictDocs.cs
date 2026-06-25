using System.Collections.Generic;

namespace pdfParserByMH
{
    public class DocData
    {
        public Dictionary<DocPropertyType,object> dictDocProperties {get; set;}
        public string filePath
        {
            get {return (string)dictDocProperties[DocPropertyType.FilePath];}
            set {dictDocProperties[DocPropertyType.FilePath] = value;}
        }
        public string folderName
        {
            get {return (string)dictDocProperties[DocPropertyType.FolderName];}
            set {dictDocProperties[DocPropertyType.FolderName] = value;}
        }
        public string fileApproxName
        {
            get {return (string)dictDocProperties[DocPropertyType.ApproxFileName];}
            set {dictDocProperties[DocPropertyType.ApproxFileName] = value;}
        }
        public string header
        {
            get {return (string)dictDocProperties[DocPropertyType.Header];}
            set {dictDocProperties[DocPropertyType.Header] = value;}
        }
        public string footer
        {
            get {return (string)dictDocProperties[DocPropertyType.Footer];}
            set {dictDocProperties[DocPropertyType.Footer] = value;}
        }
        public PageXPosition headerXPos
        {
            get {return (PageXPosition)dictDocProperties[DocPropertyType.HeaderXPos];}
            set {dictDocProperties[DocPropertyType.HeaderXPos] = value;}
        }
        public PageXPosition footerXPos
        {
            get {return (PageXPosition)dictDocProperties[DocPropertyType.FooterXPos];}
            set {dictDocProperties[DocPropertyType.FooterXPos] = value;}
        }
        public PageQuantifiers vertPagesQuantifier
        {
            get {return (PageQuantifiers)dictDocProperties[DocPropertyType.VertPageQuant];}
            set {dictDocProperties[DocPropertyType.VertPageQuant] = value;}
        }
        public PageQuantifiers horiPagesQuantifier
        {
            get {return (PageQuantifiers)dictDocProperties[DocPropertyType.HoriPageQuant];}
            set {dictDocProperties[DocPropertyType.HoriPageQuant] = value;}
        }
        public int[] vertPagesNumbers
        {
            get {return (int[])dictDocProperties[DocPropertyType.VertPageNumbers];}
            set {dictDocProperties[DocPropertyType.VertPageNumbers] = value;}
        }
        public int[] horiPagesNumbers
        {
            get {return (int[])dictDocProperties[DocPropertyType.HoriPageNumbers];}
            set {dictDocProperties[DocPropertyType.HoriPageNumbers] = value;}
        }
        public double scaleFactor
        {
            get {return (double)dictDocProperties[DocPropertyType.ScaleFactor];}
            set {dictDocProperties[DocPropertyType.ScaleFactor] = value;}
        }
        public double[] textColor
        {
            get {return (double[])dictDocProperties[DocPropertyType.TextColor];}
            set {dictDocProperties[DocPropertyType.TextColor] = value;}
        }
        public bool selected;
        public bool contentHasChanged {get; set;}
        public DocData(string folder = "", string file = "")
        {
            dictDocProperties = new Dictionary<DocPropertyType, object>();
            folderName = folder;
            fileApproxName = file;
            contentHasChanged = false;
            selected = false;
        }
    }

    public partial class FrontEndProgram
    {
        private void makeDictDocs()
        {
            dictDocs = new Dictionary<string, DocData>();
            fillDictDocsFromDefault();
        }

        private void fillDictDocsFromDefault()
        {   
            dictDocs.Add("Abfalldatenblatt",new DocData("A", "A. ADB APG0*.pdf"));
            dictDocs.Add("Repräs. Begleitschein",new DocData("B.1", "B.1 Repräsentative* Begleitschein*_Rev.*.pdf"));
            dictDocs.Add("Verarbeitungsweg",new DocData("B.1", "B.1 Verarbeitungsweg*_Rev.*.pdf"));
            dictDocs.Add("Deklarationsvorschriften",new DocData("B.1", "B.1 Deklarationsvorschrift*_Rev.*.pdf"));
            dictDocs.Add("Gamma-Analyse",new DocData("B.1", "B.1 Gamma-Analyse*_Rev.*.pdf"));
            dictDocs.Add("LSC-Analyse",new DocData("B.1", "B.1 LSC-Analyse*_Rev.*.pdf"));
            dictDocs.Add("Chargenfreigabe",new DocData("C.1", "C.1 Chargenfreigabe*_Rev.*.pdf"));
            dictDocs.Add("Fassmessprotokolle",new DocData("C.1", "C.1 Fassmessprotokoll*_Rev.*.pdf"));
            dictDocs.Add("Gasphasenanalyse",new DocData("C.1", "C.1 Gasphasenanalyse*_Rev.*.pdf"));
            dictDocs.Add("Prüfprotokoll",new DocData("C.1", "C.1 Prüfprotokoll*_Rev.*.pdf"));
            dictDocs.Add("Trocknungsprotokolle",new DocData("C.1", "C.1 Trocknungsprotokoll*_Rev.*.pdf"));
            dictDocs.Add("Trocknungsnachweis",new DocData("C.1", "C.1 Nachweis Trocknungserfolg*_Rev.*.pdf"));
            dictDocs.Add("Wiegescheine",new DocData("C.1", "C.1 Wiegeschein*.pdf"));
            dictDocs.Add("Fotodokumentation",new DocData("C.1", "C.1 Fotodokumentation_Rev.*.pdf"));
            dictDocs.Add("Inspektionsbericht",new DocData("C.1", "C.1 Inspektionsbericht*_Rev.*.pdf"));
            dictDocs.Add("Verdampferkonzentratanalyse",new DocData("C.1", "C.1 Analyse* Verdampferkonzentrat*_Rev.*.pdf"));
            dictDocs.Add("Klärschlammanalyse",new DocData("C.1", "C.1 Klärschlamm-Analyse* (FSC)_Rev.*.pdf"));
            dictDocs.Add("PSC-Zerlegung",new DocData("C.1", "C.1 PSC-Zerlegung_Rev.*.pdf"));
            dictDocs.Add("Ablaufplan",new DocData("C.2", "C.2 Ablaufpl*n*_Rev.*.pdf"));
            dictDocs.Add("Querkontamination",new DocData("D.2", "D.2 Querkontamination_Rev.*.pdf"));
            dictDocs.Add("Gammamessung Rados",new DocData("D.2", "D.2 Gammamessung* Rados_Rev.*.pdf"));
            dictDocs.Add("Prüfbericht Druckfestigkeit",new DocData("D.2", "D.2 Prüfbericht* Druckfestigkeit_Rev.*.pdf"));
            dictDocs.Add("Prüfbericht Zementierfähigkeit",new DocData("D.2", "D.2 Prüfbericht* Zementierfähigkeit_Rev.*.pdf"));
            dictDocs.Add("TNE-Stellungnahme NDKL",new DocData("D.2", "D.2 TNE-Stellungnahme* NDKL*_Rev.*.pdf"));
            dictDocs.Add("Verifizierung NDKL C-Altchargen",new DocData("D.2", "D.2 Verifizierung der NDKL der C-Altchargen_Rev.*.pdf"));
            dictDocs.Add("Behälterabnahme",new DocData("D.3", "D.3 Abnahme Behälter_Rev.*.pdf"));
            dictDocs.Add("Reparaturen",new DocData("D.3", "D.3 Reparaturen_Rev.*.pdf"));
            
            foreach(KeyValuePair<string,DocData> kvp in dictDocs)
            {
                if(kvp.Key == "Abfalldatenblatt")
                {
                    kvp.Value.header = string.Format("A. ADB Rev.{0},\r\nDoku-ID: {1}: Rev.{2}", ADBRev_placeholder, dokuID_placeholder, DokuRev_placeholder);  //Note: This text may be written into a Windows.TextBox,
                    kvp.Value.footer = "Das Original ist an dieser Stelle rot gestempelt.";                                                                     //which requires \r\n for line break, not just \n!
                    kvp.Value.headerXPos = PageXPosition.Middle;
                    kvp.Value.footerXPos = PageXPosition.Left;
                    kvp.Value.vertPagesQuantifier = PageQuantifiers.NoneExceptArray;
                    kvp.Value.vertPagesNumbers = new int[] {1, 2};
                    kvp.Value.horiPagesQuantifier = PageQuantifiers.NoneExceptArray;
                    kvp.Value.horiPagesNumbers = new int[] {};
                    kvp.Value.textColor = new double[] {1, 0, 0};
                    kvp.Value.scaleFactor = 0.9;
                }
                else
                {
                    kvp.Value.header = "";
                    kvp.Value.footer = "Seite {pagenum} von {pagecount}";
                    kvp.Value.headerXPos = PageXPosition.Middle;
                    kvp.Value.footerXPos = PageXPosition.Right;
                    kvp.Value.vertPagesQuantifier = PageQuantifiers.NoneExceptArray;
                    kvp.Value.vertPagesNumbers = new int[] {};
                    kvp.Value.horiPagesQuantifier = PageQuantifiers.NoneExceptArray;
                    kvp.Value.horiPagesNumbers = new int[] {};
                    kvp.Value.textColor = new double[] {0, 0, 0};
                    kvp.Value.scaleFactor = 0.9;
                }
            }
        }
    }
}