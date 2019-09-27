namespace SiteServer.Service.Model
{
    public class AccountInfo
    {

        /// <summary>
        /// 唯一id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 登录账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        ///  是否管理员
        /// </summary>
        public bool Admin { get; set; }

    }
}