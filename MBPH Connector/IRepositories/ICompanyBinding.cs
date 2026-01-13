using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MBPH_Connector.IRepositories
{
    public interface ICompanyBinding//ICompanyBinding
    {
        Task BindCompany();
        Task<bool> IsValidCompany();
        
    }
}
