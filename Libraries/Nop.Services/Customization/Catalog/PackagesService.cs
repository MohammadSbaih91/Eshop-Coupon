using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Catalog
{
    public class PackagesService : IPackagesService
    {
        #region Fields
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Packages> _packagesRepository;
        #endregion

        #region Ctor
        public PackagesService(IEventPublisher eventPublisher,
            IRepository<Packages> packagesRepository)
        {
            _eventPublisher = eventPublisher;
            _packagesRepository = packagesRepository;
        }
        #endregion

        #region Methods
        public virtual Packages GetPackagesById(int Id)
        {
            if(Id == 0)
                return null;

            return _packagesRepository.GetById(Id);
        }

        public virtual Packages GetPackagesByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return _packagesRepository.Table.Where(p => p.Name == name).FirstOrDefault();
        }

        public virtual IPagedList<Packages> GetPackages(string name = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _packagesRepository.Table;

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.Contains(name));

            query = from sa in query
                    where !sa.Deleted && sa.Published
                    orderby sa.DisplayOrder, sa.Id
                    select sa;
            return new PagedList<Packages>(query, pageIndex, pageSize);
        }

        public virtual IList<Packages> GetPackagesList()
        {
            var query = _packagesRepository.Table;

            query = from sa in query
                    where !sa.Deleted && sa.Published
                    orderby sa.DisplayOrder, sa.Id
                    select sa;

            return new List<Packages>(query);
        }

        public virtual IList<Packages> GetPackagesByCategoryId(int categoryId)
        {
            var query = _packagesRepository.Table;
            var catId = Convert.ToString(categoryId);
            query = from sa in query
                    where !sa.Deleted && sa.Published && sa.CategoryIdList.Contains(categoryId)
                    orderby sa.DisplayOrder, sa.Id
                    select sa;

            return new List<Packages>(query);
        }

        public virtual void DeletePackages(Packages packages)
        {
            if (packages == null)
                throw new ArgumentNullException(nameof(packages));

            packages.Deleted = true;
            UpdatePackages(packages);

            //event notification
            _eventPublisher.EntityDeleted(packages);
        }

        public virtual void InsertPackages(Packages packages)
        {
            if (packages == null)
                throw new ArgumentNullException(nameof(packages));

            _packagesRepository.Insert(packages);

            //event notification
            _eventPublisher.EntityInserted(packages);
        }

        public virtual void UpdatePackages(Packages packages)
        {
            if (packages == null)
                throw new ArgumentNullException(nameof(packages));

            _packagesRepository.Update(packages);

            //event notification
            _eventPublisher.EntityUpdated(packages);
        }
        #endregion
    }
}
