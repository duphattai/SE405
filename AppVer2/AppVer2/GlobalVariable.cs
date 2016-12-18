using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace AppVer2
{
   public class GlobalVariable
    {
       static public StorageFile filecapture { get; set; }

       public GlobalVariable()
       {
           filecapture = null;
       }
       public GlobalVariable(StorageFile _filecapture)
       {
           _filecapture = filecapture;
       }
    }
}
