namespace SiteServer.API.SiteServer.Model
{
    public class UserInfo
    {

        /// <summary>
        /// 唯一id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 教师id
        /// </summary>
        public string TId { get; set; }

        /// <summary>
        /// 用户类型：0教职工、1学生
        /// </summary>
        public int UserType { get; set; }

        /// <summary>
        ///  用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string LoginName { get; set; }

    }
}