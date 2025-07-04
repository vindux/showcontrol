using System.Net;
using System.Net.Sockets;
using System.Text;
using ShowControl.Models;

namespace ShowControl.Services
{
    public class OscService
    {
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _endPoint;
        private readonly JsonService _jsonService;

        public OscService(string host = "127.0.0.1", int port = 8000)
        {
            _udpClient = new UdpClient();
            _endPoint = new IPEndPoint(IPAddress.Parse(host), port);
            _jsonService = new JsonService();
        }

        public void SendSlideMessage(Slide slide)
        {
            try
            {
                var templateDataJson = _jsonService.SerializeTemplateData(slide.TemplateData);
                var message = CreateOscMessage("/slide", slide.Title, templateDataJson);
                SendMessage(message);
                
                Console.WriteLine($"OSC Message sent for slide: {slide.Title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OSC message for slide: {ex.Message}");
            }
        }

        public void SendCustomButtonMessage(CustomButton customButton)
        {
            try
            {
                var templateDataJson = _jsonService.SerializeTemplateData(customButton.TemplateData);
                var message = CreateOscMessage("/custom", customButton.Title, templateDataJson);
                SendMessage(message);
                
                Console.WriteLine($"OSC Message sent for custom button: {customButton.Title}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OSC message for custom button: {ex.Message}");
            }
        }

        public void SendTakeMessage()
        {
            try
            {
                var message = CreateOscMessage("/take");
                SendMessage(message);
                
                Console.WriteLine("OSC Message sent for TAKE button");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending OSC message for TAKE: {ex.Message}");
            }
        }

        private byte[] CreateOscMessage(string address, params string[] args)
        {
            var message = new List<byte>();
            
            // OSC Address
            var addressBytes = Encoding.ASCII.GetBytes(address);
            message.AddRange(addressBytes);
            
            // Null terminator for address
            message.Add(0);
            
            // Padding to make address length multiple of 4
            while (message.Count % 4 != 0)
            {
                message.Add(0);
            }
            
            // Type tag string
            var typeTag = "," + new string('s', args.Length); // 's' for each string argument
            var typeTagBytes = Encoding.ASCII.GetBytes(typeTag);
            message.AddRange(typeTagBytes);
            
            // Null terminator for type tag
            message.Add(0);
            
            // Padding to make type tag length multiple of 4
            while (message.Count % 4 != 0)
            {
                message.Add(0);
            }
            
            // Arguments
            foreach (var arg in args)
            {
                var argBytes = Encoding.UTF8.GetBytes(arg);
                message.AddRange(argBytes);
                
                // Null terminator for argument
                message.Add(0);
                
                // Padding to make argument length multiple of 4
                while (message.Count % 4 != 0)
                {
                    message.Add(0);
                }
            }
            
            return message.ToArray();
        }

        private void SendMessage(byte[] message)
        {
            _udpClient.Send(message, message.Length, _endPoint);
        }

        public void Dispose()
        {
            _udpClient?.Close();
            _udpClient?.Dispose();
        }
    }
}