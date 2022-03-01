using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Widgets.CustomerOrderReview.Models
{
    public partial class CustomerOrderReviewSearchModel : BaseSearchModel
    {
        #region Ctor

        public CustomerOrderReviewSearchModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.CustomerOrderReview.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [NopResourceDisplayName("Plugins.CustomerOrderReview.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [NopResourceDisplayName("Plugins.CustomerOrderReview.SearchText")]
        public string SearchText { get; set; }

        [NopResourceDisplayName("Plugins.CustomerOrderReview.SearchStore")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Plugins.CustomerOrderReview.SearchOrder")]
        public int SearchOrderId { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        #endregion
    }
    
    public partial class CustomerOrderReviewListModel : BasePagedListModel<CustomerOrderReviewModel>
    {
    }
}