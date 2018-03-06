using Relativity.API;
using Relativity.Services.InstanceSetting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Emailer
{
    public class SmtpClientSettings
    {
        public string EmailFrom { get; set; }
        public List<string> EmailTo { get; set; }
        public string SmtpServer { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public int SmtpPort { get; set; }
        public IServicesMgr ServicesMgr { get; set; }
        public ExecutionIdentity ExecutionIdentity { get; set; }

        public SmtpClientSettings(IServicesMgr servicesMgr, ExecutionIdentity executionIdentity)
        {
            ServicesMgr = servicesMgr;
            ExecutionIdentity = executionIdentity;
        }

        public void GetSettings()
        {
            EmailFrom = GetEmailFrom();
            EmailTo = GetEmailTo();
            SmtpServer = GetSmtpServer();
            SmtpUsername = GetSmtpUserName();
            SmtpPassword = GetSmtpPassword();
            SmtpPort = GetSmtpPort();
        }

        private string GetEmailFrom()
        {
            InstanceSetting instanceSetting = InstanceSettingHelper.QuerySingleInstanceSetting(ServicesMgr, ExecutionIdentity, Constants.InstanceSettings.Name.EmailFrom);
            string emailFrom = instanceSetting.Value;
            return emailFrom;
        }

        private List<string> GetEmailTo()
        {
            InstanceSetting instanceSetting = InstanceSettingHelper.QuerySingleInstanceSetting(ServicesMgr, ExecutionIdentity, Constants.InstanceSettings.Name.EmailTo);
            string emailTo = instanceSetting.Value;
            List<string> emailToList = emailTo.Split(';').ToList();
            List<string> emailToListTrim = emailToList.Select(x => x.Trim()).ToList();
            return emailToListTrim;
        }

        private string GetSmtpServer()
        {
            InstanceSetting instanceSetting = InstanceSettingHelper.QuerySingleInstanceSetting(ServicesMgr, ExecutionIdentity, Constants.InstanceSettings.Name.SmtpServer);
            string smtpServer = instanceSetting.Value;
            return smtpServer;
        }

        private string GetSmtpUserName()
        {
            InstanceSetting instanceSetting = InstanceSettingHelper.QuerySingleInstanceSetting(ServicesMgr, ExecutionIdentity, Constants.InstanceSettings.Name.SmtpUserName);
            string smtpUserName = instanceSetting.Value;
            return smtpUserName;
        }

        private string GetSmtpPassword()
        {
            InstanceSetting instanceSetting = InstanceSettingHelper.QuerySingleInstanceSetting(ServicesMgr, ExecutionIdentity, Constants.InstanceSettings.Name.SmtpPassword);
            string smtpPassword = instanceSetting.Value;
            return smtpPassword;
        }

        private int GetSmtpPort()
        {
            InstanceSetting instanceSetting = InstanceSettingHelper.QuerySingleInstanceSetting(ServicesMgr, ExecutionIdentity, Constants.InstanceSettings.Name.SmtpPort);
            int smtpPort = Convert.ToInt32(instanceSetting.Value);
            return smtpPort;
        }
    }
}
