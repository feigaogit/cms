using System;
using System.IO;
using System.Xml;

namespace SiteServer.Utils
{
    public class SettingHelper : IDisposable
    {
        #region 私有成员
        /// <summary>
        /// 配置文件名称
        /// </summary>
        public string ConfigFileName = "App.config";

        private string _ServiceName;
        private string _DisplayName;
        private string _Description;
        #endregion

        #region 构造函数
        /// <summary>
        /// 初始化服务配置帮助类 
        /// </summary>
        /// <param name="configFilePath">配置文件名称</param>
        public SettingHelper(string configFilePath)
        {
            ConfigFileName = configFilePath;
            InitSettings();
        }
        #endregion

        #region 属性
        /// <summary> 
        /// 系统用于标志此服务的名称 
        /// </summary> 
        public string ServiceName
        {
            get { return _ServiceName; }
        }
        /// <summary> 
        /// 向用户标志服务的友好名称 
        /// </summary> 
        public string DisplayName
        {
            get { return _DisplayName; }
        }
        /// <summary> 
        /// 服务的说明 
        /// </summary> 
        public string Description
        {
            get { return _Description; }
        }
        #endregion

        #region 私有方法
        #region 初始化服务配置信息
        /// <summary> 
        /// 初始化服务配置信息 
        /// </summary> 
        private void InitSettings()
        {
            string root = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string xmlfile = root.Remove(root.LastIndexOf('\\') + 1) + ConfigFileName;
            if (File.Exists(xmlfile))
            {
                //系统用于标志此服务名称(唯一性)
                _ServiceName = Get_ConfigValue(xmlfile, "Service_ID");
                //向用户标志服务的显示名称(可以重复)
                _DisplayName = Get_ConfigValue(xmlfile, "Service_DisplayName");
                //服务的说明(描述)
                _Description = Get_ConfigValue(xmlfile, "Service_Description");
            }
            else
            {
                throw new FileNotFoundException(string.Format("未能找到服务名称配置文件{0}！路径:{1}", ConfigFileName, xmlfile));
            }

        }
        /// <summary>
        /// 读取 XML中指定节点值
        /// </summary>
        /// <param name="configpath">配置文件路径</param>
        /// <param name="strKeyName">键值</param>        
        /// <returns></returns>
        protected static string Get_ConfigValue(string configpath, string strKeyName)
        {
            using (XmlTextReader tr = new XmlTextReader(configpath))
            {
                while (tr.Read())
                {
                    if (tr.NodeType == XmlNodeType.Element)
                    {
                        if (tr.Name == "add" && tr.GetAttribute("key") == strKeyName)
                        {
                            return tr.GetAttribute("value");
                        }
                    }
                }
            }
            return "";
        }
        #endregion
        #endregion

        #region IDisposable 成员
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //managed dispose 
                    _ServiceName = null;
                    _DisplayName = null;
                    _Description = null;
                }
                //unmanaged dispose 
            }
            disposed = true;
        }
        /// <summary>
        /// 析构函数(destructor) 与构造函数相反，当对象脱离其作用域时（例如对象所在的函数已调用完毕），系统自动执行析构函数
        /// </summary>
        ~SettingHelper()
        {
            Dispose(false);
        }
        #endregion
    }
}
