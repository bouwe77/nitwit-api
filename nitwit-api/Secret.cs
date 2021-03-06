﻿using System;
using System.IO;

namespace nitwitapi
{
    /// <summary>
    /// The password is stored in a separate file which must exist to use the application.
    /// </summary>
    public class Secret
    {
        private static readonly string _filePath = Path.Combine(Constants.ApplicationFolder, "secret.password");
        private static string _password;

        public static string Password
        {
            get
            {
                if (_password == null)
                {
                    if (!File.Exists(_filePath))
                        throw new Exception($"Missing file: '{_filePath}'");

                    _password = File.ReadAllText(Path.Combine(_filePath)).Trim();
                }

                return _password;
            }
        }
    }
}
