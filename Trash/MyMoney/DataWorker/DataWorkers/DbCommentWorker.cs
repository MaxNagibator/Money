using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Mapping;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Common;
using Common.Core;
using Common.Exceptions;
using Extentions;

namespace DataWorker
{
    public class DbCommentWorker
    {
        public static List<Comment> GetComments()
        {
            using (var context = new Data.DataContext())
            {
                var dbComments = context.Comments.OrderByDescending(x=>x.Id).ToList();
                var comments = new List<Comment>();
                foreach(var dbComment in dbComments)
                {
                    var comment = new Comment();
                    comment.Text = dbComment.Text;
                    comment.Title = dbComment.Title;
                    comment.Author = dbComment.Author;
                    comment.CreateDate = dbComment.CreateDate.ToString("yyyy.MM.dd HH:mm:ss");
                    comments.Add(comment);
                }
                return comments;
            }
        }
        public static void CreateComment(string title, string text, string author)
        {
            using (var context = new Data.DataContext())
            {
                var dbComment = new Data.Comment();
                if(title.Length > 1000)
                {
                    throw new MessageException("Заголовок максимум 1000 символов, превышено на " + (title.Length - 1000));
                }
                if (author.Length > 1000)
                {
                    throw new MessageException("Автор максимум 1000 символов, превышено на " + (title.Length - 1000));
                }
                dbComment.Text = text;
                dbComment.Title = title;
                dbComment.Author = author;
                dbComment.CreateDate = DateTime.Now;
                context.Comments.Add(dbComment);
                context.SaveChanges();
            }
        }
    }
}
