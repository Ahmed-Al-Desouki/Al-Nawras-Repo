using Al_Nawras.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Application.Common.Interfaces
{
    public interface IOverduePaymentJob
    {
        Task<OverduePaymentJobResult> RunAsync(CancellationToken cancellationToken = default);
    }
}
