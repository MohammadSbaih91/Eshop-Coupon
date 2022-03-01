using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Events;

namespace Nop.Services.Catalog
{
    public class SpecificationAttributeGroupService : ISpecificationAttributeGroupService
    {
        #region Fields
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<SpecificationAttributeGroup> _specificationAttributeGroupRepository;

        #endregion

        #region Ctor
        #region Ctor

        public SpecificationAttributeGroupService(ICacheManager cacheManager,
            IEventPublisher eventPublisher,
            IRepository<SpecificationAttributeGroup> specificationAttributeGroupRepository)
        {
            _cacheManager = cacheManager;
            _eventPublisher = eventPublisher;
            _specificationAttributeGroupRepository = specificationAttributeGroupRepository;
        }

        #endregion
        #endregion

        #region Methods

        /// <summary>
        /// Gets a specification attribute grop
        /// </summary>
        /// <param name="SpecificationAttributeGroupId">The specification attribute group identifier</param>
        /// <returns>Specification attribute</returns>
        public virtual SpecificationAttributeGroup GetSpecificationAttributeGroupById(int SpecificationAttributeGroupId)
        {
            if (SpecificationAttributeGroupId == 0)
                return null;

            return _specificationAttributeGroupRepository.GetById(SpecificationAttributeGroupId);
        }

        /// <summary>
        /// Gets specification attributes group
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Specification attribute groups</returns>
        public virtual IPagedList<SpecificationAttributeGroup> GetSpecificationAttributeGroups(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from sa in _specificationAttributeGroupRepository.Table
                        orderby sa.DisplayOrder, sa.Id
                        select sa;
            var specificationAttributeGroups = new PagedList<SpecificationAttributeGroup>(query, pageIndex, pageSize);
            return specificationAttributeGroups;
        }

        /// <summary>
        /// Deletes a specification attribute group
        /// </summary>
        /// <param name="specificationAttributeGroup">The specification attribute group</param>
        public virtual void DeleteSpecificationAttributeGroup(SpecificationAttributeGroup specificationAttributeGroup)
        {
            if (specificationAttributeGroup == null)
                throw new ArgumentNullException(nameof(specificationAttributeGroup));

            _specificationAttributeGroupRepository.Delete(specificationAttributeGroup);

            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductSpecificationAttributePatternCacheKey);

            //event notification
            _eventPublisher.EntityDeleted(specificationAttributeGroup);
        }

        /// <summary>
        /// Inserts a specification attribute group
        /// </summary>
        /// <param name="specificationAttributeGroup">The specification attribute group</param>
        public virtual void InsertSpecificationAttributeGroup(SpecificationAttributeGroup specificationAttributeGroup)
        {
            if (specificationAttributeGroup == null)
                throw new ArgumentNullException(nameof(specificationAttributeGroup));

            _specificationAttributeGroupRepository.Insert(specificationAttributeGroup);

            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductSpecificationAttributePatternCacheKey);

            //event notification
            _eventPublisher.EntityInserted(specificationAttributeGroup);
        }

        /// <summary>
        /// Updates the specification attribute group
        /// </summary>
        /// <param name="specificationAttributeGroup">The specification attribute group</param>
        public virtual void UpdateSpecificationAttributeGroup(SpecificationAttributeGroup specificationAttributeGroup)
        {
            if (specificationAttributeGroup == null)
                throw new ArgumentNullException(nameof(specificationAttributeGroup));

            _specificationAttributeGroupRepository.Update(specificationAttributeGroup);

            _cacheManager.RemoveByPattern(NopCatalogDefaults.ProductSpecificationAttributePatternCacheKey);

            //event notification
            _eventPublisher.EntityUpdated(specificationAttributeGroup);
        }
        
        #endregion
    }
}
