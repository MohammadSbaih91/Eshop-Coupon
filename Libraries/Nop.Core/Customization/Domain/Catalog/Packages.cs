using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace Nop.Core.Domain.Catalog
{
    public class Packages : BaseEntity
    {
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public string CategoryIds { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }

        [NotMapped]
        public List<int> CategoryIdList
        {
            get {
                if (string.IsNullOrEmpty(this.CategoryIds))
                    return new List<int>();
                else
                    return this.CategoryIds.Split(",").Select(int.Parse).ToList();
            }
        }
    }
}
