using System.Net;
using System.Net.Sockets;
using System.Text;
using ShowControl.Models;

namespace ShowControl.Services
{
    /// <summary>
    /// Service for sending OSC (Open Sound Control) messages via UDP
    /// </summary>
    public class OscService
    {
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _endPoint;
        private readonly JsonService _jsonService;

        /// <summary>
        /// Initializes a new instance of the OscService class
        /// </summary>
        /// <param name="host">The target host IP address (default: 127.0.0.1)</param>
        /// <param name="port">The target port number (default: 8000)</param>
        public OscService(string host = "127.0.0.1", int port = 8000)
        {
            _udpClient = new UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            _jsonService = new JsonService();
        }

        /// <summary>
        /// Sends an OSC message for a slide button click
        /// </summary>
        /// <param name="slide">The slide data containing title and template information</param>
        public void SendSlideMessage(Slide slide)
        {
            try
            {
                string templateDataJson = _jsonService.SerializeTemplateData(slide.TemplateData);
                byte[] message = CreateOscMessage("/slide", slide.Title, templateDataJson);
                SendMessage(message);
                
                Console.WriteLine($"OSC Message sent for slide: {slide.Title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OSC message for slide: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends an OSC message for a custom button click
        /// </summary>
        /// <param name="customButton">The custom button data containing title and template information</param>
        public void SendCustomButtonMessage(CustomButton customButton)
        {
            try
            {
                string templateDataJson = _jsonService.SerializeTemplateData(customButton.TemplateData);
                byte[] message = CreateOscMessage("/custom", customButton.Title, templateDataJson);
                SendMessage(message);
                
                Console.WriteLine($"OSC Message sent for custom button: {customButton.Title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OSC message for custom button: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends an OSC message for the TAKE button click
        /// </summary>
        public void SendTakeMessage()
        {
            try
            {
                byte[] message = CreateOscMessage("/take");
                SendMessage(message);
                
                Console.WriteLine("OSC Message sent for TAKE button");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OSC message for TAKE: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a properly formatted OSC message byte array
        /// </summary>
        /// <param name="address">The OSC address pattern (e.g., "/slide", "/custom", "/take")</param>
        /// <param name="args">Optional string arguments to include in the message</param>
        /// <returns>Byte array containing the formatted OSC message</returns>
        private byte[] CreateOscMessage(string address, params string[] args)
        {
            var message = new List<byte>();
            
            byte[] addressBytes = Encoding.ASCII.GetBytes(address);
            message.AddRange(addressBytes);
            
            message.Add(0);
            
            while (message.Count % 4 != 0)
            {
                message.Add(0);
            }
            
            string typeTag = "," + new string('s', args.Length); // 's' for each string argument
            byte[] typeTagBytes = Encoding.ASCII.GetBytes(typeTag);
            message.AddRange(typeTagBytes);
            
            message.Add(0);
            
            while (message.Count % 4 != 0)
            {
                message.Add(0);
            }
            
            foreach (string arg in args)
            {
                byte[] argBytes = Encoding.UTF8.GetBytes(arg);
                message.AddRange(argBytes);
                
                message.Add(0);
                
                while (message.Count % 4 != 0)
                {
                    message.Add(0);
                }
            }
            
            return message.ToArray();
        }

        /// <summary>
        /// Sends a byte array message via UDP to the configured endpoint
        /// </summary>
        /// <param name="message">The message byte array to send</param>
        private void SendMessage(byte[] message)
        {
            _udpClient.Send(message, message.Length, _endPoint);
        }

        /// <summary>
        /// Disposes of the UDP client and releases network resources
        /// </summary>
        public void Dispose()
        {
            _udpClient.Close();
            _udpClient.Dispose();
        }
    }
}