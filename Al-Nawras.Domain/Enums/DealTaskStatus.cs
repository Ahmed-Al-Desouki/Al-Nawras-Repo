using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Enums
{
    public enum DealTaskStatus   // named DealTaskStatus to avoid conflict with System.Threading.Tasks.Task
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }
}
