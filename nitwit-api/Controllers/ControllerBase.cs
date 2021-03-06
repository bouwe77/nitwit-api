﻿using Data.Repositories;
using Data.Entities;
using Dolores;
using Dolores.Exceptions;
using Dolores.Http;
using Dolores.Responses;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using nitwitapi.Extensions;

namespace nitwitapi.Controllers
{
    public abstract class ControllerBase : DoloresHandler
    {
        private string _databaseFilePath => Path.Combine(Constants.ApplicationFolder, "nitwit.sqlite");
        private static Regex _usernameRegex = new Regex(Constants.ValidUsernameRegexForValidation, RegexOptions.Compiled);

        protected void CheckPassword()
        {
            var pass = Request.GetQueryStringValue("pass");
            if (string.IsNullOrWhiteSpace(pass) || pass != Secret.Password)
                throw new HttpMethodNotAllowedException(string.Empty);
        }

        protected Response GetJsonResponse(object objectToSerializeToJson)
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(objectToSerializeToJson, jsonSettings);
            var response = new Response(HttpStatusCode.Ok)
            {
                MessageBody = new MemoryStream(Encoding.UTF8.GetBytes(json))
            };

            response.SetContentTypeHeader("application/json; charset=utf-8");
            response.AddAccessControlAllowOriginHeader();
            
            return response;
        }

        protected SqliteRepository<User> CreateUserRepository()
        {
            return new SqliteRepository<User>(_databaseFilePath);
        }

        protected PostRepository CreatePostRepository()
        {
            return new PostRepository(_databaseFilePath);
        }

        protected SqliteRepository<Following> CreateFollowingRepository()
        {
            return new SqliteRepository<Following>(_databaseFilePath);
        }

        protected SqliteRepository<ToDo> CreateToDoRepository()
        {
            return new SqliteRepository<ToDo>(_databaseFilePath);
        }

        protected SqliteRepository<Mention> CreateMentionRepository()
        {
            return new SqliteRepository<Mention>(_databaseFilePath);
        }

        protected SqliteRepository<Reply> CreateReplyRepository()
        {
            return new SqliteRepository<Reply>(_databaseFilePath);
        }

        protected bool IsPostContentValid(string post)
        {
            return !string.IsNullOrWhiteSpace(post) &&
                   post.Length <= 240;
        }

        protected bool IsPostIdentifierValid(string postId)
        {
            return !string.IsNullOrWhiteSpace(postId) &&
                   postId.Length < 10;
        }

        protected bool IsUsernameValid(string username)
        {
            return !string.IsNullOrWhiteSpace(username) &&
                   username.Length <= 100 &&
                   _usernameRegex.IsMatch(username);
        }

        protected bool IsPasswordValid(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length <= 50;
        }

        protected bool IsUsernameAllowed(string username)
        {
            var restrictedUsernames = new List<string>
            {
                "following",
                "timeline",
                "home",
                "whoami"
            };

            return !restrictedUsernames.Contains(username, StringComparer.OrdinalIgnoreCase);
        }

        protected Response GetUnauthorizedResponse(string message = "Unauthorized")
        {
            var response = new Response(HttpStatusCode.Unauthorized);

            if (DebugMode.Enabled && !string.IsNullOrEmpty(message))
            {
                response.MessageBody = new MemoryStream(Encoding.UTF8.GetBytes(message));
            }

            return response;
        }
    }
}
