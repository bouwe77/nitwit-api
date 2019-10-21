﻿using System.IO;

namespace nitwitapi
{
    /// <summary>
    /// Authentication and authorization is enabled by creating a specific file.
    /// </summary>
    public class Auth
    {
        private static readonly string _filePath = Path.Combine(Constants.ApplicationFolder, "auth.enabled");
        private static bool? _enabled;

        public static bool Enabled
        {
            get
            {
                if (!_enabled.HasValue)
                {
                    _enabled = File.Exists(_filePath);
                }

                return _enabled.Value;
            }
        }
    }
}
