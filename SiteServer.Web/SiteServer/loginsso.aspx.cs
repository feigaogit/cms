using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using SiteServer.API.SiteServer.Model;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.Utils;

namespace SiteServer.API.SiteServer
{
    public partial class loginsso : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitPage();
            }
        }

        private void InitPage()
        {
            //单点登录地址
            var ssoUrl = ConfigHelper.GetConfigString("SSOUrl");
            //获取sid
            var sid = Request.QueryString["sid"];
            if (string.IsNullOrWhiteSpace(sid))
            {
                Response.Redirect($"{ssoUrl}/login.aspx?redirecturl={Request.Url.AbsoluteUri}");
            }
            else
            {
                try
                {
                    var appId = ConfigHelper.GetConfigString("AppID");
                    var appSecret = ConfigHelper.GetConfigString("AppSecret");
                    var userApi = $"{ssoUrl}/sync.ashx?AppID={appId}&AppSecret={appSecret}&type=ssouser&token={sid}";
                    var resultStr = SendGetHttpRequest(userApi);

                    //获取用户信息
                    var result = JsonConvert.DeserializeObject<ResultInfo<UserInfo>>(resultStr);
                    if (!result.Result)
                    {
                        throw new Exception(result.Msg);
                    }

                    //匹配并写入当前用户，再登录
                    var user = result.Data;
                    if (user == null)
                    {
                        throw new Exception("未获取到用户信息");
                    }

                    //超级管理员
                    if (user.LoginName.ToLower() == "admin")
                    {
                        user.Id = "admin";
                    }

                    //登录到网站系统
                    var request = new AuthenticatedRequest();
                    var adminInfo = AdminManager.GetAdminInfoByUserName(user.Id.ToLower());
                    if (adminInfo == null)
                    {
                        throw new Exception("未获取到用户信息");
                    }
                    DataProvider.AdministratorDao.UpdateLastActivityDateAndCountOfLogin(adminInfo); // 记录最后登录时间、失败次数清零
                    var accessToken = request.AdminLogin(adminInfo.UserName, false);
                    var expiresAt = DateTime.Now.AddDays(Constants.AccessTokenExpireDays);


                    var isRedirect = Request.QueryString["isRedirect"];
                    if (isRedirect != "false")
                    {
                        Response.Redirect("pageInitialization.aspx");
                    }
                }
                catch (Exception e)
                {
                    Response.Write("身份认证失败!" + e.Message);
                }

                //Response.Write(sid);
            }
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="url">Url地址</param>
        /// <param name="contentType"></param>
        private string SendGetHttpRequest(string url, string contentType = "application/x-www-form-urlencoded")
        {
            WebRequest request = (WebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = contentType;
            string result;
            using (WebResponse response = request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }

            return result;
        }
    }
}