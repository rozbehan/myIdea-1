﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.SimpleSystemsManagement.Model.Internal.MarshallTransformations;
using Amazon.CloudWatchLogs;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HomeSecurityApi
{
    /// <summary>
    /// We can reset the machine state to default state by requence key Reset, 'Reset'='True'
    /// </summary>
    public struct Default
    {
        public const string Code = "1111";
        public const string CurrentState = MachineDefinition.Disarmed;
    }
    /// <summary>
    /// Storing the CurrentState and Code.
    /// AWS Prameter Storing Service is a secure place to save the sensitive information.
    /// Encrypted data in a file as an alternative to save the information. But I couldn't write!
    /// Finally I have also used the Environment Variables of AWS Lambda Function for that.
    /// I think using the TLS or SSL along AWS Parameter Storing Service is enough and the best.
    /// This class is for the first two.
    /// </summary>
    public class SecureParameter
    {
        private string tmpString;

        public SecureParameter()
        {
            var a = PutSecureParameter(MachineDefinition.CurrentState, Default.CurrentState);
            a = PutSecureParameter(MachineDefinition.Code, Default.Code);
        }

        public bool ValidateCode(string code)
        {
            string a = GetSecureParameter(MachineDefinition.Code).ToString();
            //var a = GetParameter(MachineDefinition.Code);
            return (tmpString == code ? true : false);
        }

        public string CurrentState()
        {
            var a = GetSecureParameter(MachineDefinition.CurrentState);
            //var a = GetParameter(MachineDefinition.CurrentState);
            if (a == null)
            {
                a = PutSecureParameter(MachineDefinition.CurrentState, Default.CurrentState);
                a = GetSecureParameter(MachineDefinition.CurrentState);
                //currentState = PutParameter(MachineDefinition.CurrentState, Default.CurrentState);
            }
            return tmpString;
        }

        public string Code()
        {
            var a = GetSecureParameter(MachineDefinition.Code);
            //var a = GetParameter(MachineDefinition.Code);
            if (a == null)
            {
                a = PutSecureParameter(MachineDefinition.Code, Default.Code);
                a = GetSecureParameter(MachineDefinition.Code);
                //a = PutParameter(MachineDefinition.Code, Default.Code);
            }
            return tmpString;
        }

        public void ChangeCurrentState(string state)
        {
            var a = PutSecureParameter(MachineDefinition.CurrentState, state);
            //var a = PutParameter(MachineDefinition.CurrentState, state);
        }
        /// <summary>
        /// An alternative is using the AWS services directly instead of .net SDK.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        private async Task<string> GetSecureParameter(string parameterName)
        {
            var client = new AmazonSimpleSystemsManagementClient(RegionEndpoint.APSoutheast2);
            var request = new GetParametersRequest
            {
                Names = new List<string> { parameterName }
            };
            var response =  await client.GetParametersAsync(request);
            tmpString = response.Parameters.SingleOrDefault().Value;
            return response.ToString();
        }

        private async Task<string> PutSecureParameter(string parameterName, string value)
        {
            var client = new AmazonSimpleSystemsManagementClient(RegionEndpoint.APSoutheast2);
            var request = new PutParameterRequest
            {
                Name = parameterName,
                Overwrite = true,
                Value = value,
                Type = ParameterType.SecureString
            };
            var response = await client.PutParameterAsync(request);
            return response.ToString();
        }

        private string GetParameter(string parameterName)
        {
            if (!File.Exists(@"Secure.json"))
            {
                File.WriteAllText(@"Secure.json", @"{'" + MachineDefinition.Code + @"':'" + Default.Code + @"', '" + 
                    MachineDefinition.CurrentState + @"':'" + Default.CurrentState + @"'}");
            }
            var s1 = File.ReadAllText(@"Secure.json");
            var o1 = JObject.Parse(s1);
            tmpString = (string)o1[parameterName];
            return (string)o1[parameterName];
        }

        private string PutParameter(string parameterName, string value)
        {
            if (!File.Exists(@"Secure.json"))
            {
                File.WriteAllText(@"Secure.json", @"{'" + MachineDefinition.Code + @"':'" + Default.Code + @"', '" + 
                    MachineDefinition.CurrentState + @"':'" + Default.CurrentState + @"'}");
            }
            var s1 = File.ReadAllText(@"Secure.json");
            var o1 = JObject.Parse(s1);
            o1[parameterName] = value;
            tmpString = value;
            File.WriteAllText(@"Secure.json", JsonConvert.SerializeObject(o1));
            return (string)o1[parameterName];
        }
    }
}