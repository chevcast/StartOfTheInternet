using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Domain.Data.Entities;

namespace Terminal.Domain.Data.Repositories.Interfaces
{
    public interface IInviteCodeRepository
    {
        void AddInviteCode(InviteCode inviteCode);
        void DeleteInviteCode(InviteCode inviteCode);
        InviteCode GetInviteCode(string code);
        IEnumerable<InviteCode> GetInviteCodes(string username);
    }
}