using ReactiveDomain.Messaging;

namespace ProcessManagerViewer.Domains;

public static partial class AclRequests {
    public class CreateContactReq : Command {
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;
        public readonly CommandSource Source;

        public CreateContactReq(
            string xrefId,
            string firstName,
            string lastName,
            string email,
            CommandSource source) {
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Source = source;
        }
    }

    public class CreateCrmContactReq : Command {
        public readonly string XrefId;
        public readonly string FirstName;
        public readonly string LastName;
        public readonly string Email;
        public readonly CommandSource Source;

        public CreateCrmContactReq(
            string xrefId,
            string firstName,
            string lastName,
            string email,
            CommandSource source) {
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Source = source;
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
        public readonly CommandSource Source;

        public CreateErpContactReq(
            string xrefId,
            string firstName,
            string lastName,
            string email,
            CommandSource source) {
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Source = source;
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
        public readonly CommandSource Source;

        public ArchiveCrmContactReq(
            string xrefId,
            CommandSource source) {
            XrefId = xrefId;
            Source = source;
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
        public readonly CommandSource Source;

        public ArchiveErpContactReq(
            string xrefId,
            CommandSource source) {
            XrefId = xrefId;
            Source = source;
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
        public readonly CommandSource Source;

        public UpdateErpContactDetailsReq(
            string xrefId,
            string firstName,
            string lastName,
            string email,
            CommandSource source) {
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Source = source;
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
        public readonly CommandSource Source;

        public UpdateCrmContactDetailsReq(
            string xrefId,
            string firstName,
            string lastName,
            string email,
            CommandSource source) {
            XrefId = xrefId;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Source = source;
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
