﻿using Reddit.Extensions;
using Reddit.Things.API.Enums;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;

namespace Reddit.Things.API
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Link : Thing
    {
        #region Properties

        public string Domain { get; set; }

        public Thing BannedBy { get; set; }

        public string MediaEmbed { get; set; }

        public string SelfContentHtml { get; set; }

        private string _SelfContent;

        public string SelfContent
        {
            get { return _SelfContent; }
            set
            {
                if (!IsSelf) throw new Exception(this.ToString() + " is not a self post!");
                _SelfContent = value;
                Edited = true;
                string Response = Connection.Post("api/editusertext", "text=" + value + "&thing_id=" + this.ToString());
            }
        }

        public int Likes { get; set; }

        public string LinkFlairText { get; set; }

        public bool Clicked { get; set; }

        public string Title { get; set; }

        public int NumComments { get; set; }

        public int Score { get; set; }

        public Thing ApprovedBy { get; set; }

        public bool Over18 { get; set; }

        public bool Hidden { get; set; }

        public string Thumbnail { get; set; }

        private string SubredditName;
        private Subreddit _Subreddit;

        public Subreddit Subreddit
        {
            get
            {
                if (_Subreddit == null)
                {
                    string Response = Connection.Get("r/" + SubredditName + ".json");
                    _Subreddit = Subreddit.Create(SubredditName, SimpleJSON.JSONDecoder.Decode(Response)["data"]);
                }
                return _Subreddit;
            }
        }

        public bool Edited { get; set; }

        public string LinkFlairCSSClass { get; set; }

        public string AuthorFlairCSSClass { get; set; }

        public int Downvotes { get; set; }

        public bool Saved { get; set; }

        public bool IsSelf { get; set; }

        public string Permalink { get; set; }

        public DateTime Created { get; set; }

        public string Url { get; set; }

        public string AuthorFlairText { get; set; }

        public string AuthorName { get; set; }

        private User _Author;

        public User Author
        {
            get
            {
                if (_Author == null)
                {
                    string Response = Connection.Get("user/" + AuthorName + "/about.json");
                    _Author = User.Create(SimpleJSON.JSONDecoder.Decode(Response)["data"]);
                }
                return _Author;
            }
        }

        public DateTime CreatedUTC { get; set; }

        public string Media { get; set; }

        public string NumReports { get; set; }

        public int Upvotes { get; set; }

        private List<Comment> _Comments;

        public List<Comment> Comments
        {
            get
            {
                if (_Comments == null)
                {
                    _Comments = GetComments();
                }
                return _Comments;
            }
            set { _Comments = value; }
        }

        #endregion

        #region Functions

        /// <summary>
        ///
        /// </summary>
        /// <param name="Sort">one of: SortBy.Hot, SortBy.New, SortBy.Old, SortBy.Top, SortBy.Controversial</param>
        /// <returns></returns>
        public List<Comment> GetComments(SortBy Sort = null)
        {
            if (Sort == null)
            {
                Sort = SortBy.Hot;
            }
            if (Sort == SortBy.Best || Sort == SortBy.Old)
            {
                throw new Exception("Can't apply SortBy.Best or SortBy.Old in this context");
            }
            string Response = Connection.Get("comments/" + ID + ".json", "sort=" + Sort.Arg);
            var Comments = new List<Comment>();
            foreach (var Comment in SimpleJSON.JSONDecoder.Decode(Response)[1]["data"]["children"].ArrayValue)
            {
                var CommentObj = API.Comment.Create(Comment["data"]);
                CommentObj._Link = this;
                Comments.Add(CommentObj);
            }
            return Comments;
        }

        public Thing Comment(string CommentMarkdown)
        {
            string PostData = new StringBuilder()
                .Append("thing_id=").Append(this.ToString())
                .Append("&text=").Append(CommentMarkdown)
                .ToString();
            string Response = Connection.Post("api/comment", PostData);
            return Thing.Get(SimpleJSON.JSONDecoder.Decode(Response)["json"]["data"]["things"].ArrayValue[0]["data"]["id"].StringValue);
        }

        public void EditContent(string Content)
        {
            this.SelfContent = Content;
        }

        public List<Link> OtherDiscussions()
        {
            string Response = Connection.Get("api/info.json", "url=" + this.Url);
            var Links = new List<Link>();
            foreach (var Link in SimpleJSON.JSONDecoder.Decode(Response)["data"]["children"].ArrayValue)
            {
                Links.Add(API.Link.Create(Link["data"]));
            }
            return Links;
        }

        #endregion

        #region Factory

        internal static Link Create(JObject Json)
        {
            var Temp = new Link();

            Temp.ID = Json["id"].StringValue;
            Temp.Kind = Kind.Link;
            Temp.Domain = Json["domain"].StringValue;
            //Temp.BannedBy = null;
            //Temp.MediaEmbed = null;
            Temp.SubredditName = Json["subreddit"].StringValue;
            Temp.SelfContentHtml = Json["selftext_html"].StringValue;
            Temp._SelfContent = Json["selftext"].StringValue;
            //Temp.Likes = Json["likes"].IntValue;
            Temp.LinkFlairText = Json["link_flair_text"].StringValue;
            Temp.Clicked = Json["clicked"].BooleanValue;
            Temp.Title = Json["title"].StringValue;
            Temp.NumComments = Json["num_comments"].IntValue;
            Temp.Score = Json["score"].IntValue;
            //Temp.ApprovedBy = null;
            Temp.Over18 = Json["over_18"].BooleanValue;
            Temp.Hidden = Json["hidden"].BooleanValue;
            Temp.Thumbnail = Json["thumbnail"].StringValue;
            Temp.Edited = Json["edited"].BooleanValue;
            Temp.LinkFlairCSSClass = Json["link_flair_css_class"].StringValue;
            Temp.AuthorFlairCSSClass = Json["author_flair_css_class"].StringValue;
            Temp.Downvotes = Json["downs"].IntValue;
            Temp.Saved = Json["saved"].BooleanValue;
            Temp.IsSelf = Json["is_self"].BooleanValue;
            Temp.Permalink = Json["permalink"].StringValue;
            Temp.Created = Json["created"].DoubleValue.ToDateTime();
            Temp.CreatedUTC = Json["created_utc"].DoubleValue.ToDateTime();
            Temp.Url = Json["url"].StringValue;
            Temp.AuthorFlairText = Json["author_flair_text"].StringValue;
            Temp.AuthorName = Json["author"].StringValue;
            //Temp.Media = null;
            //Temp.NumReports = null;
            Temp.Upvotes = Json["ups"].IntValue;

            return Temp;
        }

        internal static Link Create(string Input)
        {
            return Link.Create(SimpleJSON.JSONDecoder.Decode(Input)["data"]["children"][0]);
        }

        #endregion
    }
}