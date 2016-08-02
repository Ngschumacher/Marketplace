using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Controllers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.Vendor.Controllers
{
	public class TestController : BasePluginController
    {
	    private readonly CatalogSettings _catalogSettings;
	    private readonly IProductService _productService;
	    private readonly ICategoryService _categoryService;
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly ICacheManager _cacheManager;

		//public CatalogController(ICategoryService categoryService, IManufacturerService manufacturerService, IProductService productService, IVendorService vendorService, ICategoryTemplateService categoryTemplateService, IManufacturerTemplateService manufacturerTemplateService, IWorkContext workContext, IStoreContext storeContext, ITaxService taxService, ICurrencyService currencyService, IPictureService pictureService, ILocalizationService localizationService, IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter, IWebHelper webHelper, ISpecificationAttributeService specificationAttributeService, IProductTagService productTagService, IGenericAttributeService genericAttributeService, IAclService aclService, IStoreMappingService storeMappingService, IPermissionService permissionService, ICustomerActivityService customerActivityService, ITopicService topicService, IEventPublisher eventPublisher, ISearchTermService searchTermService, MediaSettings mediaSettings, CatalogSettings catalogSettings, VendorSettings vendorSettings, BlogSettings blogSettings, ForumSettings forumSettings, ICacheManager cacheManager) : base(categoryService, manufacturerService, productService, vendorService, categoryTemplateService, manufacturerTemplateService, workContext, storeContext, taxService, currencyService, pictureService, localizationService, priceCalculationService, priceFormatter, webHelper, specificationAttributeService, productTagService, genericAttributeService, aclService, storeMappingService, permissionService, customerActivityService, topicService, eventPublisher, searchTermService, mediaSettings, catalogSettings, vendorSettings, blogSettings, forumSettings, cacheManager)
		//{
		//	_categoryService = categoryService;
		//	_workContext = workContext;
		//	_storeContext = storeContext;
		//	_cacheManager = cacheManager;
		//}
	    public TestController(ICacheManager cacheManager,CatalogSettings catalogSettings, ICategoryService categoryService, IWorkContext workContext, IStoreContext storeContext, IProductService productService) 
	    {
	        _cacheManager = cacheManager;
	        _catalogSettings = catalogSettings;
	        _productService = productService;
            _categoryService = categoryService;
            _workContext = workContext;
            _storeContext = storeContext;
            _cacheManager = cacheManager;
        }

        public ActionResult Test(string widgetArea, object additionalData)
		{
            var categoryNavigationModel = new CategoryNavigationModel();

		    int currentCategoryId = (int)additionalData.GetType().GetProperty("currentCategoryId").GetValue(additionalData, null); ;
		    int currentProductId = (int)additionalData.GetType().GetProperty("currentProductId").GetValue(additionalData, null);
		    int currentVendorId = (int)additionalData.GetType().GetProperty("currentVendorId").GetValue(additionalData, null);

		    //return View(categoryNavigationModel);

            //get active category

            int activeCategoryId = 0;
            if (currentCategoryId > 0)
            {
                //category details page
                activeCategoryId = currentCategoryId;
            }
            else if (currentProductId > 0)
            {
                //product details page
                var productCategories = _categoryService.GetProductCategoriesByProductId(currentProductId);
                if (productCategories.Count > 0)
                    activeCategoryId = productCategories[0].CategoryId;
            }
            else if (currentVendorId > 0)
            {
                //IList<ProductCategory> vendorCategories = _categoryService.GetProductCategoriesByVendorId(currentVendorId);
            }
            string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NAVIGATION_MODEL_KEY,
                _workContext.WorkingLanguage.Id,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () => PrepareCategorySimpleModels(0).ToList());

            var model = new CategoryNavigationModel
            {
                CurrentCategoryId = activeCategoryId,
                Categories = cachedModel
            };

            return View("~/Plugins/Misc.Vendor/Views/Test/Test.cshtml", model);
        }
        /// <summary>
        /// Prepare category (simple) models
        /// </summary>
        /// <param name="rootCategoryId">Root category identifier</param>
        /// <param name="loadSubCategories">A value indicating whether subcategories should be loaded</param>
        /// <param name="allCategories">All available categories; pass null to load them internally</param>
        /// <returns>Category models</returns>
        [NonAction]
        protected virtual IList<CategorySimpleModel> PrepareCategorySimpleModels(int rootCategoryId,
            bool loadSubCategories = true, IList<Category> allCategories = null)
        {
            var result = new List<CategorySimpleModel>();

            //little hack for performance optimization.
            //we know that this method is used to load top and left menu for categories.
            //it'll load all categories anyway.
            //so there's no need to invoke "GetAllCategoriesByParentCategoryId" multiple times (extra SQL commands) to load childs
            //so we load all categories at once
            //if you don't like this implementation if you can uncomment the line below (old behavior) and comment several next lines (before foreach)
            //var categories = _categoryService.GetAllCategoriesByParentCategoryId(rootCategoryId);
            if (allCategories == null)
            {
                //load categories if null passed
                //we implemeneted it this way for performance optimization - recursive iterations (below)
                //this way all categories are loaded only once
                allCategories = _categoryService.GetAllCategories();
            }
            var categories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).ToList();
            foreach (var category in categories)
            {
                var categoryModel = new CategorySimpleModel
                {
                    Id = category.Id,
                    Name = category.GetLocalized(x => x.Name),
                    SeName = category.GetSeName(),
                    IncludeInTopMenu = category.IncludeInTopMenu
                };

                //product number for each category
                if (_catalogSettings.ShowCategoryProductNumber)
                {
                    string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY,
                        string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                        _storeContext.CurrentStore.Id,
                        category.Id);
                    categoryModel.NumberOfProducts = _cacheManager.Get(cacheKey, () =>
                    {
                        var categoryIds = new List<int>();
                        categoryIds.Add(category.Id);
                        //include subcategories
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            categoryIds.AddRange(GetChildCategoryIds(category.Id));
                        return _productService.GetCategoryProductNumber(categoryIds, _storeContext.CurrentStore.Id);
                    });
                }

                if (loadSubCategories)
                {
                    var subCategories = PrepareCategorySimpleModels(category.Id, loadSubCategories, allCategories);
                    categoryModel.SubCategories.AddRange(subCategories);
                }
                result.Add(categoryModel);
            }

            return result;
        }
        [NonAction]
        protected virtual List<int> GetChildCategoryIds(int parentCategoryId)
        {
            string cacheKey = string.Format(ModelCacheEventConsumer.CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY,
                parentCategoryId,
                string.Join(",", _workContext.CurrentCustomer.GetCustomerRoleIds()),
                _storeContext.CurrentStore.Id);
            return _cacheManager.Get(cacheKey, () =>
            {
                var categoriesIds = new List<int>();
                var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId);
                foreach (var category in categories)
                {
                    categoriesIds.Add(category.Id);
                    categoriesIds.AddRange(GetChildCategoryIds(category.Id));
                }
                return categoriesIds;
            });
        }
    }
}
