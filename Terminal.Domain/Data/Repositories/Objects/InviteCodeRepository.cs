using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Domain;
using Terminal.Domain.Data.Repositories.Interfaces;
using Terminal.Domain.Data.Entities;

namespace Terminal.Domain.Data.Repositories.Objects
{
    public class InviteCodeRepository : IInviteCodeRepository
    {
        /// <summary>
        /// Every repository requires an instance of the Entity Framework data context.
        /// </summary>
        EntityContainer _entityContainer;

        public InviteCodeRepository(EntityContainer entityContainer)
        {
            _entityContainer = entityContainer;
        }

        public void AddInviteCode(InviteCode inviteCode)
        {
            _entityContainer.InviteCodes.Add(inviteCode);
        }

        public void DeleteInviteCode(InviteCode inviteCode)
        {
            _entityContainer.InviteCodes.Remove(inviteCode);
        }

        public InviteCode GetInviteCode(string code)
        {
            return _entityContainer.InviteCodes.SingleOrDefault(x => x.Code.ToUpper() == code.ToUpper());
        }

        public IEnumerable<InviteCode> GetInviteCodes(string username)
        {
            return _entityContainer.InviteCodes.Where(x => x.Username.ToUpper() == username.ToUpper());
        }
    }
}