using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FluffyBunny.Admin.Services
{
    public interface IPagingHelper
    {
        List<SelectListItem> GetPagingSizeOptions();
        int ValidatePageSize(int? pageSize);

    }
}
