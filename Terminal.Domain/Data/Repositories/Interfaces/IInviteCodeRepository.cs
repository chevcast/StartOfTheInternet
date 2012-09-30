using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Terminal.Core.Data.Entities;

namespace Terminal.Core.Data.Repositories.Interfaces
{
    public interface IInviteCodeRepository
    {
        void AddInviteCode(InviteCode inviteCode);
        void DeleteInviteCode(InviteCode inviteCode);
        InviteCode GetInviteCode(string code);
        IEnumerable<InviteCode> GetInviteCodes(string username);
    }
}