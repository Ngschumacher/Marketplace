using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;

namespace Marketplace.Services1.Catalog
{
	interface ICategoryService :  Nop.Services.Catalog.ICategoryService
	{
		IPagedList<ProductCategory> GetProductVendorCategoriesByCategoryId(int categoryId, int vendorId,
		  int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
	}
}
