using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains;

public static partial class AclRequests {
    public class CreateCrmContactReq : Command {
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public CreateCrmContactReq(
            string xrefId,
            string firstName,
            string lastName,
            string email) {
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }

    public class CreateCrmContactResp : Event {
        public readonly string XrefId;
        public readonly bool IsOk;

        public CreateCrmContactResp(
            string xrefId,
            bool isOk) {
            XrefId = xrefId;
            IsOk = isOk;
        }
    }

    public class CreateErpContactReq : Command {
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public CreateErpContactReq(
            string xrefId,
            string firstName,
            string lastName,
            string email) {
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }

    public class CreateErpContactResp : Event {
        public readonly string XrefId;
        public readonly bool IsOk;

        public CreateErpContactResp(
            string xrefId,
            bool isOk) {
            XrefId = xrefId;
            IsOk = isOk;
        }
    }

    public class ArchiveCrmContactReq : Command {
        public readonly string XrefId;

        public ArchiveCrmContactReq(
            string xrefId) {
            XrefId = xrefId;
        }
    }

    public class ArchiveCrmContactResp : Event {
        public readonly string XrefId;
        public readonly bool IsOk;

        public ArchiveCrmContactResp(
            string xrefId,
            bool isOk) {
            XrefId = xrefId;
            IsOk = isOk;
        }
    }

    public class ArchiveErpContactReq : Command {
        public readonly string XrefId;

        public ArchiveErpContactReq(
            string xrefId) {
            XrefId = xrefId;
        }
    }

    public class ArchiveErpContactResp : Event {
        public readonly string XrefId;
        public readonly bool IsOk;

        public ArchiveErpContactResp(
            string xrefId,
            bool isOk) {
            XrefId = xrefId;
            IsOk = isOk;
        }
    }

    public class UpdateErpContactDetailsReq : Command {
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public UpdateErpContactDetailsReq(
            string xrefId,
            string firstName,
            string lastName,
            string email) {
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }

    public class UpdateErpContactDetailsResp : Event {
        public readonly string XrefId;
        public readonly bool IsOk;

        public UpdateErpContactDetailsResp(
            string xrefId,
            bool isOk) {
            XrefId = xrefId;
            IsOk = isOk;
        }
    }

    public class UpdateCrmContactDetailsReq : Command {
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;

        public UpdateCrmContactDetailsReq(
            string xrefId,
            string firstName,
            string lastName,
            string email) {
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }
    }

    public class UpdateCrmContactDetailsResp : Event {
        public readonly string XrefId;
        public readonly bool IsOk;

        public UpdateCrmContactDetailsResp(
            string xrefId,
            bool isOk) {
            XrefId = xrefId;
            IsOk = isOk;
        }
    }
}
