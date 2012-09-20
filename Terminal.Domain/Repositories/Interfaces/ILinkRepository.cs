using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Domain.Entities;
using Terminal.Domain.Objects;

namespace Terminal.Domain.Repositories.Interfaces
{
    public interface ILinkRepository
    {
        void AddLink(Link link);
        void UpdateLink(Link link);
        void DeleteLink(Link link);
        Link GetLink(long linkId);
        Tag GetTag(string name);
        IEnumerable<Tag> GetTags();
        CollectionPage<Link> GetLinks(
            int page,
            int itemsPerPage,
            List<string> tags,
            List<string> searchTerms,
            string sortBy
        );
        void AddComment(LinkComment linkComment);
        void UpdateComment(LinkComment linkComment);
        void DeleteComment(LinkComment linkComment);
        LinkComment GetComment(int linkCommentId);
        CollectionPage<LinkComment> GetComments(long linkId, int page, int itemsPerPage);
    }
}