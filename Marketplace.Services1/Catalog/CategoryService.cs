using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Marketplace.Services1.Catalog
{
	public  class CategoryService : Nop.Services.Catalog.CategoryService, ICategoryService
	{
		#region Constants

		/// <summary>
		/// Key for caching
		/// </summary>
		/// <remarks>
		/// {0} : show hidden records?
		/// {1} : category ID
		/// {2} : page index
		/// {3} : page size
		/// {4} : current customer ID
		/// {5} : store ID
		/// </remarks>
		private const string PRODUCTVENDORCATEGORIES_ALLBYCATEGORYIDANDVENDORID_KEY = "Nop.productvendorcategory.allbycategoryidAndVendorId-{0}-{1}-{2}-{3}-{4}-{5}-{6}";

		#endregion

		private readonly ICacheManager _cacheManager;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<ProductCategory> _productCategoryRepository;
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<AclRecord> _aclRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly CatalogSettings _catalogSettings;

		public CategoryService(ICacheManager cacheManager, IRepository<Category> categoryRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<Product> productRepository, IRepository<AclRecord> aclRepository, IRepository<StoreMapping> storeMappingRepository, IWorkContext workContext, IStoreContext storeContext, IEventPublisher eventPublisher, IStoreMappingService storeMappingService, IAclService aclService, CatalogSettings catalogSettings) : base(cacheManager, categoryRepository, productCategoryRepository, productRepository, aclRepository, storeMappingRepository, workContext, storeContext, eventPublisher, storeMappingService, aclService, catalogSettings)
		{
			_cacheManager = cacheManager;
			_categoryRepository = categoryRepository;
			_productCategoryRepository = productCategoryRepository;
			_productRepository = productRepository;
			_aclRepository = aclRepository;
			_storeMappingRepository = storeMappingRepository;
			_workContext = workContext;
			_storeContext = storeContext;
			_catalogSettings = catalogSettings;
		}

		public IPagedList<ProductCategory> GetProductVendorCategoriesByCategoryId(int categoryId, int vendorId, int pageIndex = 0,
			int pageSize = Int32.MaxValue, bool showHidden = false)
		{
			if (categoryId == 0)
				return new PagedList<ProductCategory>(new List<ProductCategory>(), pageIndex, pageSize);

			string key = string.Format(PRODUCTVENDORCATEGORIES_ALLBYCATEGORYIDANDVENDORID_KEY, showHidden, categoryId, vendorId, pageIndex, pageSize, _workContext.CurrentCustomer.Id, _storeContext.CurrentStore.Id);
			return _cacheManager.Get(key, () =>
			{
				var query = from pc in _productCategoryRepository.Table
							join p in _productRepository.Table on pc.ProductId equals p.Id
							where pc.CategoryId == categoryId &&
								  !p.Deleted &&
								  (showHidden || p.Published) &&
								  p.VendorId == vendorId
							orderby pc.DisplayOrder
							select pc;

				if (!showHidden && (!_catalogSettings.IgnoreAcl || !_catalogSettings.IgnoreStoreLimitations))
				{
					if (!_catalogSettings.IgnoreAcl)
					{
						//ACL (access control list)
						var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
						query = from pc in query
								join c in _categoryRepository.Table on pc.CategoryId equals c.Id
								join acl in _aclRepository.Table
								on new { c1 = c.Id, c2 = "Category" } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into c_acl
								from acl in c_acl.DefaultIfEmpty()
								where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
								select pc;
					}
					if (!_catalogSettings.IgnoreStoreLimitations)
					{
						//Store mapping
						var currentStoreId = _storeContext.CurrentStore.Id;
						query = from pc in query
								join c in _categoryRepository.Table on pc.CategoryId equals c.Id
								join sm in _storeMappingRepository.Table
								on new { c1 = c.Id, c2 = "Category" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into c_sm
								from sm in c_sm.DefaultIfEmpty()
								where !c.LimitedToStores || currentStoreId == sm.StoreId
								select pc;
					}
					//only distinct categories (group by ID)
					query = from c in query
							group c by c.Id
							into cGroup
							orderby cGroup.Key
							select cGroup.FirstOrDefault();
					query = query.OrderBy(pc => pc.DisplayOrder);
				}

				var productCategories = new PagedList<ProductCategory>(query, pageIndex, pageSize);
				return productCategories;
			});
		}
	}
}
