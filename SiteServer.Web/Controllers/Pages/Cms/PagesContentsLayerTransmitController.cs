using System;
using System.Collections.Generic;
using System.Web.Http;
using SiteServer.CMS.Core;
using SiteServer.CMS.Core.Create;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.DataCache.Content;
using SiteServer.CMS.Model.Enumerations;
using SiteServer.CMS.Plugin.Impl;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Pages.Cms
{
    /// <summary>
    /// 批量跨站转发
    /// </summary>
    [RoutePrefix("pages/cms/contentsLayerTransmit")]
    public class PagesContentsLayerTransmitController : ApiController
    {
        private const string Route = "";
        private const string RouteGetChannels = "actions/getChannels";

        //[HttpGet, Route(Route)]
        //public IHttpActionResult GetConfig()
        //{
        //    try
        //    {
        //        var request = new AuthenticatedRequest();

        //        var siteId = request.GetQueryInt("siteId");
        //        var channelId = request.GetQueryInt("channelId");
        //        var contentIdList = TranslateUtils.StringCollectionToIntList(request.GetQueryString("contentIds"));

        //        if (!request.IsAdminLoggin ||
        //            !request.AdminPermissionsImpl.HasChannelPermissions(siteId, channelId,
        //                ConfigManager.ChannelPermissions.ContentTranslate))
        //        {
        //            return Unauthorized();
        //        }

        //        var siteInfo = SiteManager.GetSiteInfo(siteId);
        //        if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

        //        var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
        //        if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

        //        var retval = new List<Dictionary<string, object>>();
        //        foreach (var contentId in contentIdList)
        //        {
        //            var contentInfo = ContentManager.GetContentInfo(siteInfo, channelInfo, contentId);
        //            if (contentInfo == null) continue;

        //            var dict = contentInfo.ToDictionary();
        //            dict["checkState"] =
        //                CheckManager.GetCheckState(siteInfo, contentInfo);
        //            retval.Add(dict);
        //        }

        //        var sites = new List<object>();
        //        var channels = new List<object>();

        //        var siteIdList = request.AdminPermissionsImpl.GetSiteIdList();
        //        foreach (var permissionSiteId in siteIdList)
        //        {
        //            var permissionSiteInfo = SiteManager.GetSiteInfo(permissionSiteId);
        //            sites.Add(new
        //            {
        //                permissionSiteInfo.Id,
        //                permissionSiteInfo.SiteName
        //            });
        //        }

        //        var channelIdList = request.AdminPermissionsImpl.GetChannelIdList(siteInfo.Id,
        //            ConfigManager.ChannelPermissions.ContentAdd);
        //        foreach (var permissionChannelId in channelIdList)
        //        {
        //            var permissionChannelInfo = ChannelManager.GetChannelInfo(siteInfo.Id, permissionChannelId);
        //            channels.Add(new
        //            {
        //                permissionChannelInfo.Id,
        //                ChannelName = ChannelManager.GetChannelNameNavigation(siteInfo.Id, permissionChannelId)
        //            });
        //        }

        //        return Ok(new
        //        {
        //            Value = retval,
        //            Sites = sites,
        //            Channels = channels,
        //            Site = siteInfo
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogUtils.AddErrorLog(ex);
        //        return InternalServerError(ex);
        //    }
        //}

        [HttpGet, Route(Route)]
        public IHttpActionResult GetConfig()
        {
            try
            {
                var request = new AuthenticatedRequest();

                var siteId = request.GetQueryInt("siteId");
                var channelId = request.GetQueryInt("channelId");
                var contentIdList = TranslateUtils.StringCollectionToIntList(request.GetQueryString("contentIds"));

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                var sites = new List<object>();
                var channels = new List<object>();

                if (channelInfo.Additional.TransType == ECrossSiteTransType.SelfSite || channelInfo.Additional.TransType == ECrossSiteTransType.SpecifiedSite || channelInfo.Additional.TransType == ECrossSiteTransType.ParentSite)
                {
                    int theSiteId;
                    if (channelInfo.Additional.TransType == ECrossSiteTransType.SelfSite)
                    {
                        theSiteId = siteInfo.Id;
                    }
                    else if (channelInfo.Additional.TransType == ECrossSiteTransType.SpecifiedSite)
                    {
                        theSiteId = channelInfo.Additional.TransSiteId;
                    }
                    else
                    {
                        theSiteId = SiteManager.GetParentSiteId(siteInfo.Id);
                    }
                    if (theSiteId > 0)
                    {
                        var theSiteInfo = SiteManager.GetSiteInfo(theSiteId);
                        if (theSiteInfo != null)
                        {
                            sites.Add(new
                            {
                                theSiteInfo.Id,
                                theSiteInfo.SiteName
                            });
                        }
                    }
                }
                else if (channelInfo.Additional.TransType == ECrossSiteTransType.AllParentSite)
                {
                    var siteIdList = SiteManager.GetSiteIdList();

                    var allParentSiteIdList = new List<int>();
                    SiteManager.GetAllParentSiteIdList(allParentSiteIdList, siteIdList, siteInfo.Id);

                    foreach (var psId in siteIdList)
                    {
                        if (psId == siteInfo.Id) continue;
                        var psInfo = SiteManager.GetSiteInfo(psId);
                        var show = psInfo.IsRoot || allParentSiteIdList.Contains(psInfo.Id);
                        if (show)
                        {
                            sites.Add(new
                            {
                                psInfo.Id,
                                psInfo.SiteName
                            });
                        }
                    }
                }
                else if (channelInfo.Additional.TransType == ECrossSiteTransType.AllSite)
                {
                    var siteIdList = SiteManager.GetSiteIdList();

                    foreach (var psId in siteIdList)
                    {
                        var psInfo = SiteManager.GetSiteInfo(psId);
                        sites.Add(new
                        {
                            psInfo.Id,
                            psInfo.SiteName
                        });
                    }
                }

                var retval = new List<Dictionary<string, object>>();
                foreach (var contentId in contentIdList)
                {
                    var contentInfo = ContentManager.GetContentInfo(siteInfo, channelInfo, contentId);
                    if (contentInfo == null) continue;

                    var dict = contentInfo.ToDictionary();
                    dict["checkState"] =
                        CheckManager.GetCheckState(siteInfo, contentInfo);
                    retval.Add(dict);
                }

                var channelIdList = ChannelManager.GetChannelIdList(siteInfo.Id);
                foreach (var permissionChannelId in channelIdList)
                {
                    var permissionChannelInfo = ChannelManager.GetChannelInfo(siteInfo.Id, permissionChannelId);
                    channels.Add(new
                    {
                        permissionChannelInfo.Id,
                        ChannelName = ChannelManager.GetChannelNameNavigation(siteInfo.Id, permissionChannelId)
                    });
                }

                return Ok(new
                {
                    Value = retval,
                    Sites = sites,
                    Channels = channels,
                    Site = siteInfo
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        //[HttpGet, Route(RouteGetChannels)]
        //public IHttpActionResult GetChannels()
        //{
        //    try
        //    {
        //        var request = new AuthenticatedRequest();

        //        var siteId = request.GetQueryInt("siteId");

        //        var channels = new List<object>();
        //        var channelIdList = request.AdminPermissionsImpl.GetChannelIdList(siteId,
        //            ConfigManager.ChannelPermissions.ContentAdd);
        //        foreach (var permissionChannelId in channelIdList)
        //        {
        //            var permissionChannelInfo = ChannelManager.GetChannelInfo(siteId, permissionChannelId);
        //            channels.Add(new
        //            {
        //                permissionChannelInfo.Id,
        //                ChannelName = ChannelManager.GetChannelNameNavigation(siteId, permissionChannelId)
        //            });
        //        }

        //        return Ok(new
        //        {
        //            Value = channels
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        LogUtils.AddErrorLog(ex);
        //        return InternalServerError(ex);
        //    }
        //}

        [HttpGet, Route(RouteGetChannels)]
        public IHttpActionResult GetChannels()
        {
            try
            {
                var request = new AuthenticatedRequest();

                var siteId = request.GetQueryInt("siteId");

                var channels = new List<object>();
                //获取所有栏目
                var channelIdList = ChannelManager.GetChannelIdList(siteId);
                foreach (var permissionChannelId in channelIdList)
                {
                    var permissionChannelInfo = ChannelManager.GetChannelInfo(siteId, permissionChannelId);
                    channels.Add(new
                    {
                        permissionChannelInfo.Id,
                        ChannelName = ChannelManager.GetChannelNameNavigation(siteId, permissionChannelId)
                    });
                }

                return Ok(new
                {
                    Value = channels
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Submit()
        {
            try
            {
                var request = new AuthenticatedRequest();

                var siteId = request.GetPostInt("siteId");
                var channelId = request.GetPostInt("channelId");
                var contentIdList = TranslateUtils.StringCollectionToIntList(request.GetPostString("contentIds"));
                var targetSiteId = request.GetPostInt("targetSiteId");
                var targetChannelId = request.GetPostInt("targetChannelId");
                var copyType = request.GetPostString("copyType");


                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var targetSiteInfo = SiteManager.GetSiteInfo(targetSiteId);
                if (targetSiteInfo == null) return BadRequest("无法确定转发对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                foreach (var contentId in contentIdList)
                {
                    if (targetChannelId != 0)
                    {
                        CrossSiteTransUtility.TransContentInfo(siteInfo, channelInfo, contentId, targetSiteInfo, targetChannelId);
                    }
                }

                //request.AddSiteLog(siteId, channelId, "批量跨站转发", string.Empty);
                request.AddSiteLog(siteId, channelId, "内容跨站转发", $"转发到站点:{targetSiteInfo.SiteName}");

                CreateManager.TriggerContentChangedEvent(siteId, channelId);

                return Ok(new
                {
                    Value = contentIdList
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex, "批量跨站转发失败");
                return InternalServerError(ex);
            }
        }
    }
}
