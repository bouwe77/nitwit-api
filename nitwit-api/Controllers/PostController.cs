using Data;
using Data.Entities;
using Dolores.Http;
using Dolores.Requests;
using Dolores.Responses;
using nitwitapi.Extensions;
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
            if (!IsUsernameValid(username))
                return new Response(HttpStatusCode.Unauthorized);

            Request.CheckAuthorization(username);

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
            if (!IsPostContentValid(newPost.Content))
            {
                var response = new Response(HttpStatusCode.BadRequest);
                response.Json(new { Message = "Post content invalid" });
                return response;
            }

            using (var userRepository = CreateUserRepository())
            using (var postRepository = CreatePostRepository())
            using (var followingRepository = CreateFollowingRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                newPost.Username = user.Name;
                newPost.UserId = user.Id;
                newPost.CreatedAt = DateTime.UtcNow;

                SaveMentions(newPost);
                SaveReplies(newPost);

                // Insert post into database.
                postRepository.Insert(newPost);

                // Update the user's Etag versions because both the timeline and posts have changed.
                user.TimelineEtagVersion = IdGenerator.GetId();
                user.PostsEtagVersion = IdGenerator.GetId();
                userRepository.Update(user);

                // Update the user's followers TimelineEtagVersion too because their timeline has also changed.
                var followersOfPostingUser = followingRepository.Find(x => x.FollowingUserId == user.Id).Select(x => x.UserId);
                foreach (var userId in followersOfPostingUser)
                {
                    var follower = userRepository.GetById(userId);
                    follower.TimelineEtagVersion = IdGenerator.GetId();
                    userRepository.Update(follower);
                }
            }

            return new CreatedResponse($"/users/{newPost.Id}");
        }

        public Response GetAllPosts(string username)
        {
            // Note: this call requires no authentication.

            // Validate
            if (!IsUsernameValid(username))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "User name invalid" });
                return badRequestResponse;
            }

            // Read from database
            string currentPostsEtag;
            var posts = new List<Post>();
            using (var userRepository = CreateUserRepository())
            using (var postRepository = CreatePostRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                // Get the If-Not-Match ETag from the request and if present, 
                // compare against the current timeline version number.
                currentPostsEtag = user.PostsEtagVersion;
                var requestedTimelineEtag = Request.GetHeaderValue(HttpRequestHeaderFields.IfNoneMatch);
                if (!string.IsNullOrWhiteSpace(requestedTimelineEtag))
                {
                    // If the requested ETag is equal to the current posts version number 
                    // the client is already up to date and no response data needs to be sent.
                    if (requestedTimelineEtag.Equals(currentPostsEtag, StringComparison.OrdinalIgnoreCase))
                    {
                        var responseNotModified = new Response(HttpStatusCode.NotModified);
                        responseNotModified.SetEtagHeader(currentPostsEtag);
                        return responseNotModified;
                    }
                }

                posts = postRepository
                    .GetAll()
                    .Where(p => p.UserId == user.Id)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();
            }

            var response = GetJsonResponse(posts);
            response.SetEtagHeader(currentPostsEtag);

            return response;
        }

        public Response GetAllPosts()
        {
            CheckPassword();

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
            if (!IsUsernameValid(username))
                return new Response(HttpStatusCode.Unauthorized);

            Request.CheckAuthorization(username);

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

            // Get the If-Not-Match ETag from the request and if present, 
            // compare against the current timeline version number.
            var requestedTimelineEtag = Request.GetHeaderValue(HttpRequestHeaderFields.IfNoneMatch);
            if (!string.IsNullOrWhiteSpace(requestedTimelineEtag))
            {
                // If the requested ETag is equal to the current timeline version number 
                // the client is already up to date and no response data needs to be sent.
                if (requestedTimelineEtag.Equals(currentTimelineEtag, StringComparison.OrdinalIgnoreCase))
                {
                    var responseNotModified = new Response(HttpStatusCode.NotModified);
                    responseNotModified.SetEtagHeader(currentTimelineEtag);
                    return responseNotModified;
                }
            }

            // Read from database
            var posts = new List<Post>();
            using (var followingRepository = CreateFollowingRepository())
            using (var postRepository = CreatePostRepository())
            {
                posts = postRepository.GetTimeline(user.Id).ToList();
            }

            var response = GetJsonResponse(posts);
            response.SetEtagHeader(currentTimelineEtag);

            return response;
        }

        public Response GetPost(string username, string postId)
        {
            // Note: this call requires no authentication.

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

            using (var mentionRepository = CreateMentionRepository())
            using (var replyRepository = CreateReplyRepository())
            using (var postRepository = CreatePostRepository())
            {
                mentionRepository.DeleteAll();
                replyRepository.DeleteAll();
                postRepository.DeleteAll();

                return new Response(HttpStatusCode.NoContent);
            }
        }

        public Response DeleteAllPosts(string username)
        {
            CheckPassword();

            if (!IsUsernameValid(username))
                return new Response(HttpStatusCode.Unauthorized);

            // Delete from database
            using (var userRepository = CreateUserRepository())
            using (var postRepository = CreatePostRepository())
            using (var mentionRepository = CreateMentionRepository())
            using (var replyRepository = CreateReplyRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                // Delete posts
                var posts = postRepository.GetAll().Where(p => p.UserId == user.Id).ToList();
                foreach (var post in posts) postRepository.Delete(post);

                // Delete mentions and replies.
                var mentions = mentionRepository.GetAll().Where(m => posts.Select(p => p.Id).Contains(m.PostId));
                foreach (var mention in mentions) mentionRepository.Delete(mention);
                var replies = replyRepository.GetAll().Where(r => posts.Select(p => p.Id).Contains(r.PostId));
                foreach (var reply in replies) replyRepository.Delete(reply);
            }

            return new Response(HttpStatusCode.NoContent);
        }

        public Response DeletePost(string username, string postId)
        {
            if (!IsUsernameValid(username))
                return new Response(HttpStatusCode.Unauthorized);

            Request.CheckAuthorization(username);

            if (!IsPostIdentifierValid(postId))
            {
                var badRequestResponse = new Response(HttpStatusCode.BadRequest);
                badRequestResponse.Json(new { Message = "Post identifier invalid" });
                return badRequestResponse;
            }

            // Delete from database
            using (var userRepository = CreateUserRepository())
            using (var postRepository = CreatePostRepository())
            using (var mentionRepository = CreateMentionRepository())
            using (var replyRepository = CreateReplyRepository())
            {
                var user = userRepository.GetAll().SingleOrDefault(u => u.Name.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                    return new Response(HttpStatusCode.NotFound);

                var post = postRepository.GetById(postId);
                if (post == null || post.UserId != user.Id)
                    return new Response(HttpStatusCode.NoContent);

                // Delete mentions and replies.
                var mentions = mentionRepository.Find(m => m.PostId == post.Id);
                foreach (var mention in mentions) mentionRepository.Delete(mention);
                var replies = replyRepository.Find(r => r.PostId == post.Id);
                foreach (var reply in replies) replyRepository.Delete(reply);

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
