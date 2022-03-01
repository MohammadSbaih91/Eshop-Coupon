using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Data.Mapping.Catalog;
using Nop.Services.Events;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Manufacturer service
    /// </summary>
    public partial class ManufacturerService 
    {
        
        #region Methods

        public virtual IPagedList<Manufacturer> GetAllManufacturersByCategoryId(string manufacturerName = "",
            int storeId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false, int categoryId = 0)
        {
            var query = _manufacturerRepository.Table;
            if (!showHidden)
                query = query.Where(m => m.Published);
            if (!string.IsNullOrWhiteSpace(manufacturerName))
                query = query.Where(m => m.Name.Contains(manufacturerName));
            query = query.Where(m => !m.Deleted);

            #region Customize category wise manufacturer filter

            if (categoryId != 0)
            {
                var productCategoryRepository = EngineContext.Current.Resolve<IRepository<ProductCategory>>();
                var categoryManufacturerIds =
                    (from pc in productCategoryRepository.TableNoTracking.Where(m => m.CategoryId == categoryId)
                        from pm in _productManufacturerRepository.TableNoTracking.Where(
                            m => m.ProductId == pc.ProductId)
                        select pm.ManufacturerId).ToArray();

                query = query.Where(x => categoryManufacturerIds.Contains(x.Id));
            }

            #endregion

            query = query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id);

            if ((storeId <= 0 || _catalogSettings.IgnoreStoreLimitations) && (showHidden || _catalogSettings.IgnoreAcl))
                return new PagedList<Manufacturer>(query, pageIndex, pageSize);

            if (!showHidden && !_catalogSettings.IgnoreAcl)
            {
                //ACL (access control list)
                var allowedCustomerRolesIds = _workContext.CurrentCustomer.GetCustomerRoleIds();
                query = from m in query
                    join acl in _aclRepository.Table
                        on new {c1 = m.Id, c2 = _entityName} equals new {c1 = acl.EntityId, c2 = acl.EntityName} into
                        m_acl
                    from acl in m_acl.DefaultIfEmpty()
                    where !m.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                    select m;
            }

            if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
            {
                //Store mapping
                query = from m in query
                    join sm in _storeMappingRepository.Table
                        on new {c1 = m.Id, c2 = _entityName} equals new {c1 = sm.EntityId, c2 = sm.EntityName} into m_sm
                    from sm in m_sm.DefaultIfEmpty()
                    where !m.LimitedToStores || storeId == sm.StoreId
                    select m;
            }

            query = query.Distinct().OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id);

            return new PagedList<Manufacturer>(query, pageIndex, pageSize);
        }

        #endregion
    }
}