using Microsoft.AspNetCore.Html;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Events;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Encodings.Web;
using Nop.Web.Framework.Extensions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Core.Domain.Topics;
using Nop.Core.Infrastructure;

namespace Nop.Web.Areas.Admin.Customization.Events
{
    public class EventConsumer : IConsumer<EntityDeletedEvent<Category>>,
        IConsumer<EntityInsertedEvent<Category>>,
        IConsumer<EntityUpdatedEvent<Category>>,
        IConsumer<EntityDeletedEvent<Product>>,
        IConsumer<EntityInsertedEvent<Product>>,
        IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityDeletedEvent<Manufacturer>>,
        IConsumer<EntityInsertedEvent<Manufacturer>>,
        IConsumer<EntityUpdatedEvent<Manufacturer>>,
        IConsumer<EntityDeletedEvent<Topic>>,
        IConsumer<EntityInsertedEvent<Topic>>,
        IConsumer<EntityUpdatedEvent<Topic>>
    {
        private readonly INopFileProvider _fileProvider;
        public EventConsumer(INopFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        private void DeleteXmlFile()
        {
            var arfilePath = _fileProvider.GetAbsolutePath("sitemap", $"sitemap.ar.xml");
            var enfilePath = _fileProvider.GetAbsolutePath("sitemap", $"sitemap.en.xml");
            
            if (_fileProvider.FileExists(arfilePath))
                _fileProvider.DeleteFile(arfilePath);

            if (_fileProvider.FileExists(enfilePath))
                _fileProvider.DeleteFile(enfilePath);

        }

        public void HandleEvent(EntityDeletedEvent<Category> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityInsertedEvent<Category> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityUpdatedEvent<Category> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityDeletedEvent<Product> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityInsertedEvent<Product> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityUpdatedEvent<Product> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityDeletedEvent<Manufacturer> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityInsertedEvent<Manufacturer> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityInsertedEvent<Topic> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityDeletedEvent<Topic> eventMessage)
        {
            DeleteXmlFile();
        }

        public void HandleEvent(EntityUpdatedEvent<Topic> eventMessage)
        {
            DeleteXmlFile();
        }
    }
}
