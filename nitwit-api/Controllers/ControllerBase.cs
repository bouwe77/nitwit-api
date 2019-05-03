﻿using Data;
using Data.Entities;
using Dolores;
using Dolores.Http;
using Dolores.Responses;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace nitwitapi.Controllers
{
    public abstract class ControllerBase : DoloresHandler
    {
        private string _databaseFilePath => @"D:\home\site\wwwroot\nitwit.sqlite";
        private static Regex _usernameRegex = new Regex("^[a-zA-Z0-9]+$", RegexOptions.Compiled);

        protected Response GetJsonResponse(object objectToSerializeToJson)
        {
            string json = JsonConvert.SerializeObject(objectToSerializeToJson);
            var response = new Response(HttpStatusCode.Ok)
            {
                MessageBody = new MemoryStream(Encoding.UTF8.GetBytes(json))
            };

            response.SetContentTypeHeader("application/json; charset=utf-8");
            AddAccessControlAllowOriginHeader(response);

            return response;
        }

        protected static void AddAccessControlAllowOriginHeader(Response response)
        {
            response.SetHeader(HttpResponseHeaderFields.AccessControlAllowOrigin, "http://localhost:3000");
        }

        protected SqliteRepository<User> CreateUserRepository()
        {
            return new SqliteRepository<User>(_databaseFilePath);
        }

        protected SqliteRepository<Post> CreatePostRepository()
        {
            return new SqliteRepository<Post>(_databaseFilePath);
        }

        protected SqliteRepository<Following> CreateFollowingRepository()
        {
            return new SqliteRepository<Following>(_databaseFilePath);
        }

        protected SqliteRepository<ToDo> CreateToDoRepository()
        {
            return new SqliteRepository<ToDo>(_databaseFilePath);
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
    }
}