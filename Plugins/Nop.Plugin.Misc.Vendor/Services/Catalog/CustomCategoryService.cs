using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.Vendor.Services.Catalog
{
	public class CustomCategoryService : CategoryService, ICustomCategoryService
	{
		private readonly ICacheManager _cacheManager;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<ProductCategory> _productCategoryRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly IStoreMappingService _storeMappingService;
		private readonly IAclService _aclService;
		private const string PRODUCTCATEGORIES_ALLBYVENDORID_KEY = "Nop.productcategory.allbyvendorid-{0}-{1}-{2}-{3}";
		public CustomCategoryService(ICacheManager cacheManager,
			IRepository<Category> categoryRepository,
			IRepository<ProductCategory> productCategoryRepository,
			IRepository<Product> productRepository,
			IRepository<AclRecord> aclRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IWorkContext workContext,
			IStoreContext storeContext,
			IEventPublisher eventPublisher,
			IStoreMappingService storeMappingService,
			IAclService aclService,
			CatalogSettings catalogSettings) : base(cacheManager, categoryRepository, productCategoryRepository, productRepository, aclRepository, storeMappingRepository, workContext, storeContext, eventPublisher, storeMappingService, aclService, catalogSettings)
		{
			_cacheManager = cacheManager;
			_categoryRepository = categoryRepository;
			_productCategoryRepository = productCategoryRepository;
			_productRepository = productRepository;
			_storeMappingRepository = storeMappingRepository;
			_workContext = workContext;
			_storeContext = storeContext;
			_storeMappingService = storeMappingService;
			_aclService = aclService;
		}

		public string TestMethod()
		{
			throw new NotImplementedException();
		}
		public IList<ProductCategory> GetProductCategoriesByVendorId(int vendorId, bool showHidden = false)
		{
			return GetProductCategoriesByVendorId(vendorId, _storeContext.CurrentStore.Id, showHidden);
		}

		public IList<ProductCategory> GetProductCategoriesByVendorId(int vendorId, int storeId, bool showHidden = false)
		{
			if (vendorId == 0)
				return new List<ProductCategory>();

			string key = string.Format(PRODUCTCATEGORIES_ALLBYVENDORID_KEY, showHidden, vendorId, _workContext.CurrentCustomer.Id, storeId);
			return _cacheManager.Get(key, () =>
			{
				var query = from pc in _productCategoryRepository.Table
							join p in _productRepository.Table on pc.ProductId equals p.Id
							where p.VendorId == vendorId &&
								  !p.Deleted &&
								  (showHidden || p.Published)
							orderby pc.DisplayOrder
							select pc;

				var allProductCategories = query.ToList();
				var result = new List<ProductCategory>();
				if (!showHidden)
				{
					foreach (var pc in allProductCategories)
					{
						//ACL (access control list) and store mapping
						var category = pc.Category;
						if (_aclService.Authorize(category) && _storeMappingService.Authorize(category, storeId))
							result.Add(pc);
					}
				}
				else
				{
					//no filtering
					result.AddRange(allProductCategories);
				}
				return result;
			});
		}
	}
}
