using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;

namespace AlphaMDHealth.ServiceConfiguration
{
    public class PrefixKeyVaultSecretManager : KeyVaultSecretManager 
    {
        private readonly string _prefix;

        public PrefixKeyVaultSecretManager(string prefix)
        {
            _prefix = $"{prefix}-";
        }

        public override string GetKey(KeyVaultSecret secret)
        {
            //var result = secret.Name.Replace("---", ".").Replace("--", ConfigurationPath.KeyDelimiter);
            var secretname = secret.Name;
            var result = "";
            if(secretname.Contains("--"))
            {
                result = secret.Name.Substring(_prefix.Length).Replace("--", ConfigurationPath.KeyDelimiter);
            }
            return result;
        }

        //public bool Load(SecretItem secret)
        //{
        //    return secret.Identifier.Name.StartsWith(_prefix);
        //}

        //public string GetKey(SecretBundle secret)
        //{
        //    return secret.SecretIdentifier.Name.Substring(_prefix.Length).Replace("--", ConfigurationPath.KeyDelimiter);
        //}
    }
}