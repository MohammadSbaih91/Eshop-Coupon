using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Events;

namespace Nop.Services.Customization.Common
{
    public partial class SearchTermServiceOverride : SearchTermService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<SearchTerm> _searchTermRepository;

        public SearchTermServiceOverride(IEventPublisher eventPublisher, IRepository<SearchTerm> searchTermRepository) : base(eventPublisher, searchTermRepository)
        {
            _eventPublisher = eventPublisher;
            _searchTermRepository = searchTermRepository;
        }

        #endregion
        //public SearchTermServiceOverride(IEventPublisher eventPublisher,
        // IRepository<SearchTerm> searchTermRepository)
        //{
        //    this._eventPublisher1 = eventPublisher;
        //    this._searchTermRepository1 = searchTermRepository;
        //}

        public override IPagedList<SearchTermReportLine> GetStats(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = (from st in _searchTermRepository.Table
                             // group st by st.Keyword into groupedResult
                             //select new
                             //{
                             //    Keyword = groupedResult.Key,
                             //    Count = groupedResult.Sum(o => o.Count)
                             //})

                         select new
                         {
                             Keyword = st.Keyword,
                             Count = st.Count
                         })
                        .OrderByDescending(m => m.Count)
                        .Select(r => new SearchTermReportLine
                        {
                            Keyword = r.Keyword,
                            Count = r.Count
                        });

            var result = new PagedList<SearchTermReportLine>(query, pageIndex, pageSize);
            return result;
        }

    }
}