using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terminal.Domain.Repositories.Interfaces;
using Terminal.Domain.Entities;
using Terminal.Domain.Objects;
using Terminal.Domain.ExtensionMethods;

namespace Terminal.Domain.Repositories.Objects
{
    public class LinkRepository : ILinkRepository
    {
        /// <summary>
        /// Every repository requires an instance of the Entity Framework data context.
        /// </summary>
        EntityContainer _entityContainer;

        public LinkRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        public void AddLink(Link link)
        {
            _entityContainer.Links.Add(link);
            _entityContainer.SaveChanges();
        }

        public void UpdateLink(Link link)
        {
            _entityContainer.SaveChanges();
        }

        public void DeleteLink(Link link)
        {
            link.LinkComments.ToList().ForEach(x => _entityContainer.LinkComments.Remove(x));
            link.LinkVotes.ToList().ForEach(x => _entityContainer.LinkVotes.Remove(x));
            link.LinkClicks.ToList().ForEach(x => _entityContainer.LinkClicks.Remove(x));
            link.Tags.ToList().ForEach(x => link.Tags.Remove(x));
            _entityContainer.Links.Remove(link);
            _entityContainer.SaveChanges();
        }

        public Link GetLink(long linkId)
        {
            return _entityContainer.Links.SingleOrDefault(x => x.LinkID == linkId);
        }

        public Tag GetTag(string name)
        {
            return _entityContainer.Tags.SingleOrDefault(x => x.Name.ToUpper() == name.ToUpper());
        }

        public IEnumerable<Tag> GetTags()
        {
            return _entityContainer.Tags;
        }

        public CollectionPage<Link> GetLinks(int page, int itemsPerPage, List<string> tags, List<string> searchTerms, string sortBy)
        {
            var links = _entityContainer.Links.AsQueryable();
            var totalLinks = 0;

            if (tags != null && searchTerms != null)
            {
                links = links
                .Where(x => x.Tags.Any(y => tags.Contains(y.Name)) ||
                    searchTerms.Any(y => x.Title.Contains(y)));
            }
            else if (tags != null)
            {
                links = links
                    .Where(x => x.Tags.Any(y => tags.Contains(y.Name)));
            }
            else if (searchTerms != null)
            {
                links = links
                    .Where(x => searchTerms.Any(y => x.Title.Contains(y) || x.Description.Contains(y)));
            }

            switch (sortBy.ToUpper())
            {
                case "RATING":
                    links = links.OrderByDescending(x => x.LinkVotes.Max(y => y.Rating));
                    break;
                case "CLICKS":
                    links = links.OrderByDescending(x => x.LinkClicks.Count);
                    break;
                case "REPLIES":
                    links = links.OrderByDescending(x => x.LinkComments.Count);
                    break;
                default:
                    links = links.OrderByDescending(x => x.Date);
                    break;
            }

            totalLinks = links.Count();

            var totalPages = totalLinks.NumberOfPages(itemsPerPage);
            if (totalPages <= 0)
                totalPages = 1;

            if (page > totalPages)
                return GetLinks(totalPages, itemsPerPage, tags, searchTerms, sortBy);
            else if (page < 1)
                return GetLinks(1, itemsPerPage, tags, searchTerms, sortBy);
            else
            {
                if (totalLinks > itemsPerPage)
                    links = links
                        .Skip((page - 1) * itemsPerPage)
                        .Take(itemsPerPage);

                return new CollectionPage<Link>
                {
                    Items = links.AsEnumerable().Reverse().ToList(),
                    TotalItems = totalLinks,
                    TotalPages = totalPages
                };
            }
        }

        public void AddComment(LinkComment linkComment)
        {
            _entityContainer.LinkComments.Add(linkComment);
            _entityContainer.SaveChanges();
        }

        public void UpdateComment(LinkComment linkComment)
        {
            _entityContainer.SaveChanges();
        }

        public void DeleteComment(LinkComment linkComment)
        {
            _entityContainer.LinkComments.Remove(linkComment);
            _entityContainer.SaveChanges();
        }

        public LinkComment GetComment(int linkCommentId)
        {
            return _entityContainer.LinkComments.SingleOrDefault(x => x.CommentID == linkCommentId);
        }

        public CollectionPage<LinkComment> GetComments(long linkId, int page, int itemsPerPage)
        {
            var totalComments = 0;
            var comments = _entityContainer.LinkComments
                .Where(x => x.LinkID == linkId)
                .OrderBy(x => x.Date);

            totalComments = comments.Count();


            int totalPages = totalComments.NumberOfPages(itemsPerPage);
            if (totalPages <= 0)
                totalPages = 1;

            if (page > totalPages)
                return GetComments(linkId, totalPages, itemsPerPage);
            else if (page < 1)
                return GetComments(linkId, 1, itemsPerPage);
            else
            {
                if (totalComments > itemsPerPage)
                    comments = comments
                        .Skip((page - 1) * itemsPerPage)
                        .Take(itemsPerPage)
                        .OrderBy(x => x.Date);

                return new CollectionPage<LinkComment>
                {
                    Items = comments.ToList(),
                    TotalItems = totalComments,
                    TotalPages = totalPages
                };
            }
        }
    }
}
