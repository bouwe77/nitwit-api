using Data.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Data.Repositories
{
    public class PostRepository : SqliteRepository<Post>
    {
        public PostRepository(string databaseFilePath)
            : base(databaseFilePath)
        {
        }

        public IEnumerable<Post> GetTimeline(string userId)
        {
            var query = @"
SELECT * FROM posts
WHERE userid = ?
OR userid IN (SELECT followinguserid FROM following WHERE userid = ?)
OR id IN (SELECT postid FROM mentions WHERE mentioneduserid = ?)
OR id IN (SELECT postid FROM replies WHERE replieduserid = ?)
ORDER BY createdat DESC";

            var sameUserIdRepeated = new[] { userId, userId, userId, userId };

            return _sqliteConnection.Query<Post>(query, sameUserIdRepeated);
        }
    }
}
