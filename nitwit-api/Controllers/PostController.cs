using Data;
using Data.Entities;
using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using nitwitapi.Logic;
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

                SaveMentions(newPost);
                SaveReplies(newPost);

                // Insert post into database.
                postRepository.Insert(newPost);

                // Update the user's TimelineEtagVersion because its timeline has changed.
                user.TimelineEtagVersion = IdGenerator.GetId();
                userRepository.Update(user);
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

        public Response GetAllPosts()
        {
            // Read from database
            var posts = new List<Post>();
            using (var postRepository = CreatePostRepository())
            {
                posts = postRepository
                    .GetAll()
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

            // Get the user from the database.
            string currentTimelineEtag;
            User user;
            using (var userRepository = CreateUserRepository())
            {
                user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                currentTimelineEtag = user.TimelineEtagVersion;
            }

            // Get the If-Not_Match ETag from the request and if present, 
            // compare against the current timeline version number.
            var requestedTimelineEtag = Request.GetHeaderValue(HttpRequestHeaderFields.IfNoneMatch);
            if (!string.IsNullOrWhiteSpace(requestedTimelineEtag))
            {
                // If the requested ETag is equal to the current timeline version number 
                // the client is already up to date and no response data needs to be sent.
                if (requestedTimelineEtag.Equals(currentTimelineEtag, StringComparison.OrdinalIgnoreCase))
                    return new Response(HttpStatusCode.NotModified);
            }

            // Read from database
            var posts = new List<Post>();
            using (var followingRepository = CreateFollowingRepository())
            using (var postRepository = CreatePostRepository())
            {
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
            }

            var response = GetJsonResponse(posts);
            response.SetHeader("ETag", currentTimelineEtag);

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

        public Response DeleteAllPosts()
        {
            CheckPassword();

            using (var postRepository = CreatePostRepository())
            {
                postRepository.DeleteAll();
                return new Response(HttpStatusCode.NoContent);
            }
        }

        public Response DeleteAllPosts(string username)
        {
            CheckPassword();

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

        private void SaveReplies(Post newPost)
        {
            var repliedUsernames = new ReplyLogic().GetRepliedUsernames(newPost.Content);

            if (!repliedUsernames.Any())
                return;

            using (var userRepository = CreateUserRepository())
            using (var replyRepository = CreateReplyRepository())
            {
                foreach (var username in repliedUsernames)
                {
                    var user = userRepository
                        .Find(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    if (user != null)
                    {
                        // Insert a reply for the post/user combination.
                        var reply = new Reply { PostId = newPost.Id, RepliedUserId = user.Id };
                        replyRepository.Insert(reply);

                        // Update the user's TimelineEtagVersion because its timeline has changed.
                        user.TimelineEtagVersion = IdGenerator.GetId();
                        userRepository.Update(user);
                    }
                }
            }
        }

        private void SaveMentions(Post newPost)
        {
            var mentionedUsernames = new MentionLogic().GetMentionedUsernames(newPost.Content);

            if (!mentionedUsernames.Any())
                return;

            using (var userRepository = CreateUserRepository())
            using (var mentionRepository = CreateMentionRepository())
            {
                foreach (var username in mentionedUsernames)
                {
                    var user = userRepository
                        .Find(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    if (user != null)
                    {
                        // Insert a mention for the post/user combination.
                        var mention = new Mention { PostId = newPost.Id, MentionedUserId = user.Id };
                        mentionRepository.Insert(mention);

                        // Update the user's TimelineEtagVersion because its timeline has changed.
                        user.TimelineEtagVersion = IdGenerator.GetId();
                        userRepository.Update(user);
                    }
                }
            }
        }
    }
}
