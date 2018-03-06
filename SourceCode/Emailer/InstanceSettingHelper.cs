using Relativity.API;
using Relativity.Services;
using Relativity.Services.InstanceSetting;
using System;
using System.Linq;

namespace Emailer
{
    public class InstanceSettingHelper
    {
        public static InstanceSetting QuerySingleInstanceSetting(IServicesMgr servicesMgr, ExecutionIdentity executionIdentity, string name)
        {
            string errorContext = $"An error occured when querying for the instance setting. [{nameof(name)}: {name}]";
            InstanceSetting instanceSetting = null;

            try
            {
                using (IInstanceSettingManager instanceSettingManager = servicesMgr.CreateProxy<IInstanceSettingManager>(executionIdentity))
                {
                    Query instanceSettingQuery = new Query();
                    Condition nameCondition = new TextCondition(InstanceSettingFieldNames.Name, TextConditionEnum.EqualTo, name);
                    instanceSettingQuery.Condition = nameCondition.ToQueryString();
                    InstanceSettingQueryResultSet instanceSettingQueryResultSet;

                    try
                    {
                        instanceSettingQueryResultSet = instanceSettingManager.QueryAsync(instanceSettingQuery).Result;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"{errorContext}. QueryAsync.", ex);
                    }

                    if (instanceSettingQueryResultSet.Success)
                    {
                        if (instanceSettingQueryResultSet.Results.Count == 1)
                        {
                            instanceSetting = instanceSettingQueryResultSet.Results.First().Artifact;
                        }
                        if (instanceSettingQueryResultSet.Results.Count > 1)
                        {
                            throw new Exception("More than one instance setting exists with the same name.");
                        }
                    }
                    else
                    {
                        throw new Exception($"{errorContext}. ErrorMessage = {instanceSettingQueryResultSet.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"{errorContext}", ex);
            }

            return instanceSetting;
        }
    }
}
