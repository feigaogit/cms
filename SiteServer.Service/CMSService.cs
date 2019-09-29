using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SiteServer.CMS.Core;
using SiteServer.CMS.Data;
using SiteServer.CMS.Model;
using SiteServer.Service.Model;
using SiteServer.Utils;
using SiteServer.Utils.Enumerations;

namespace SiteServer.Service
{
    public partial class CMSService : ServiceBase
    {
        System.Timers.Timer timer1;  //计时器

        /// <summary>
        /// 账号获取接口
        /// </summary>
        private string AccountInfoApi => ConfigHelper.GetConfigString("AccountInfoApi");

        public CMSService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                //配置的时间间隔，单位秒
                var interval = ConfigHelper.GetConfigInt("Interval");
                interval = interval == 0 ? 10 : interval;

                //读取配置项并设置连接字符串
                var applicationPhysicalPath = AppDomain.CurrentDomain.BaseDirectory;
                WebConfigUtils.Load(applicationPhysicalPath, PathUtils.Combine(applicationPhysicalPath, "SiteServer.Service.exe.config"));

                timer1 = new System.Timers.Timer { Interval = interval * 1000 };
                //设置计时器事件间隔执行时间
                timer1.Elapsed += timer1_Elapsed;
                timer1.Enabled = true;
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
            }
        }

        protected override void OnStop()
        {
            try
            {
                timer1.Enabled = false;
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
            }
        }

        /// <summary>
        /// 计时器事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;

            try
            {
                //处理数据
                HandleData();
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }

            timer1.Enabled = true;
        }

        /// <summary>
        /// 处理数据
        /// </summary>
        private void HandleData()
        {
            try
            {
                var adminAccount = "admin";

                //获取账号信息
                var resultStr = SendGetHttpRequest(AccountInfoApi);
                //账号信息
                var result = JsonConvert.DeserializeObject<ResultInfo<List<AccountInfo>>>(resultStr);
                if (!result.Result)
                {
                    WriteLog($"账号信息获取失败：{result.Msg}");
                    return;
                }

                //账号
                var accounts = result.Data;
                var nowTime = DateTime.Now;

                var addList = new List<AdministratorInfo>();//需要新增的管理员信息
                var updateList = new List<AdministratorInfo>();//需要更新的管理员信息

                //存接口获取的正常的用户id
                var ids = new List<string>();

                //获取系统中的所有管理员账号
                var allList = DataProvider.AdministratorDao.ApiGetAdministrators(0, int.MaxValue);

                //新增或更新用户信息
                foreach (var account in accounts)
                {
                    ids.Add(account.Id);

                    //找到cms系统中对应的管理员
                    var admin = allList.Find(t => t.UserName.ToLower() == account.Id.ToLower());

                    //账号不存在的，需要新增账号
                    if (admin == null)
                    {
                        //是超管,要先关联（这里有点问题哎，管理员username更新不了！！！，我们直接不关联这里的管理员了，请在登录那块处理吧）
                        if (account.Admin && account.Account == adminAccount)
                        {
                            //var cmsAdmin = allList.Find(t => t.UserName == adminAccount);
                            //cmsAdmin.UserName = account.Id;
                            //updateList.Add(cmsAdmin);
                            continue;
                        }

                        addList.Add(new AdministratorInfo
                        {
                            UserName = account.Id.ToLower(),
                            Password = "123qwe",
                            PasswordFormat = EPasswordFormatUtils.GetValue(EPasswordFormat.Encrypted),
                            CreationDate = nowTime,
                            LastActivityDate = DateUtils.SqlMinValue,
                            CountOfLogin = 0,
                            CountOfFailedLogin = 0,
                            CreatorUserName = string.Empty,
                            IsLockedOut = false,
                            SiteIdCollection = string.Empty,
                            SiteId = 0,
                            DepartmentId = 0,
                            AreaId = 0,
                            DisplayName = account.Name,
                            Mobile = string.Empty,
                            Email = string.Empty,
                            AvatarUrl = string.Empty
                        });
                    }
                    else
                    {
                        //超管就别更新了
                        if (admin.UserName == adminAccount)
                        {
                            continue;
                        }

                        if (!admin.IsLockedOut && admin.DisplayName == account.Name)
                        {
                            continue;
                        }

                        //更新用户的名称和锁定状态
                        admin.IsLockedOut = false;
                        admin.DisplayName = account.Name;

                        updateList.Add(admin);
                    }
                }

                //获取到要锁定的用户
                var lockList = allList.FindAll(t => !ids.Contains(t.UserName));
                foreach (var item in lockList)
                {
                    //超管就别锁了吧
                    if (item.UserName == adminAccount)
                    {
                        continue;
                    }

                    //不需要重复锁啦
                    if (item.IsLockedOut)
                    {
                        continue;
                    }

                    //锁定(数字校园没有权限的这边直接锁定，不删除)
                    item.IsLockedOut = true;
                    updateList.Add(item);
                }

                //这里是新增出现的错误，我们要记录下
                var errors = new StringBuilder();
                foreach (var item in addList)
                {
                    DataProvider.AdministratorDao.Insert(item, out var msg);
                    if (!string.IsNullOrWhiteSpace(msg))
                    {
                        errors.Append($"id:{item.UserName};{msg}\r\n");
                    }
                }

                foreach (var item in updateList)
                {
                    DataProvider.AdministratorDao.Update(item);
                }

                if (errors.Length > 0)
                {
                    WriteLog("存在错误：" + errors);
                }
            }
            catch (Exception e)
            {
                WriteLog("处理失败：" + e.Message);
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

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="content"></param>
        private void WriteLog(string content)
        {
            string strFileName = System.DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            SiteServer.Utils.Log.WriteLog.WriteLogFile(strFileName, content);
        }
    }
}
