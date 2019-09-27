namespace SiteServer.Utils
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Cofig文件AppSetting项操作类
    /// </summary>
    public sealed class ConfigHelper
    {
        #region 获取或设置AppSettings相关配置项

        /// <summary>
        /// 获取key且将其转化为bool型，如果读取到该配置项，则返回false
        /// </summary>
        /// <param name="key">配置项名称</param>
        /// <returns></returns>
        public static bool GetConfigBool(string key)
        {
            bool flag = false;
            string configString = GetConfigString(key);
            if ((configString != null) && (string.Empty != configString))
            {
                try
                {
                    flag = bool.Parse(configString);
                }
                catch (FormatException)
                {
                }
            }
            return flag;
        }

        /// <summary>
        /// 获取key且将其转化为decimal型，如果读取到该配置项，则返回0M
        /// </summary>
        /// <param name="key">配置项名称</param>
        /// <returns></returns>
        public static decimal GetConfigDecimal(string key)
        {
            decimal num = 0M;
            string configString = GetConfigString(key);
            if ((configString != null) && (string.Empty != configString))
            {
                try
                {
                    num = decimal.Parse(configString);
                }
                catch (FormatException)
                {
                }
            }
            return num;
        }

        /// <summary>
        /// 获取key且将其转化为int型，如果读取到该配置项，则返回0
        /// </summary>
        /// <param name="key">配置项名称</param>
        /// <returns></returns>
        public static int GetConfigInt(string key)
        {
            int num = 0;
            string configString = GetConfigString(key);
            if ((configString != null) && (string.Empty != configString))
            {
                try
                {
                    num = int.Parse(configString);
                }
                catch (FormatException)
                {
                }
            }
            return num;
        }

        /// <summary>
        /// 获取key且将其转化为string型，如果读取不到该配置项，则返回null
        /// </summary>
        /// <param name="key">配置项名称</param>
        /// <returns></returns>
        public static string GetConfigString(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }


        /// <summary>
        /// 保存AppSettings项值
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="appvaluelist"></param>
        public static bool SaveConfigString(string appkey, string appvalue)
        {
            bool ret = false;

            try
            {
                System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                AppSettingsSection section = (AppSettingsSection)configuration.GetSection("appSettings");
                section.Settings[appkey].Value = appvalue;
                configuration.Save();
                ConfigurationManager.RefreshSection("appSettings");
                ret = true;
            }
            catch (System.Exception ex)
            {
                //EmailLogger.Debug("保存参数: " + appkey + "  出现异常");
                ret = false;
            }

            return ret;
        }

        #endregion

        #region 获取或设置ConnectionStrings相关配置项

        /// <summary>
        /// 获取key且将其转化为string型，如果读取不到该配置项，则返回""
        /// </summary>
        /// <param name="key">配置项名称</param>
        /// <returns></returns>
        public static string GetConnectionString(string key)
        {
            return ConfigurationManager.ConnectionStrings[key].ToString();
        }


        /// <summary>
        /// 保存ConnectionStrings项值
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="appvaluelist"></param>
        public static bool SaveConnectionString(string connectionkey, string appvalue)
        {
            bool ret = false;

            try
            {
                System.Configuration.Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                ConnectionStringsSection section = (ConnectionStringsSection)configuration.GetSection("connectionStrings");
                section.ConnectionStrings[connectionkey].ConnectionString = appvalue;
                configuration.Save();
                ConfigurationManager.RefreshSection("connectionStrings");
                ret = true;
            }
            catch
            {
                ret = false;
            }

            return ret;
        }

        #endregion
    }
}

