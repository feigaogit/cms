using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;
using SiteServer.Utils;

namespace SiteServer.Service
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            using (SettingHelper setting = new SettingHelper("SiteServer.Service.exe.config"))
            {
                //系统用于标志此服务名称(唯一性)
                serviceInstaller1.ServiceName = setting.ServiceName;
                //向用户标志服务的显示名称(可以重复)
                serviceInstaller1.DisplayName = setting.DisplayName;
                //服务的说明(描述)
                serviceInstaller1.Description = setting.Description;
            }
        }
    }
}
