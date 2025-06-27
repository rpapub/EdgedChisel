using System;
using System.Collections.Generic;
using System.IO; // For Path
using System.Net.Mail; // For MailAddress
using System.Security.Cryptography; // For SHA256
using System.Text; // For StringBuilder
using Newtonsoft.Json;
using UiPath.MicrosoftOffice365.Models; // For Office365Message

namespace Cprima.RpaPub.EdgedChisel
{
    public class Mail
    {
        public string MessageId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public MailAddress From { get; set; }
        public List<MailAddress> To { get; set; }
        public List<MailAddress> CC { get; set; }
        public List<GenericAttachment> Attachments { get; set; }

        public string MailboxAccount { get; set; }
        public string SourceFolder { get; set; }
        public string CurrentFolder { get; set; }
        public string TempProcessingFolder { get; set; }
        public string TargetFolderSuccess { get; set; }
        public string TargetFolderHandover { get; set; }

        // Placeholder properties for orchestration
        public bool IsAutomatable { get; set; } = false;
        public bool IsPotentialDuplicate { get; set; } = false;
        public bool IsRetry { get; set; } = false;
        public bool WasMoved { get; set; } = false;
        public string ProcessingStatus { get; set; } = "Pending";
        public string FailureReason { get; set; } = null;
        public string Checksum { get; set; }


        public Mail()
        {
            To = new List<MailAddress>();
            CC = new List<MailAddress>();
            Attachments = new List<GenericAttachment>();

            ProcessingStatus = MailProcessingState.Pending.ToString();
            IsAutomatable = false;
            IsPotentialDuplicate = false;
            IsRetry = false;
            WasMoved = false;
            FailureReason = null;
            MailboxAccount = null;
            SourceFolder = null;
            CurrentFolder = null;
            TempProcessingFolder = null;
            TargetFolderSuccess = null;
            TargetFolderHandover = null;
        }

        public enum MailProcessingState
        {
            Pending,       // Awaiting handling
            Processing,    // In progress
            Completed,     // Finished successfully
            Failed,        // Permanent error
            Skipped,       // Intentionally ignored
            Escalated      // Handed off to manual handling
        }

        // Overloaded method to populate from Office365Message without filtering
        public static List<Mail> PopulateFromOffice365Messages(Office365Message[] messages, Dictionary<string, object> config)
        {
            return PopulateFromOffice365Messages(messages, config, null);
        }

        //
        public static List<Mail> PopulateFromOffice365Messages(
            Office365Message[] messages,
            Dictionary<string, object> config,
            string extension = null)
        {
            var genericMessages = new List<Mail>();

            string GetConfigValue(string key) =>
                config != null && config.ContainsKey(key) && config[key] != null ? config[key].ToString() : null;

            string mailboxAccount = GetConfigValue("MailboxAccount");
            string sourceFolder = GetConfigValue("MailboxFolder");
            string targetSuccess = GetConfigValue("TargetFolderSuccess");
            string targetHandover = GetConfigValue("TargetFolderHandover");

            foreach (var msg in messages)
            {
                var genericMessage = new Mail
                {
                    MessageId = msg.MessageId,
                    Subject = msg.Subject,
                    Body = msg.Body,
                    From = msg.From,
                    To = new List<MailAddress>(msg.To),
                    CC = new List<MailAddress>(msg.CC),
                    MailboxAccount = mailboxAccount,
                    SourceFolder = sourceFolder,
                    CurrentFolder = sourceFolder,
                    TargetFolderSuccess = targetSuccess,
                    TargetFolderHandover = targetHandover
                };

                foreach (var attachment in msg.Attachments)
                {
                    bool include = string.IsNullOrEmpty(extension) ||
                                   Path.GetExtension(attachment.Name)
                                       .Equals($".{extension}", StringComparison.OrdinalIgnoreCase);

                    if (!include) continue;

                    using var ms = new MemoryStream();
                    attachment.ContentStream.CopyTo(ms);
                    var contentBytes = ms.ToArray();

                    genericMessage.Attachments.Add(new GenericAttachment
                    {
                        Name = attachment.Name,
                        ContentType = attachment.ContentType?.MediaType,
                        ContentBytes = contentBytes
                    });
                }

                genericMessage.Checksum = genericMessage.GenerateChecksum();
                genericMessages.Add(genericMessage);
            }

            return genericMessages;
        }


        // Overloaded method to populate from MailMessage without filtering
        public static List<Mail> PopulateFromSystemNetMailMessages(List<MailMessage> messages, Dictionary<string, object> config)
        {
            return PopulateFromSystemNetMailMessages(messages, config, null);
        }

        public static List<Mail> PopulateFromSystemNetMailMessages(
            List<MailMessage> messages,
            Dictionary<string, object> config,
            string extension = null)
        {
            var genericMessages = new List<Mail>();

            string GetConfigValue(string key) =>
                config != null && config.ContainsKey(key) && config[key] != null ? config[key].ToString() : null;

            string mailboxAccount = GetConfigValue("MailboxAccount");
            string sourceFolder = GetConfigValue("MailboxFolder");
            string targetSuccess = GetConfigValue("TargetFolderSuccess");
            string targetHandover = GetConfigValue("TargetFolderHandover");

            foreach (var msg in messages)
            {
                var genericMessage = new Mail
                {
                    MessageId = msg.Headers["Message-ID"],
                    Subject = msg.Subject,
                    Body = msg.Body,
                    From = msg.From,
                    To = new List<MailAddress>(),
                    CC = new List<MailAddress>(),
                    MailboxAccount = mailboxAccount,
                    SourceFolder = sourceFolder,
                    CurrentFolder = sourceFolder,
                    TargetFolderSuccess = targetSuccess,
                    TargetFolderHandover = targetHandover
                };

                foreach (MailAddress to in msg.To)
                    genericMessage.To.Add(to);

                foreach (MailAddress cc in msg.CC)
                    genericMessage.CC.Add(cc);

                foreach (var attachment in msg.Attachments)
                {
                    bool include = string.IsNullOrEmpty(extension) ||
                                   Path.GetExtension(attachment.Name)
                                       .Equals($".{extension}", StringComparison.OrdinalIgnoreCase);

                    if (!include) continue;

                    using var ms = new MemoryStream();
                    attachment.ContentStream.CopyTo(ms);
                    var contentBytes = ms.ToArray();

                    genericMessage.Attachments.Add(new GenericAttachment
                    {
                        Name = attachment.Name,
                        ContentType = attachment.ContentType?.MediaType,
                        ContentBytes = contentBytes
                    });
                }

                genericMessage.Checksum = genericMessage.GenerateChecksum();
                genericMessages.Add(genericMessage);
            }

            return genericMessages;
        }

        /// <summary>
        /// Saves all attachments of this message into a unique subfolder.
        /// </summary>
        public void SaveAttachments(string baseDirectory)
        {
            string root = string.IsNullOrWhiteSpace(baseDirectory)
                ? throw new ArgumentNullException(nameof(baseDirectory))
                : baseDirectory;

            string messageFolder = Path.Combine(root, Checksum ?? Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(messageFolder);

            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var attachment in Attachments)
            {
                string safeName = Path.GetFileName(attachment.Name) ?? "unnamed";
                string fullPath = Path.Combine(messageFolder, safeName);

                int counter = 1;
                while (usedNames.Contains(safeName) || File.Exists(fullPath))
                {
                    string nameOnly = Path.GetFileNameWithoutExtension(attachment.Name);
                    string ext = Path.GetExtension(attachment.Name);
                    safeName = $"{nameOnly}_{counter++}{ext}";
                    fullPath = Path.Combine(messageFolder, safeName);
                }

                File.WriteAllBytes(fullPath, attachment.ContentBytes);
                attachment.TempFilePath = fullPath;
                usedNames.Add(safeName);
            }

            TempProcessingFolder = messageFolder;
        }


        /// <summary>
        /// Saves all attachments for a collection of messages using the specified or default cache.
        /// Cleans the root only once before starting.
        /// </summary>
        public static void SaveAllAttachments(IEnumerable<Mail> messages, string baseDirectory = null)
        {
            string root = string.IsNullOrWhiteSpace(baseDirectory)
                ? GetDefaultAttachmentsCacheRoot()
                : baseDirectory;

            if (string.IsNullOrWhiteSpace(baseDirectory))
                CleanDefaultAttachmentsCache();

            foreach (var message in messages)
                message.SaveAttachments(root);
        }

        public static void SaveAllAttachments(IEnumerable<Mail> messages)
        {
            SaveAllAttachments(messages, null);
        }

        // Method to generate a checksum for the mail message
        public string GenerateChecksum()
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Combine properties into a single string
                string data = $"{MessageId}|{Subject}|{Body}|{From}|{string.Join(",", To)}|{string.Join(",", CC)}|{string.Join(",", Attachments)}";

                // Compute the hash
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));

                // Convert byte array to a hexadecimal string
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2")); // Convert to hex
                }
                return builder.ToString();
            }
        }

        public static string ComputeCombinedChecksum(IEnumerable<Mail> messages)
        {
            using var sha = SHA256.Create();
            var builder = new StringBuilder();

            foreach (var msg in messages)
                builder.Append(msg.GenerateChecksum());

            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()));
            return Convert.ToHexString(hashBytes);
        }

        // Override ToString method to return a string representation of the mail message
        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("== GenericMailMessage ==");
            sb.AppendLine($"MessageId: {MessageId}");
            sb.AppendLine($"Subject: {Subject}");
            sb.AppendLine($"From: {From}");
            sb.AppendLine($"To: {string.Join(", ", To)}");
            sb.AppendLine($"CC: {string.Join(", ", CC)}");
            sb.AppendLine($"Body: {(string.IsNullOrWhiteSpace(Body) ? "[Empty]" : Body.Substring(0, Math.Min(200, Body.Length)) + (Body.Length > 200 ? "..." : ""))}");
            sb.AppendLine($"Attachments: {Attachments?.Count ?? 0}");
            foreach (var att in Attachments)
            {
                sb.AppendLine($"  - {att.Name} ({att.ContentType}, {att.Size} bytes)");
                if (!string.IsNullOrWhiteSpace(att.TempFilePath))
                    sb.AppendLine($"    Path: {att.TempFilePath}");
            }

            sb.AppendLine("--- Routing Info ---");
            sb.AppendLine($"SourceFolder: {SourceFolder}");
            sb.AppendLine($"CurrentFolder: {CurrentFolder}");
            sb.AppendLine($"TempProcessingFolder: {TempProcessingFolder}");
            sb.AppendLine($"TargetFolderSuccess: {TargetFolderSuccess}");
            sb.AppendLine($"TargetFolderHandover: {TargetFolderHandover}");

            sb.AppendLine("--- Processing Flags ---");
            sb.AppendLine($"IsAutomatable: {IsAutomatable}");
            sb.AppendLine($"IsPotentialDuplicate: {IsPotentialDuplicate}");
            sb.AppendLine($"IsRetry: {IsRetry}");
            sb.AppendLine($"WasMoved: {WasMoved}");
            sb.AppendLine($"ProcessingStatus: {ProcessingStatus}");
            sb.AppendLine($"FailureReason: {FailureReason}");

            sb.AppendLine($"Checksum: {Checksum}");

            return sb.ToString();
        }

        public string ToJson(bool indented = false)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = indented ? Formatting.Indented : Formatting.None
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static Mail FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Mail>(json);
        }

        /// <summary>
        /// Deletes all contents in the default attachments cache.
        /// Call explicitly before saving if needed.
        /// </summary>
        public static void CleanDefaultAttachmentsCache()
        {
            string path = GetDefaultAttachmentsCacheRoot();
            if (Directory.Exists(path))
            {
                try
                {
                    Directory.Delete(path, recursive: true);
                }
                catch (IOException) { /* Optionally log */ }
                catch (UnauthorizedAccessException) { /* Optionally log */ }
            }
        }

        /// <summary>
        /// Returns the default root path for attachment cache.
        /// </summary>
        public static string GetDefaultAttachmentsCacheRoot()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Cprima.RpaPub.EdgedChisel.AttachmentsCache"
            );
        }
    }

    public class GenericAttachment
    {
        public string Name { get; set; }
        public string ContentType { get; set; }
        public byte[] ContentBytes { get; set; }

        // Optional:
        public string TempFilePath { get; set; } // Filled if saved to disk
        public long Size => ContentBytes?.Length ?? 0;
    }

}