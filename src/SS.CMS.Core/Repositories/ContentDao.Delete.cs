﻿using System.Collections.Generic;
using SS.CMS.Core.Cache.Content;
using SS.CMS.Core.Common;
using SS.CMS.Plugin.Data;
using Attr = SS.CMS.Core.Models.Attributes.ContentAttribute;

namespace SS.CMS.Core.Repositories
{
    public partial class ContentDao
    {
        public void Delete(int siteId, int contentId)
        {
            if (siteId <= 0 || contentId <= 0) return;

            _repository.Delete(Q
                .Where(Attr.SiteId, siteId)
                .Where(Attr.Id, contentId)
            );
        }

        private void DeleteReferenceContents(int siteId, int channelId, IList<int> contentIdList)
        {
            var deleteNum = 0;

            if (contentIdList != null && contentIdList.Count > 0)
            {
                TagUtils.RemoveTags(siteId, contentIdList);

                deleteNum = _repository.Delete(Q
                    .Where(Attr.SiteId, siteId)
                    .Where(Attr.ReferenceId, ">", 0)
                    .WhereIn(Attr.Id, contentIdList));
            }

            if (deleteNum <= 0) return;

            ContentManager.RemoveCache(TableName, channelId);
        }
    }
}