# Nitwit API documentation

## Introduction

The API's endpoint can be found at https://nitwit-api.azurewebsites.net

> When calling this API, in the examples below replace the "john" username by your own.

## Resources

### Posts

Create a post:

```
  POST /users/john/posts
  { "content": "This is my first post! :)" }
```


Retrieve all posts of a user:

```
  GET /users/john/posts
```


Retrieve a post:

```
  GET /users/john/posts/abc123
```


Delete a post:

```
  DELETE /users/john/posts/abc123
```



### Timeline

Retrieve timeline of a user, i.e. all posts of the user and of those he/she is following:

```
  GET /users/john/timeline
```



### Following

Follow another user, i.e. John starts to follow Jack:

```
  POST /users/john/following
  { "user": "jack" }
```


Unfollow a user, i.e. John unfollows Jack:

```
  DELETE /users/john/following/jack
```


Retrieve all users (except the given user) and for each user indicate that user is following
the given user and vice versa.
I.e. for the retrieved users you can see whether John follows that user
and/or whether that user is following John:

```
  GET /users/john/following
```
