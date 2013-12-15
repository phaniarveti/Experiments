﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Events;
using Umbraco.Core.Models;
using Umbraco.Core.Publishing;
using umbraco;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.presentation.cache;

namespace Umbraco.Web.Strategies.Publishing
{
    /// <summary>
    /// Represents the UpdateCacheAfterPublish class, which subscribes to the Published event
    /// of the <see cref="PublishingStrategy"/> class and is responsible for doing the actual
    /// cache refresh after a content item has been published.
    /// </summary>
    /// <remarks>
    /// This implementation is meant as a seperation of the cache refresh from the ContentService
    /// and PublishingStrategy.
    /// This event subscriber will only be relevant as long as there is an xml cache.
    /// </remarks>
    public class UpdateCacheAfterPublish : IApplicationStartupHandler
    {
        public UpdateCacheAfterPublish()
        {
            PublishingStrategy.Published += PublishingStrategy_Published;
        }

        void PublishingStrategy_Published(IPublishingStrategy sender, PublishEventArgs<IContent> e)
        {
            if (e.PublishedEntities.Any())
            {
                if (e.IsAllRepublished)
                {
                    UpdateEntireCache();
                    return;
                }

                if (e.PublishedEntities.Count() > 1)
                {
                    UpdateMultipleContentCache(e.PublishedEntities);
                }
                else
                {
                    var content = e.PublishedEntities.FirstOrDefault();
                    UpdateSingleContentCache(content);
                }
            }
        }

        /// <summary>
        /// Refreshes the xml cache for all nodes
        /// </summary>
        private void UpdateEntireCache()
        {
            if (UmbracoSettings.UseDistributedCalls)
            {
                dispatcher.RefreshAll(new Guid("27ab3022-3dfa-47b6-9119-5945bc88fd66"));
            }
            else
            {
                content.Instance.RefreshContentFromDatabaseAsync();
            }
        }

        /// <summary>
        /// Refreshes the xml cache for nodes in list
        /// </summary>
        private void UpdateMultipleContentCache(IEnumerable<IContent> content)
        {
            if (UmbracoSettings.UseDistributedCalls)
            {
                foreach (var c in content)
                {
                    dispatcher.Refresh(new Guid("27ab3022-3dfa-47b6-9119-5945bc88fd66"), c.Id);
                }
            }
            else
            {
                var documents = content.Select(x => new Document(x)).ToList();
                global::umbraco.content.Instance.UpdateDocumentCache(documents);
            }
        }

        /// <summary>
        /// Refreshes the xml cache for a single node
        /// </summary>
        private void UpdateSingleContentCache(IContent content)
        {
            if (UmbracoSettings.UseDistributedCalls)
            {
                dispatcher.Refresh(new Guid("27ab3022-3dfa-47b6-9119-5945bc88fd66"), content.Id);
            }
            else
            {
                var doc = new Document(content);
                global::umbraco.content.Instance.UpdateDocumentCache(doc);
            }
        }
    }
}