namespace Database
{
    using System;
    /// <summary>
    /// ϵͳ����Ա
    /// </summary>
    public partial class Admin
    {
        /// <summary>
        /// ��ʶ
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// ���ű�ʶ
        /// </summary>
        public int Department_ID { get; set; }
        /// <summary>
        /// �û���
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// ��ʵ����
        /// </summary>
        public string Realname { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// ���ݽ�ɫ
        /// ϵͳ����Ա���ݽ�ɫ��
        /// 0:��������Ա-superadmin
        /// 1:������Ա-manager
        /// 2:��ά��Ա-normal
        /// 3:�������-partner
        /// </summary>
        public string Roletype { get; set; }
        /// <summary>
        /// ��Ұ
        /// 0:ֻ�ܲ鿴�Լ�����Ϣ-self
        /// 1:�鿴���е���Ϣ-all
        /// </summary>
        public string Seescope { get; set; }
        /// <summary>
        /// ��¼����
        /// </summary>
        public Nullable<int> LoginTimes { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public Nullable<System.DateTime> CommitTime { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public Nullable<System.DateTime> UpdateTime { get; set; }
    
        public virtual Department Department { get; set; }
    }
}
