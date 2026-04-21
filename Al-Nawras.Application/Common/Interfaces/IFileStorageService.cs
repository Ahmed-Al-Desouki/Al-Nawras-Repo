using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces
{
    public interface IFileStorageService
    {
        // Saves the file and returns the relative storage path
        Task<string> SaveAsync(
            Stream fileStream,
            string fileName,
            string folder,
            CancellationToken cancellationToken = default);

        void Delete(string storagePath);
    }
}
