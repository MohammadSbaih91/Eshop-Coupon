using Nop.Core;
using Nop.Core.Data;
using Nop.Plugin.Widgets.AnywhereSlider.Domains;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Widgets.AnywhereSlider.Services
{
    public class AnywhereSliderService : IAnywhereSliderService
    {
        #region Fields
        private readonly IRepository<Accordingly> _accordinglyRepository;
        private readonly IRepository<Slider> _sliderRepository;
        private readonly IRepository<SliderGroup> _sliderGroupRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor
        public AnywhereSliderService(IRepository<Accordingly> accordinglyRepository,
            IRepository<Slider> sliderRepository,
            IEventPublisher eventPublisher,
            IRepository<SliderGroup> sliderGroupRepository)
        {
            _accordinglyRepository = accordinglyRepository;
            _sliderRepository = sliderRepository;
            _eventPublisher = eventPublisher;
            _sliderGroupRepository = sliderGroupRepository;
        }
        #endregion

        #region Methods
        #region Slider

        public IPagedList<Slider> GetAllSlider(string name, string widgetsPlace, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _sliderRepository.Table;

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.Contains(name));

            if (!string.IsNullOrEmpty(widgetsPlace))
                query = query.Where(p => p.Name.Contains(widgetsPlace));

            query = query.OrderBy(p => p.Name);

            var sliders = new PagedList<Slider>(query, pageIndex, pageSize);

            return sliders;
        }

        public Slider GetSliderById(int id)
        {
            if (id == 0)
                return null;

            return _sliderRepository.GetById(id);
        }

        public Slider GetSliderByWidgetsPlace(string widgetZone)
        {
            if (string.IsNullOrEmpty(widgetZone))
                return null;

            return _sliderRepository.Table.Where(p => p.WidgetZone == widgetZone && !p.Deleted && p.Published).FirstOrDefault();
        }

        public virtual void InsertSlider(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            _sliderRepository.Insert(slider);


            //event notification
            _eventPublisher.EntityInserted(slider);
        }

        public virtual void UpdateSlider(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            _sliderRepository.Update(slider);


            //event notification
            _eventPublisher.EntityUpdated(slider);
        }

        public virtual void DeleteSlider(Slider slider)
        {
            if (slider == null)
                throw new ArgumentNullException(nameof(slider));

            slider.Deleted = true;
            _sliderRepository.Update(slider);

            //event notification
            _eventPublisher.EntityDeleted(slider);
        }
        #endregion

        #region Accordingly Slider
        public IList<Accordingly> GetAccordinglyBySliderId(int sliderGroupId)
        {
            if (sliderGroupId == 0)
                return null;

            return _accordinglyRepository.Table.Where(p => p.SliderGroupId == sliderGroupId).OrderBy(p => p.DisplayOrder).ToList();
        }

        public Accordingly GetAccordinglyById(int id)
        {
            if (id == 0)
                return null;

            return _accordinglyRepository.GetById(id);
        }

        public virtual void InsertAccordingly(Accordingly accordingly)
        {
            if (accordingly == null)
                throw new ArgumentNullException(nameof(accordingly));

            _accordinglyRepository.Insert(accordingly);


            //event notification
            _eventPublisher.EntityInserted(accordingly);
        }

        public virtual void UpdateAccordingly(Accordingly accordingly)
        {
            if (accordingly == null)
                throw new ArgumentNullException(nameof(accordingly));

            _accordinglyRepository.Update(accordingly);


            //event notification
            _eventPublisher.EntityUpdated(accordingly);
        }

        public virtual void DeleteAccordingly(Accordingly accordingly)
        {
            if (accordingly == null)
                throw new ArgumentNullException(nameof(accordingly));

            _accordinglyRepository.Delete(accordingly);

            //event notification
            _eventPublisher.EntityDeleted(accordingly);
        }
        #endregion

        #region Slider Group
        public void InsertSliderGroup(SliderGroup sliderGroup)
        {
            if (sliderGroup == null)
                throw new ArgumentNullException(nameof(sliderGroup));

            _sliderGroupRepository.Insert(sliderGroup);

            //event notification
            _eventPublisher.EntityInserted(sliderGroup);
        }

        public void UpdateSliderGroup(SliderGroup sliderGroup)
        {
            if (sliderGroup == null)
                throw new ArgumentNullException(nameof(sliderGroup));

            _sliderGroupRepository.Update(sliderGroup);

            //event notification
            _eventPublisher.EntityUpdated(sliderGroup);
        }

        public void DeleteSliderGroup(SliderGroup sliderGroup)
        {
            if (sliderGroup == null)
                throw new ArgumentNullException(nameof(sliderGroup));

            sliderGroup.Deleted = true;
            _sliderGroupRepository.Update(sliderGroup);

            //event notification
            _eventPublisher.EntityUpdated(sliderGroup);
        }

        public SliderGroup GetsliderGroupById(int id)
        {
            if (id == 0)
                return null;

            return _sliderGroupRepository.Table.Where(p => p.Id == id && (p.Deleted ?? false) == false).FirstOrDefault();
        }

        public IList<SliderGroup> GetSliderGroupBySliderId(int sliderId)
        {
            if (sliderId == 0)
                return null;

            return _sliderGroupRepository.Table.Where(p => p.SliderId == sliderId && (p.Deleted ?? false) == false).OrderBy(p => p.DisplayOrder).ToList();
        }
        #endregion
        #endregion
    }
}
