using Data.Entities;
using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nitwitapi.Controllers
{
    public class PostController : ControllerBase
    {
        public Response AddPost(string username)
        {
            // Deserialize
            Post newPost;
            try
            {
                newPost = Request.MessageBody.DeserializeJson<Post>();
            }
            catch (Exception)
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "Request body is not a valid Post" });
                return response;
            }

            // Validate
            if (!IsUsernameValid(username))
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "User name invalid" });
                return response;
            }
            if (!IsPostContentValid(newPost.Content))
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "Post content invalid" });
                return response;
            }

            using (var userRepository = CreateUserRepository())
            using (var postRepository = CreatePostRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                newPost.Username = user.Name;
                newPost.UserId = user.Id;
                newPost.CreatedAt = DateTime.Now;

                // Save to database
                postRepository.Insert(newPost);
            }

            return new CreatedResponse($"/users/{newPost.Id}");
        }

        public Response GetAllPosts(string username)
        {
            // Validate
            if (!IsUsernameValid(username))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }

            // Read from database
            var posts = new List<Post>();
            using (var userRepository = CreateUserRepository())
            using (var postRepository = CreatePostRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                posts = postRepository
                    .GetAll()
                    .Where(p => p.UserId == user.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();
            }

            var response = GetJsonResponse(posts);

            return response;
        }

        public Response GetTimeline(string username)
        {
            // Validate
            if (!IsUsernameValid(username))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }

            // Read from database
            var posts = new List<Post>();
            using (var userRepository = CreateUserRepository())
            using (var followingRepository = CreateFollowingRepository())
            using (var postRepository = CreatePostRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                var followers = followingRepository
                    .GetAll()
                    .Where(f => f.UserId.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    .Select(f => f.FollowingUserId)
                    .ToList();
                followers.Add(user.Id);

                posts = postRepository
                    .GetAll()
                    .Where(p => followers.Contains(p.UserId))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                var year = 2016;
                var month = 1;
                foreach (var post in posts)
                {
                    post.CreatedAt = new DateTime(year++, month++, 1);
                }
            }

            var response = GetJsonResponse(posts);

            return response;
        }

        public Response GetPost(string username, string postId)
        {
            // Validate
            if (!IsUsernameValid(username))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }
            if (!IsPostIdentifierValid(postId))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "Post identifier invalid" });
                return badRequestResponse;
            }

            // Read from database
            Post post;
            using (var userRepository = CreateUserRepository())
            using (var postRepository = CreatePostRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                post = postRepository.GetById(postId);
                if (post == null || post.UserId != user.Id)
                    return new Response(HttpStatusCode.NotFound);
            }

            var response = GetJsonResponse(post);

            return response;
        }

        public Response DeleteAllPosts(string username)
        {
            // Check "password"
            var pass = Request.GetQueryStringValue("pass");
            if (string.IsNullOrWhiteSpace(pass) || pass != "z0BnkB7E2ET01qaN")
                return new Response(HttpStatusCode.MethodNotAllowed);

            // Validate
            if (!IsUsernameValid(username))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }

            // Delete from database
            using (var userRepository = CreateUserRepository())
            using (var postRepository = CreatePostRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                // Delete posts
                var posts = postRepository.GetAll().Where(p => p.UserId == user.Id).ToList();
                foreach (var post in posts)
                    postRepository.Delete(post);
            }

            return new Response(HttpStatusCode.NoContent);
        }

        public Response DeletePost(string username, string postId)
        {
            // Validate
            if (!IsUsernameValid(username))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }
            if (!IsPostIdentifierValid(postId))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "Post identifier invalid" });
                return badRequestResponse;
            }

            // Delete from database
            using (var userRepository = CreateUserRepository())
            using (var postRepository = CreatePostRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                var post = postRepository.GetById(postId);
                if (post == null || post.UserId != user.Id)
                    return new Response(HttpStatusCode.NoContent);

                postRepository.Delete(post);
            }

            return new Response(HttpStatusCode.NoContent);
        }
    }
}
