using System;
using System.Net;
using System.Net.Mail;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Sender's email address and credentials
            string senderEmail = "sender@example.com";
            string senderPassword = "your_password";

            // Recipient's email address
            string recipientEmail = "recipient@example.com";

            // Create a new MailMessage object
            MailMessage mail = new MailMessage(senderEmail, recipientEmail);

            // Set the subject and body of the email
            mail.Subject = "Email with Attachment";
            mail.Body = "Please find the attached file.";

            // Create an attachment
            string fname = "file.txt";
            string currentDirectory = Directory.GetCurrentDirectory();
            string projectRoot = Directory.GetParent(currentDirectory)?.Parent?.Parent?.FullName ?? "./";
            string filePath = Path.Combine(projectRoot, fname);

            Console.WriteLine($"Looking for file at: {filePath}");

            if (File.Exists(filePath))
            {
                Console.WriteLine("File found!");
                // Use the file
            }
            else
            {
                Console.WriteLine("File not found!");
            }
            Attachment attachment = new Attachment(filePath);

            // Add the attachment to the email
            mail.Attachments.Add(attachment);

            // Configure the SMTP client
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);

            // Send the email
            smtpClient.Send(mail);

            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}