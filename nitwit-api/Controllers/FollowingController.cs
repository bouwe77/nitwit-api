using Data.Entities;
using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using System;
using System.Linq;

namespace nitwitapi.Controllers
{
    public class FollowingController : ControllerBase
    {
        public Response AddFollowing(string username)
        {
            // Deserialize
            Following newFollowing;
            try
            {
                newFollowing = Request.MessageBody.DeserializeJson<Following>();
            }
            catch (Exception)
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "Request body is not a valid Following" });
                return response;
            }

            // Validate
            if (!IsUsernameValid(username))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }
            if (!IsUsernameValid(newFollowing.FollowingUsername))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }

            if (username.Equals(newFollowing.FollowingUsername, StringComparison.OrdinalIgnoreCase))
                return new Response(HttpStatusCode.BadRequest);

            using (var userRepository = CreateUserRepository())
            using (var followingRepository = CreateFollowingRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                var followingUser = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(newFollowing.FollowingUsername, StringComparison.OrdinalIgnoreCase));
                if (followingUser == null)
                    return new Response(HttpStatusCode.NotFound);

                var existingFollowing = followingRepository
                    .GetAll()
                    .Where(f => f.UserId.Equals(user.Id, StringComparison.OrdinalIgnoreCase)
                    && f.FollowingUserId.Equals(followingUser.Id, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();

                if (existingFollowing == null)
                {
                    newFollowing.UserId = user.Id;
                    newFollowing.FollowingUserId = followingUser.Id;
                    newFollowing.CreatedAt = DateTime.Now;

                    // Save to database
                    followingRepository.Insert(newFollowing);
                }
            }

            return new CreatedResponse($"/users/{username}/following/{newFollowing.FollowingUsername}");
        }

        public Response DeleteFollowing(string followerUsername, string followingUsername)
        {
            // Validate
            if (!IsUsernameValid(followerUsername))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }
            if (!IsUsernameValid(followingUsername))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }

            if (followerUsername.Equals(followingUsername, StringComparison.OrdinalIgnoreCase))
                return new Response(HttpStatusCode.BadRequest);

            using (var userRepository = CreateUserRepository())
            using (var followingRepository = CreateFollowingRepository())
            {
                var followerUser = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(followerUsername, StringComparison.OrdinalIgnoreCase));
                if (followerUser == null)
                    return new Response(HttpStatusCode.NotFound);

                var followingUser = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(followingUsername, StringComparison.OrdinalIgnoreCase));
                if (followingUser == null)
                    return new Response(HttpStatusCode.NotFound);

                var existingFollowing = followingRepository
                    .GetAll()
                    .Where(f => f.UserId.Equals(followerUser.Id, StringComparison.OrdinalIgnoreCase)
                    && f.FollowingUserId.Equals(followingUser.Id, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();

                if (existingFollowing != null)
                {
                    // Delete from database.
                    followingRepository.Delete(existingFollowing);
                }
            }

            return new Response(HttpStatusCode.NoContent);
        }

        public Response EveryoneFollowsEachOther()
        {
            // Check "password"
            var pass = Request.GetQueryStringValue("pass");
            if (string.IsNullOrWhiteSpace(pass) || pass != "z0BnkB7E2ET01qaN")
                return new Response(HttpStatusCode.NotFound);

            // Replace current Following data by data that indicates all current users are following each other.
            using (var userRepository = CreateUserRepository())
            using (var followingRepository = CreateFollowingRepository())
            {
                // Delete all current following data.
                followingRepository.DeleteAll();

                var users1 = userRepository.GetAll();
                var users2 = userRepository.GetAll();
                foreach (var user1 in users1)
                {
                    foreach (var user2 in users2)
                    {
                        if (user1.Id != user2.Id)
                        {
                            var following = new Following
                            {
                                UserId = user1.Id,
                                FollowingUserId = user2.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            followingRepository.Insert(following);
                        }
                    }
                }
            }

                return new Response(HttpStatusCode.NoContent);

        }
    }
}
