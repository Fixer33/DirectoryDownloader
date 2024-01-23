using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryDownloader
{
    [Serializable]
    public class DirectoryContent
    {
        public List<AdditionalFile> ToDownload;
        public List<DeleteFile> ToDelete;
    }

    [Serializable]
    public class AdditionalFile
    {
        public string Url;
        public string NameToSet;
        public string Sum;
    }

    [Serializable]
    public class DeleteFile
    {
        public string NameToDelete;
    }
}
