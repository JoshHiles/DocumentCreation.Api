using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentCreation.Api.Helpers
{
    public interface IFileHelper
    {
        void StreamToFile(string fileSavePath, string fileName, string fileExt, MemoryStream stream);
    }

    public class FileHelper : IFileHelper
    {
        public void StreamToFile(string fileSavePath, string fileName, string fileExt, MemoryStream stream)
        {
            var fileDetails = string.Format(@"{0}\\{1}.{2}", fileSavePath, fileName, fileExt);
            var file = new FileStream(fileDetails, FileMode.Create, FileAccess.Write);

            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            file.Write(bytes, 0, bytes.Length);
            stream.Close();
            file.Close();
        }
    }
}
