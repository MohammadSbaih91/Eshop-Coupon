using Nop.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Catalog
{
    public interface IPackagesService
    {
        Packages GetPackagesById(int Id);

        Packages GetPackagesByName(string name);

        IPagedList<Packages> GetPackages(string name = "", int pageIndex = 0, int pageSize = int.MaxValue);

        IList<Packages> GetPackagesList();

        IList<Packages> GetPackagesByCategoryId(int categoryId);

        void DeletePackages(Packages packages);

        void InsertPackages(Packages packages);

        void UpdatePackages(Packages packages);
    }
}
