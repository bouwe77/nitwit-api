using Data;
using Data.Entities;
using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nitwitapi.Controllers
{
    public class UserController : ControllerBase
    {
        public Response GetAllUsers()
        {
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
            if (newUser == null || !IsUsernameValid(newUser.Name))
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "User name invalid" });
                return response;
            }

            using (var repo = CreateUserRepository())
            {
                // Check user already exists.
                var existingUser = repo.GetAll().SingleOrDefault(u => u.Name.Equals(newUser.Name, StringComparison.OrdinalIgnoreCase));
                if (existingUser != null)
                    return new Response(HttpStatusCode.Conflict);

                // Save new user to database
                newUser.CreatedAt = DateTime.Now;
                newUser.TimelineEtagVersion = IdGenerator.GetId();
                repo.Insert(newUser);
            }

            return new CreatedResponse($"/users/{newUser.Name}");
        }

        public Response GetUser(string username)
        {
            // Validate
            if (!IsUsernameValid(username))
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "User name invalid" });
                return response;
            }

            // Get from database.
            User user;
            using (var repo = CreateUserRepository())
            {
                user = repo.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);
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
