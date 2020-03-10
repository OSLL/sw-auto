﻿namespace AnalyzeResults.Settings
{
    public class MongoSettings
    {
        public string Database { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(User) || string.IsNullOrEmpty(Password))
                    return $@"mongodb://{Host}:{Port}/{Database}";
                return $@"mongodb://{User}:{Password}@{Host}:{Port}/{Database}";
            }
        }
    }
}