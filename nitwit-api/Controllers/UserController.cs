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
            // =============== Following =======================
            var user = Request.GetQueryStringValue("user");
            if (!string.IsNullOrWhiteSpace(user))
                return GetFollowingInfoForUser(user);
            // =================================================

            // Get from database.
            List<User> users;
            using (var repo = CreateUserRepository())
            {
                users = repo.GetAll().ToList();
            }

            var response = GetJsonResponse(users);

            return response;
        }

        private Response GetFollowingInfoForUser(string username)
        {
            // Validate
            if (!IsUsernameValid(username))
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "User name invalid" });
                return response;
            }

            // Get from database.
            List<User> allUsersExceptUserFromRequest;
            using (var userRepo = CreateUserRepository())
            using (var followingRepo = CreateFollowingRepository())
            {
                var userFromRequest = userRepo.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (userFromRequest == null)
                    return new Response(HttpStatusCode.NotFound);

                allUsersExceptUserFromRequest = userRepo.GetAll().Where(u => !u.Name.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();

                var usersYouAreFollowing = followingRepo.GetAll().Where(f => f.UserId == userFromRequest.Id).Select(f => f.FollowingUserId).ToList();
                var usersFollowingYou = followingRepo.GetAll().Where(f => f.FollowingUserId == userFromRequest.Id).Select(f => f.UserId).ToList();

                var users = new List<object>();
                foreach (var user in allUsersExceptUserFromRequest)
                {
                    users.Add(new
                    {
                        user = user.Name,
                        youAreFollowing = usersYouAreFollowing.Contains(user.Id),
                        isFollowingYou = usersFollowingYou.Contains(user.Id)
                    });
                }

                return GetJsonResponse(users);
            }
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

            // Get from database.
            using (var repo = CreateUserRepository())
            {
                var existingUser = repo.GetAll().SingleOrDefault(u => u.Name.Equals(newUser.Name, StringComparison.OrdinalIgnoreCase));
                if (existingUser != null)
                    return new Response(HttpStatusCode.Conflict);

                newUser.CreatedAt = DateTime.Now;

                // Save to database
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
            using (var repo = CreateUserRepository())
            {
                var user = repo.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user != null)
                    repo.Delete(user);
            }

            return new Response(HttpStatusCode.NoContent);
        }

        public Response DeleteAllUsers()
        {
            CheckPassword();

            // Delete from database.
            using (var repo = CreateUserRepository())
            {
                repo.DeleteAll();
            }

            return new Response(HttpStatusCode.NoContent);
        }
    }
}
