using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.Vendor.Services.Catalog
{
	public interface ICustomCategoryService : ICategoryService
	{
		string TestMethod();
		IList<ProductCategory> GetProductCategoriesByVendorId(int vendorId, bool showHidden = false);
	}
}
