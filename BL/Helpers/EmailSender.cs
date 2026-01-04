using DO;
using System;
using System.Net.Mail;

namespace Helpers;

internal static class EmailSender
{
    /// <summary>
    /// Best-effort email sender. Swallows exceptions to avoid breaking main flow.
    /// Configure SMTP pickup or relay as needed; defaults to localhost pickup directory.
    /// </summary>
    internal static void TrySendOrderAssignedEmail(Courier courier, Order order)
    {
        try
        {
            // If no email, skip
            if (string.IsNullOrWhiteSpace(courier.Email)) return;

            // Basic mail body
            string subject = $"New order assigned #{order.Id}";
            string body =
                $"Hello {courier.Name},\n\n" +
                "A new order has been assigned to you.\n" +
                $"Order: #{order.Id}\n" +
                $"Customer: {order.CustomerName}\n" +
                $"Phone: {order.CustomerPhone}\n" +
                $"Address: {order.Address}\n" +
                $"Type: {order.OrderType}\n" +
                $"Order time: {order.OrderTime}\n" +
                "\nThank you.\n";

            using var mail = new MailMessage("noreply@company.local", courier.Email.Trim(), subject, body);

            // Default SmtpClient: configure to pickup directory to avoid network dependency
            using var smtp = new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = System.IO.Path.Combine(AppContext.BaseDirectory, "mails")
            };

            // Ensure directory exists
            System.IO.Directory.CreateDirectory(smtp.PickupDirectoryLocation);

            smtp.Send(mail);
        }
        catch
        {
            // ignore all exceptions (best effort)
        }
    }
}
