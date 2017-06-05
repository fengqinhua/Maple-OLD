
using Maple.Web.Environment.Configuration;
using Maple.Web.Environment.ShellBuilders;

namespace Maple.Web.Environment {
    /// <summary>
    /// ����Ӧ�ó�������
    /// </summary>
    public interface IMapleHost
    {
        /// <summary>
        /// ������Ӧ�ó�������ʱ���ã��ҽ�����һ�Σ����ڳ�ʼ�����
        /// </summary>
        void Initialize();
        /// <summary>
        /// ���¼�����չ��Ϣ
        /// </summary>
        void ReloadExtensions();
        /// <summary>
        /// ��ÿ�������������ʱִ��
        /// </summary>
        void BeginRequest();
        /// <summary>
        /// ��ÿ�������������ʱִ��
        /// </summary>
        void EndRequest();

        ShellContext GetShellContext(ShellSettings shellSettings);
        /// <summary>
        /// ���ڹ���shell���ô������ʱ����ʵ��
        /// </summary>
        IWorkContextScope CreateStandaloneEnvironment(ShellSettings shellSettings);
    }
}
