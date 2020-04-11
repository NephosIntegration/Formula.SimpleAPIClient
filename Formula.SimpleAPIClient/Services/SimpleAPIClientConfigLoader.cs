using System;
using System.IO;
using System.Text.Json;
using Formula.SimpleCore;

namespace Formula.SimpleAPIClient
{
    public class SimpleAPIClientConfigLoader : ConfigLoader<SimpleAPIClientConfigModel>
    {
        public SimpleAPIClientConfigModel Instance { get { return this.instance; } }

        public OAuth2ConfigModel OAuth2Config { get { return this.instance.OAuth2Config; } }
        
        public static new SimpleAPIClientConfigLoader Get(String fileName, GetDefaults getDefaults = null)
        {
            var output = new SimpleAPIClientConfigLoader();

            output.LoadFromFile(fileName, getDefaults);

            return output;
        }

    }
}