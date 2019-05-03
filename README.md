# Nitwit API documentation

## Users

Create a user:

  POST /users
  { ""user"": ""john"" }


Retrieve all users:

  GET /users


Retrieve a user:

  GET /users/john


Delete a user:

  DELETE /users/john



## Posts

Create a post:

  POST /users/john/posts
  { ""post"": ""This is my first post! :)"" }


Retrieve all posts of a user:

  GET /users/john/posts


Retrieve a post:

  GET /users/john/posts/abc123


Delete a post:

  DELETE /users/john/posts/abc123



## Timeline

Retrieve timeline of a user, i.e. all posts of the user and of those he/she is following:

  GET /users/john/timeline



## Following

Follow another user, i.e. John starts to follow Jack:

  POST /users/john/following
  { ""user"": ""jack"" }


Unfollow a user, i.e. John unfollows Jack:

  DELETE /users/john/following/jack


Retrieve all users and indicate the following status for the given user.
i.e. for the retrieved users you can see whether John follows that user
and/or whether that user is following John:

  GET /users?user=john



