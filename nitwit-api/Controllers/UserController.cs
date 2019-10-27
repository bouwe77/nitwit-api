using Data;
using Data.Entities;
using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using nitwitapi.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace nitwitapi.Controllers
{
    public class UserController : ControllerBase
    {
        public Response GetAllUsers()
        {
            CheckPassword();

            // Get from database.
            List<User> users;
            using (var repo = CreateUserRepository())
            {
                users = repo.GetAll().ToList();
            }

            var response = GetJsonResponse(users);

            return response;
        }

        public Response AddUser()
        {
            CheckPassword();

            // Deserialize
            User newUser;
            try
            {
                newUser = Request.MessageBody.DeserializeJson<User>();
            }
            catch (Exception)
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "Request body is not a valid User" });
                return response;
            }

            // Validate
            if (newUser == null || !IsUsernameValid(newUser.Name) || !IsPasswordValid(newUser.Password))
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "User invalid" });
                return response;
            }

            using (var repo = CreateUserRepository())
            {
                // Check user already exists or username is not allowed.
                var existingUser = repo.GetAll().SingleOrDefault(u => u.Name.Equals(newUser.Name, StringComparison.OrdinalIgnoreCase));
                if (existingUser != null || !IsUsernameAllowed(newUser.Name))
                    return new Response(HttpStatusCode.Conflict);

                // Save new user to database
                newUser.CreatedAt = DateTime.UtcNow;
                newUser.TimelineEtagVersion = IdGenerator.GetId();

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
                newUser.PasswordHash = passwordHash;
                newUser.Password = null;

                repo.Insert(newUser);
            }

            var createdResponse = new CreatedResponse($"/users/{newUser.Name}");
            createdResponse.AddAccessControlAllowOriginHeader();

            return createdResponse;
        }

        public Response GetUser(string username)
        {
            if (username.Equals("whoami", StringComparison.OrdinalIgnoreCase))
            {
                username = Request.CheckAuthorization();
                if (string.IsNullOrEmpty(username))
                {
                    var r1 = new Response(HttpStatusCode.Unauthorized);
                    r1.AddAccessControlAllowOriginHeader();
                    return r1;
                }
            }
            else
            {
                if (!IsUsernameValid(username))
                {
                    var r2 = new Response(HttpStatusCode.Unauthorized);
                    r2.AddAccessControlAllowOriginHeader();
                    return r2;
                }

                Request.CheckAuthorization(username);
            }

            // Get from database.
            User user;
            using (var repo = CreateUserRepository())
            {
                user = repo.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    var r3 = new Response(HttpStatusCode.NotFound);
                    r3.AddAccessControlAllowOriginHeader();
                    return r3;
                }
            }

            return GetJsonResponse(user);
        }

        public Response DeleteUser(string username)
        {
            CheckPassword();

            // Validate
            if (!IsUsernameValid(username))
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "User name invalid" });
                return response;
            }

            // Delete from database.
            using (var followingRepository = CreateFollowingRepository())
            using (var repo = CreateUserRepository())
            {
                var user = repo.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                {
                    var followings = followingRepository.Find(x => x.FollowingUserId.Equals(user.Id, StringComparison.OrdinalIgnoreCase) ||
                                                                   x.UserId.Equals(user.Id, StringComparison.OrdinalIgnoreCase));
                    foreach (var following in followings) followingRepository.Delete(following);

                    repo.Delete(user);
                }
            }

            return new Response(HttpStatusCode.NoContent);
        }

        public Response DeleteAllUsers()
        {
            CheckPassword();

            // Delete from database.
            using (var followingRepository = CreateFollowingRepository())
            using (var repo = CreateUserRepository())
            {
                followingRepository.DeleteAll();
                repo.DeleteAll();
            }

            return new Response(HttpStatusCode.NoContent);
        }
    }
}
