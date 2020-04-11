using System;
using System.IO;
using System.Text.Json;
using Formula.SimpleCore;

namespace Formula.SimpleAPIClient
{
    public class OAuth2Client : BaseClient
    {
        private OAuth2ConfigModel config = null;

        public OAuth2Client(OAuth2ConfigModel config) : base()
        {
            this.config = config;
        }
    }
}