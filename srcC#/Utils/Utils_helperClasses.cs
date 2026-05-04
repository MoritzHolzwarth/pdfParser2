// using System.Linq;

// namespace pdfParserByMH
// {
//     public partial class Utils
//     {
//         public class specialArrayObRefDictionary
//         {
//             System.Collections.Generic.Dictionary<double[],pdfObjectReference> dict;
//             public specialArrayObRefDictionary()
//             {
//                 dict = new System.Collections.Generic.Dictionary<double[],pdfObjectReference>();
//             }
//             public void add(double[] key, pdfObjectReference val)
//             {
//                 dict.Add(key,val);
//             }
//             public bool containsKey(double[] arr, ref pdfObjectReference obRef)
//             {
//                 foreach(double[] key in dict.Keys)
//                 {
//                     if(key.SequenceEqual(arr))
//                     {
//                         if(obRef != null)
//                         {
//                             obRef = dict[key];
//                         }
//                         return true;
//                     }
//                 }
//                 return false;
//             }
//         }
//     }
// }
        